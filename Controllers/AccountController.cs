using Microsoft.AspNetCore.Mvc;
using Proyecto_Grupo_gris.Models;

namespace Proyecto_Grupo_gris.Controllers;

public class AccountController : Controller
{
    [HttpGet]
    public IActionResult Login(string returnUrl = "/")
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpGet]
    public IActionResult Register(string returnUrl = "/")
    {
        return View(new RegisterViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    public IActionResult Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public IActionResult Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public IActionResult Logout()
    {
        // Lógica de logout aquí
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult ExternalLogin(string provider, string? returnUrl = null)
    {
        return Challenge(provider);
    }
}