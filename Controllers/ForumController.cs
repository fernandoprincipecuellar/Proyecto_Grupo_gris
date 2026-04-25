using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Proyecto_Grupo_gris.Data;
using Proyecto_Grupo_gris.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text.Json;

namespace Proyecto_Grupo_gris.Controllers
{
    public class ForumController(ApplicationDbContext context, IWebHostEnvironment environment, IDistributedCache cache) : Controller
    {
        private const string ForumCacheKey = "forum_posts";
        private static readonly DistributedCacheEntryOptions CacheOptions = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
            SlidingExpiration = TimeSpan.FromMinutes(2)
        };

        public async Task<IActionResult> Index()
        {
            List<ForumPost>? posts = null;

            // Intentar obtener del caché Redis
            try
            {
                var cachedData = await cache.GetStringAsync(ForumCacheKey);
                if (!string.IsNullOrEmpty(cachedData))
                {
                    posts = JsonSerializer.Deserialize<List<ForumPost>>(cachedData);
                }
            }
            catch
            {
                // Si Redis falla, seguimos con la DB
            }

            // Si no hay caché, consultar la DB y guardar en caché
            if (posts == null)
            {
                posts = await context.ForumPosts
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                try
                {
                    var serialized = JsonSerializer.Serialize(posts);
                    await cache.SetStringAsync(ForumCacheKey, serialized, CacheOptions);
                }
                catch
                {
                    // Si Redis falla al escribir, no pasa nada
                }
            }

            // Necesitamos el UserId para el botón de eliminar, 
            // así que pasamos los posts con esa info
            return View(posts);
        }

        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ForumPost post, IFormFile? imageFile)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            post.UserId = userId;

            if (string.IsNullOrEmpty(post.Location))
            {
                post.Location = "Ubicación actual";
            }

            if (imageFile != null && imageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(environment.WebRootPath, "images/forum");
                
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(imageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }
                post.ImageUrl = "/images/forum/" + uniqueFileName;
            }

            post.CreatedAt = DateTime.UtcNow;
            context.Add(post);
            await context.SaveChangesAsync();

            // Invalidar caché para que el nuevo post aparezca
            await InvalidateForumCacheAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await context.ForumPosts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (post.UserId != userId)
            {
                return Forbid();
            }

            if (!string.IsNullOrEmpty(post.ImageUrl) && !post.ImageUrl.Contains("river.png") && !post.ImageUrl.Contains("dump.png") && !post.ImageUrl.Contains("air.png") && !post.ImageUrl.Contains("trees.png"))
            {
                var filePath = Path.Combine(environment.WebRootPath, post.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            context.ForumPosts.Remove(post);
            await context.SaveChangesAsync();

            // Invalidar caché tras eliminar
            await InvalidateForumCacheAsync();

            return RedirectToAction(nameof(Index));
        }

        private async Task InvalidateForumCacheAsync()
        {
            try
            {
                await cache.RemoveAsync(ForumCacheKey);
            }
            catch
            {
                // Si Redis no está disponible, no pasa nada
            }
        }
    }
}
