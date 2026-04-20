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
            Co2AhorradoKg = 89,
            ArbolesSalvados = 12,
            AguaAhorradaL = 340,
            NombreUsuario = "María González",
            CargoUsuario = "Eco-conductora desde 2023",
            Email = "maria.g@email.com",
            Telefono = "+34 612 345 678",
            Ciudad = "Madrid, España",
            PerfilIniciales = "M",
            Logro1Titulo = "Eco-Conductor del Mes",
            Logro1Descripcion = "Menor huella de carbono",
            Logro2Titulo = "100 Viajes Verdes",
            Logro2Descripcion = "Completados este año",
        };

        return View(model);
    }

    public IActionResult Profile()
    {
        var model = new DashboardViewModel
        {
            NombreUsuario = "María González",
            CargoUsuario = "Eco-conductora desde 2023",
            Email = "maria.g@email.com",
            Telefono = "+34 612 345 678",
            Ciudad = "Madrid, España",
            PerfilIniciales = "M",
            Logro1Titulo = "Eco-Conductor del Mes",
            Logro1Descripcion = "Menor huella de carbono",
            Logro2Titulo = "100 Viajes Verdes",
            Logro2Descripcion = "Completados este año"
        };

        return View(model);
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
