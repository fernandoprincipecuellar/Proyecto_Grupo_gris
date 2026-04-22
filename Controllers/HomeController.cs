using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Proyecto_Grupo_gris.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Proyecto_Grupo_gris.Controllers;

public class HomeController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;

    public HomeController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public IActionResult Index()
    {
        var model = new DashboardViewModel
        {
            ReciclajeTotalKg = 124,
            Crecimiento = "+15%",
            Entregas = 48,
            Puntos = 2450,
            RecolectorNombre = "Camión Recolector",
            RecolectorDireccion = "Calle Principal 123",
            Distancia = "A 500 Metros de tu hogar",
            Co2AhoradoKg = 89,
            ArbolesSalvados = 12,
            AguaAhorradaL = 340,
        };

        return View(model);
    }

    [Microsoft.AspNetCore.Authorization.Authorize]
    public IActionResult Profile()
    {
        return View();
    }

    [Microsoft.AspNetCore.Authorization.Authorize]
    public IActionResult EditProfile()
    {
        return View();
    }

    [HttpPost]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public async Task<IActionResult> EditProfile(string fullName, string pictureUrl, string phone, string city, string bio)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var claims = await _userManager.GetClaimsAsync(user);
        
        async Task UpdateClaim(string type, string value)
        {
            var claim = claims.FirstOrDefault(c => c.Type == type);
            if (claim != null) await _userManager.RemoveClaimAsync(user, claim);
            if (!string.IsNullOrEmpty(value)) await _userManager.AddClaimAsync(user, new Claim(type, value));
        }

        await UpdateClaim("FullName", fullName);
        await UpdateClaim("Picture", pictureUrl);
        await UpdateClaim("Phone", phone);
        await UpdateClaim("City", city);
        await UpdateClaim("Bio", bio);

        await _signInManager.RefreshSignInAsync(user);
        
        return RedirectToAction("Profile");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
