using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto_Grupo_gris.Data;
using Proyecto_Grupo_gris.Models;

namespace Proyecto_Grupo_gris.Controllers
{
    public class ForumController(ApplicationDbContext context, IWebHostEnvironment environment) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var posts = await context.ForumPosts.OrderByDescending(p => p.CreatedAt).ToListAsync();
            return View(posts);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ForumPost post, IFormFile? imageFile)
        {
            // For location, if it's empty, use a default for the mockup look
            if (string.IsNullOrEmpty(post.Location))
            {
                post.Location = "Ubicación actual";
            }

            if (imageFile != null && imageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(environment.WebRootPath, "images/forum");
                
                // Ensure directory exists
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
            return RedirectToAction(nameof(Index));
        }
    }
}
