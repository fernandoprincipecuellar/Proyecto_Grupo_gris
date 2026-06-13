using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Proyecto_Grupo_gris.Api.Services.Interfaces;
using Proyecto_Grupo_gris.Api.DTOs.Forum;
using Proyecto_Grupo_gris.Data;
using Proyecto_Grupo_gris.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Text.Json;

namespace Proyecto_Grupo_gris.Controllers
{
    public class ForumController(ApplicationDbContext context, IWebHostEnvironment environment, IDistributedCache cache, ICloudinaryService cloudinaryService) : Controller
    {
        private readonly ICloudinaryService _cloudinaryService = cloudinaryService;
        private const string ForumCacheKey = "forum_posts";
        private static readonly DistributedCacheEntryOptions CacheOptions = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
            SlidingExpiration = TimeSpan.FromMinutes(2)
        };

        public async Task<IActionResult> Index()
        {
            List<ForumPostDto>? posts = null;

            // Intentar obtener del caché Redis
            try
            {
                var cachedData = await cache.GetStringAsync(ForumCacheKey);
                if (!string.IsNullOrEmpty(cachedData))
                {
                    posts = JsonSerializer.Deserialize<List<ForumPostDto>>(cachedData);
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
                    .AsNoTracking()
                    .OrderByDescending(p => p.CreatedAt)
                    .Select(p => new ForumPostDto
                    {
                        Id = p.Id,
                        ReportType = p.ReportType ?? string.Empty,
                        Description = p.Description ?? string.Empty,
                        ImageUrl = p.ImageUrl,
                        Location = p.Location,
                        Urgency = p.Urgency,
                        UserId = p.UserId ?? string.Empty,
                        UserName = p.User != null ? p.User.UserName ?? string.Empty : string.Empty,
                        CreatedAt = p.CreatedAt,
                        CommentsCount = p.CommentsCount,
                        LikesCount = p.LikesCount
                    })
                    .ToListAsync();

                try
                {
                    var jsonOptions = new JsonSerializerOptions
                    {
                        ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
                    };
                    var serialized = JsonSerializer.Serialize(posts, jsonOptions);
                    await cache.SetStringAsync(ForumCacheKey, serialized, CacheOptions);
                }
                catch
                {
                    // Si Redis falla al escribir, no pasa nada
                }
            }

            // Obtener el último foro revisado de la sesión
            try
            {
                var lastVisitedJson = HttpContext.Session.GetString("LastVisitedPost");
                if (!string.IsNullOrEmpty(lastVisitedJson))
                {
                    ViewBag.LastVisitedPost = JsonSerializer.Deserialize<ForumPostDto>(lastVisitedJson);
                }
            }
            catch
            {
                // Session no disponible, ignorar
            }

            return View(posts);
        }

        public async Task<IActionResult> Details(int id)
        {
            var post = await context.ForumPosts
                .Include(p => p.Comments).ThenInclude(c => c.User)
                .Include(p => p.Likes)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (post == null)
            {
                return NotFound();
            }

            // Guardar en sesión como el último visitado (Solo las propiedades necesarias)
            var lastVisited = new ForumPostDto
            {
                Id = post.Id,
                ReportType = post.ReportType ?? string.Empty,
                Description = post.Description ?? string.Empty,
                Location = post.Location,
                ImageUrl = post.ImageUrl,
                UserId = post.UserId ?? string.Empty,
                UserName = post.User != null ? post.User.UserName ?? string.Empty : string.Empty,
                CreatedAt = post.CreatedAt,
                CommentsCount = post.CommentsCount,
                LikesCount = post.LikesCount
            };

            var serialized = JsonSerializer.Serialize(lastVisited);
            HttpContext.Session.SetString("LastVisitedPost", serialized);

            return View(post);
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

            // Análisis de Sentimiento con ML.NET para definir urgencia del Post
            var sentiment = Proyecto_Grupo_gris.ML.SentimentAnalysis.SentimentAnalysisConsumption.Predict(post.Description ?? "");
            post.Urgency = sentiment.Sentiment == "Negativo" ? "Alta" : (sentiment.Sentiment == "Neutro" ? "Media" : "Baja");
            Console.WriteLine($"[ML.NET] Post analizado: Sentimiento={sentiment.Sentiment}, Confianza={sentiment.ConfidencePercentage}% -> Urgencia={post.Urgency}");


            if (imageFile != null && imageFile.Length > 0)
            {
                // Intentar Cloudinary primero
                try
                {
                    var uploadedUrl = await _cloudinaryService.UploadImageAsync(imageFile, "forum_posts");
                    if (!string.IsNullOrEmpty(uploadedUrl))
                    {
                        post.ImageUrl = uploadedUrl;
                    }
                }
                catch
                {
                    // Cloudinary falló, guardar localmente
                }

                // Si Cloudinary no funcionó, guardar archivo local
                if (string.IsNullOrEmpty(post.ImageUrl))
                {
                    var uploadsDir = Path.Combine(environment.WebRootPath, "uploads", "forum_posts");
                    Directory.CreateDirectory(uploadsDir);

                    var ext = Path.GetExtension(imageFile.FileName);
                    var fileName = $"{Guid.NewGuid()}{ext}";
                    var filePath = Path.Combine(uploadsDir, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    post.ImageUrl = $"/uploads/forum_posts/{fileName}";
                }
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

            if (!string.IsNullOrEmpty(post.ImageUrl) && post.ImageUrl.StartsWith("/uploads/"))
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
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Like(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var post = await context.ForumPosts.Include(p => p.Likes).FirstOrDefaultAsync(p => p.Id == id);
            
            if (post != null && userId != null)
            {
                var existingLike = post.Likes.FirstOrDefault(l => l.UserId == userId);
                if (existingLike == null)
                {
                    var newLike = new ForumLike { PostId = id, UserId = userId };
                    context.ForumLikes.Add(newLike);
                    post.LikesCount++;
                    await context.SaveChangesAsync();
                    await InvalidateForumCacheAsync();
                }
            }
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int id, string commentText)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var post = await context.ForumPosts.FindAsync(id);
            
            if (post != null && !string.IsNullOrWhiteSpace(commentText))
            {
                // Análisis de sentimiento para el comentario
                var sentimentResult = Proyecto_Grupo_gris.ML.SentimentAnalysis.SentimentAnalysisConsumption.Predict(commentText);
                
                var newComment = new ForumComment
                {
                    PostId = id,
                    UserId = userId,
                    Text = commentText,
                    Sentiment = sentimentResult.Sentiment,
                    SentimentConfidence = sentimentResult.ConfidencePercentage
                };
                
                context.ForumComments.Add(newComment);
                post.CommentsCount++;
                await context.SaveChangesAsync();
                await InvalidateForumCacheAsync();
            }
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearAll()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var comments = await context.ForumComments.ToListAsync();
            context.ForumComments.RemoveRange(comments);

            var likes = await context.ForumLikes.ToListAsync();
            context.ForumLikes.RemoveRange(likes);

            var posts = await context.ForumPosts.ToListAsync();
            context.ForumPosts.RemoveRange(posts);

            await context.SaveChangesAsync();
            await InvalidateForumCacheAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
