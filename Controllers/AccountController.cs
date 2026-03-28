using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Proyecto_Grupo_gris.Models;

namespace Proyecto_Grupo_gris.Controllers;

[AllowAnonymous]
public class AccountController : Controller
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    public AccountController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginViewModel
        {
            ReturnUrl = returnUrl ?? Url.Content("~/")
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Correo o contraseña inválidos.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(user, model.Password, isPersistent: false, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            return LocalRedirect(model.ReturnUrl ?? Url.Content("~/"));
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Cuenta bloqueada temporalmente.");
            return View(model);
        }

        ModelState.AddModelError(string.Empty, "Correo o contraseña inválidos.");
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}
