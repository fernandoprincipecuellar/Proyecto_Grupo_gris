using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Proyecto_Grupo_gris.Models;

namespace Proyecto_Grupo_gris.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<ApplicationUser> AspNetUsers => Set<ApplicationUser>();
    public DbSet<IdentityRole> AspNetRoles => Set<IdentityRole>();
    public DbSet<IdentityUserRole<string>> AspNetUserRoles => Set<IdentityUserRole<string>>();
    public DbSet<IdentityUserClaim<string>> AspNetUserClaims => Set<IdentityUserClaim<string>>();
    public DbSet<IdentityUserLogin<string>> AspNetUserLogins => Set<IdentityUserLogin<string>>();
    public DbSet<IdentityRoleClaim<string>> AspNetRoleClaims => Set<IdentityRoleClaim<string>>();
    public DbSet<IdentityUserToken<string>> AspNetUserTokens => Set<IdentityUserToken<string>>();

    public DbSet<ForumPost> ForumPosts { get; set; }
    public DbSet<EcoRoute> EcoRoutes { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<ForumComment> ForumComments { get; set; }
    public DbSet<ForumLike> ForumLikes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ApplicationUser - Sin configurar HasKey porque AddIdentity lo hace automáticamente
        modelBuilder.Entity<ApplicationUser>(b =>
        {
            b.HasIndex(u => u.NormalizedUserName).HasDatabaseName("UserNameIndex").IsUnique();
            b.HasIndex(u => u.NormalizedEmail).HasDatabaseName("EmailIndex");
            b.ToTable("AspNetUsers");
            b.Property(u => u.ConcurrencyStamp).IsConcurrencyToken();
            b.HasMany(u => u.ForumPosts).WithOne(p => p.User).HasForeignKey(p => p.UserId);
            b.HasMany(u => u.EcoRoutes).WithOne(r => r.CreatedBy).HasForeignKey(r => r.CreatedById);
            b.HasMany(u => u.Comments).WithOne(c => c.Author).HasForeignKey(c => c.AuthorId);
        });

        // Roles
        modelBuilder.Entity<IdentityRole>(b =>
        {
            b.HasKey(r => r.Id);
            b.HasIndex(r => r.NormalizedName).HasDatabaseName("RoleNameIndex").IsUnique();
            b.ToTable("AspNetRoles");
            b.Property(r => r.ConcurrencyStamp).IsConcurrencyToken();
        });

        // UserRoles
        modelBuilder.Entity<IdentityUserRole<string>>(b =>
        {
            b.HasKey(ur => new { ur.UserId, ur.RoleId });
            b.HasOne<ApplicationUser>().WithMany().HasForeignKey(ur => ur.UserId).IsRequired();
            b.HasOne<IdentityRole>().WithMany().HasForeignKey(ur => ur.RoleId).IsRequired();
            b.ToTable("AspNetUserRoles");
        });

        // UserClaims
        modelBuilder.Entity<IdentityUserClaim<string>>(b =>
        {
            b.HasKey(uc => uc.Id);
            b.HasOne<ApplicationUser>().WithMany().HasForeignKey(uc => uc.UserId).IsRequired();
            b.ToTable("AspNetUserClaims");
        });

        // UserLogins
        modelBuilder.Entity<IdentityUserLogin<string>>(b =>
        {
            b.HasKey(ul => new { ul.LoginProvider, ul.ProviderKey });
            b.HasOne<ApplicationUser>().WithMany().HasForeignKey(ul => ul.UserId).IsRequired();
            b.ToTable("AspNetUserLogins");
        });

        // RoleClaims
        modelBuilder.Entity<IdentityRoleClaim<string>>(b =>
        {
            b.HasKey(rc => rc.Id);
            b.HasOne<IdentityRole>().WithMany().HasForeignKey(rc => rc.RoleId).IsRequired();
            b.ToTable("AspNetRoleClaims");
        });

        // UserTokens
        modelBuilder.Entity<IdentityUserToken<string>>(b =>
        {
            b.HasKey(ut => new { ut.UserId, ut.LoginProvider, ut.Name });
            b.HasOne<ApplicationUser>().WithMany().HasForeignKey(ut => ut.UserId).IsRequired();
            b.ToTable("AspNetUserTokens");
        });

        // Forum related entities
        modelBuilder.Entity<Comment>(b =>
        {
            b.HasKey(c => c.Id);
            b.HasOne(c => c.Author).WithMany(u => u.Comments).HasForeignKey(c => c.AuthorId).IsRequired();
            b.HasOne(c => c.EcoRoute).WithMany(r => r.Comments).HasForeignKey(c => c.EcoRouteId);
            b.HasOne(c => c.ForumPost).WithMany().HasForeignKey(c => c.ForumPostId);
            b.ToTable("Comments");
        });

        modelBuilder.Entity<EcoRoute>(b =>
        {
            b.HasKey(r => r.Id);
            b.HasOne(r => r.CreatedBy).WithMany(u => u.EcoRoutes).HasForeignKey(r => r.CreatedById).IsRequired();
            b.ToTable("EcoRoutes");
        });

        modelBuilder.Entity<ForumPost>(b =>
        {
            b.HasKey(p => p.Id);
            b.HasOne(p => p.User).WithMany(u => u.ForumPosts).HasForeignKey(p => p.UserId);
            b.ToTable("ForumPosts");
        });

        modelBuilder.Entity<ForumComment>(b =>
        {
            b.HasKey(c => c.Id);
            b.HasOne(c => c.Post).WithMany(p => p.Comments).HasForeignKey(c => c.PostId).IsRequired();
            b.HasOne(c => c.User).WithMany().HasForeignKey(c => c.UserId);
            b.ToTable("ForumComments");
        });

        modelBuilder.Entity<ForumLike>(b =>
        {
            b.HasKey(l => l.Id);
            b.HasOne(l => l.Post).WithMany(p => p.Likes).HasForeignKey(l => l.PostId).IsRequired();
            b.HasOne(l => l.User).WithMany().HasForeignKey(l => l.UserId);
            b.ToTable("ForumLikes");
        });
    }
}
