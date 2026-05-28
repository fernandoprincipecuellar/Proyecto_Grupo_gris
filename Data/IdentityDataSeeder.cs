using Microsoft.AspNetCore.Identity;
using Proyecto_Grupo_gris.Models;

namespace Proyecto_Grupo_gris.Data;

public static class IdentityDataSeeder
{
    private const string AdminEmailKey = "AdminSettings:Email";
    private const string AdminPasswordKey = "AdminSettings:Password";

    public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        var roles = new[] { "Admin", "User" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    public static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
    {
        var email = configuration[AdminEmailKey] ?? "admin@ecorut.local";
        var password = configuration[AdminPasswordKey] ?? "Admin123!";

        var admin = await userManager.FindByEmailAsync(email);
        if (admin != null)
        {
            if (!await userManager.IsInRoleAsync(admin, "Admin"))
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
            return;
        }

        admin = new ApplicationUser
        {
            UserName = email,
            Email = email,
            Nombre = "Administrador EcoRut",
            City = "EcoRut City",
            Country = "EcoRut",
            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(admin, password);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}
