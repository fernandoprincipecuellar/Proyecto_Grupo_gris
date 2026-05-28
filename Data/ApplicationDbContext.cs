using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Proyecto_Grupo_gris.Models;

namespace Proyecto_Grupo_gris.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<ForumPost> ForumPosts { get; set; }
    public DbSet<EcoRoute> EcoRoutes { get; set; }
    public DbSet<Comment> Comments { get; set; }
}
