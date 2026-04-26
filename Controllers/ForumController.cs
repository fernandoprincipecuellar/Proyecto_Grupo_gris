using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto_Grupo_gris.Data;
using Proyecto_Grupo_gris.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Proyecto_Grupo_gris.Controllers
{
    public class ForumController(ApplicationDbContext context, IWebHostEnvironment environment) : Controller
    {
        public async Task<IActionResult> Index()
        {
            // LEER COOKIE: Obtener la preferencia de diseño (Grid o List)
            ViewBag.ViewMode = Request.Cookies["ForumViewMode"] ?? "Grid";

            var posts = await context.ForumPosts
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
            return View(posts);
        }

        // ACCIÓN PARA COOKIES: Cambiar el modo de vista
        public IActionResult SetViewMode(string mode)
        {
            CookieOptions option = new CookieOptions();
            option.Expires = DateTime.Now.AddDays(30); // Guardar por 30 días
            Response.Cookies.Append("ForumViewMode", mode, option);

            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        public IActionResult Create()
        {
            // Recuperar el último tipo de reporte usado de la SESIÓN y pasarlo al Modelo
            var model = new ForumPost
            {
                ReportType = HttpContext.Session.GetString("LastReportType")
            };
            return View(model);
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

            // GUARDAR EN SESIÓN: Recordar el tipo de reporte para el próximo post
            if (!string.IsNullOrEmpty(post.ReportType))
            {
                HttpContext.Session.SetString("LastReportType", post.ReportType);
            }

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
            return RedirectToAction(nameof(Index));
        }
    }
}
