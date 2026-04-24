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

    private static int _routeIndex = 0;
    private static double _totalKg = 124.0;
    private static int _entregas = 48;
    private static readonly List<(double Lat, double Lng, string Dir)> _routePoints = new List<(double, double, string)>
    {
        (-12.0464, -77.0428, "Cerca a Plaza de Armas"),
        (-12.0480, -77.0410, "Jr. de la Unión"),
        (-12.0510, -77.0430, "Av. Tacna"),
        (-12.0550, -77.0460, "Av. Wilson"),
        (-12.0520, -77.0500, "Av. Uruguay"),
        (-12.0490, -77.0530, "Av. Alfonso Ugarte"),
        (-12.0460, -77.0500, "Jr. Quilca")
    };

    [Route("api/camion")]
    [HttpGet]
    public IActionResult GetCamionUbicacion()
    {
        var random = new Random();
        var point = _routePoints[_routeIndex];
        _routeIndex = (_routeIndex + 1) % _routePoints.Count;

        // Simulamos que el peso fluctúa (puede subir o bajar)
        _totalKg += (random.NextDouble() * 2.0 - 1.0); // Entre -1.0 y +1.0 kg
        if (_totalKg < 10) _totalKg = 10; // Evitar que sea negativo

        if (_routeIndex % 2 == 0) _entregas += 1;

        var dist = 600 - (_routeIndex * 50); // Simular que se acerca

        return Json(new {
            lat = point.Lat,
            lng = point.Lng,
            nombre = "Camión Recolector",
            distancia = $"A {Math.Max(100, dist)} metros",
            direccion = point.Dir
        });
    }

    [Route("api/stats")]
    [HttpGet]
    public IActionResult GetStats()
    {
        return Json(new {
            totalKg = Math.Round(_totalKg, 1),
            entregas = _entregas,
            puntos = 2488,
            co2 = 89 + (int)(_totalKg - 124),
            arboles = 12,
            agua = 340 + (int)(_totalKg - 124) * 5
        });
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
