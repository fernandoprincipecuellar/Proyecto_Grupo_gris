using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Proyecto_Grupo_gris.Models;

namespace Proyecto_Grupo_gris.Controllers;

public class HomeController : Controller
{
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

    public IActionResult Profile()
    {
        return View();
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
