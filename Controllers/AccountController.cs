using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Proyecto_Grupo_gris.Models;
using System.Security.Claims;

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


[HttpGet]
public IActionResult Register()
{
    return View();
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Register(RegisterViewModel model)
{
    if (!ModelState.IsValid)
    {
        return View(model);
    }

    var user = new IdentityUser
    {
        UserName = model.Email,
        Email = model.Email
    };

    var result = await _userManager.CreateAsync(user, model.Password);

    if (result.Succeeded)
    {
        await _signInManager.SignInAsync(user, isPersistent: false);
        return RedirectToAction("Index", "Home");
    }

    foreach (var error in result.Errors)
    {
        ModelState.AddModelError(string.Empty, error.Description);
    }

    return View(model);
}

[HttpPost]
[AllowAnonymous]
public IActionResult ExternalLogin(string provider, string? returnUrl = null)
{
    var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { returnUrl });
    var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

    return Challenge(properties, provider);
}

[AllowAnonymous]
public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null)
{
    var info = await _signInManager.GetExternalLoginInfoAsync();

    if (info == null)
        return RedirectToAction("Login");

    var email = info.Principal.FindFirstValue(ClaimTypes.Email);
    var name = info.Principal.FindFirstValue(ClaimTypes.Name) ?? info.Principal.FindFirstValue("given_name");
    var picture = info.Principal.FindFirstValue("picture");

    var signInResult = await _signInManager.ExternalLoginSignInAsync(
        info.LoginProvider,
        info.ProviderKey,
        isPersistent: false);

    if (signInResult.Succeeded)
    {
        var existingUser = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
        if (existingUser != null)
        {
            var claims = await _userManager.GetClaimsAsync(existingUser);

            if (!claims.Any(c => c.Type == "FullName") && !string.IsNullOrEmpty(name))
                await _userManager.AddClaimAsync(existingUser, new Claim("FullName", name));
            
            if (!claims.Any(c => c.Type == "Picture") && !string.IsNullOrEmpty(picture))
                await _userManager.AddClaimAsync(existingUser, new Claim("Picture", picture));
        }
        return RedirectToAction("Index", "Home");
    }

    if (email == null)
        return RedirectToAction("Login");

    var user = new IdentityUser
    {
        UserName = email,
        Email = email
    };

    var result = await _userManager.CreateAsync(user);

    if (result.Succeeded)
    {
        await _userManager.AddLoginAsync(user, info);
        
        // Add claims for name and picture
        if (!string.IsNullOrEmpty(name))
            await _userManager.AddClaimAsync(user, new Claim("FullName", name));
        if (!string.IsNullOrEmpty(picture))
            await _userManager.AddClaimAsync(user, new Claim("Picture", picture));

        await _signInManager.SignInAsync(user, false);
        return RedirectToAction("Index", "Home");
    }

    return RedirectToAction("Login");
}

}
