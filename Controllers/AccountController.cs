using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Proyecto_Grupo_gris.Models;
using System.Linq;
using System.Security.Claims;

namespace Proyecto_Grupo_gris.Controllers;

[AllowAnonymous]
[Route("Account")]
public class AccountController : Controller
{
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly UserManager<IdentityUser> _userManager;

    public AccountController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [HttpGet("Login")]
    public async Task<IActionResult> Login(string? returnUrl = null)
    {
        var model = new LoginViewModel
        {
            ReturnUrl = returnUrl ?? Url.Content("~/"),
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
        };
        return View(model);
    }

    [HttpPost("Login")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        model.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

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
            var safeReturnUrl = Url.IsLocalUrl(model.ReturnUrl) ? model.ReturnUrl : Url.Content("~/");
            return LocalRedirect(safeReturnUrl);
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Cuenta bloqueada temporalmente.");
            return View(model);
        }

        ModelState.AddModelError(string.Empty, "Correo o contraseña inválidos.");
        return View(model);
    }

    [HttpGet("Register")]
    public async Task<IActionResult> Register(string? returnUrl = null)
    {
        var externalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        
        var model = new RegisterViewModel
        {
            ReturnUrl = returnUrl ?? Url.Content("~/"),
            ExternalLogins = externalLogins
        };

        return View(model);
    }

    [HttpPost("Register")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        model.ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new IdentityUser { UserName = model.Email, Email = model.Email };
        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            await _signInManager.SignInAsync(user, isPersistent: false);
            var safeReturnUrl = Url.IsLocalUrl(model.ReturnUrl) ? model.ReturnUrl : Url.Content("~/");
            return LocalRedirect(safeReturnUrl);
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    [HttpGet("ExternalLogin")]
    public IActionResult ExternalLogin(string provider, string? returnUrl = null)
    {
        var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(properties, provider);
    }

    [HttpGet("ExternalLoginCallback")]
    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null, string? remoteError = null)
    {
        returnUrl ??= Url.Content("~/");

        if (remoteError != null)
        {
            var loginModel = new LoginViewModel
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };
            ModelState.AddModelError(string.Empty, $"Error de proveedor externo: {remoteError}");
            return View("Login", loginModel);
        }

        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info == null)
        {
            return RedirectToAction(nameof(Login));
        }

        var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false, true);
        if (signInResult.Succeeded)
        {
            var safeReturnUrlLocal = Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Content("~/");
            return LocalRedirect(safeReturnUrlLocal);
        }

        var email = info.Principal?.FindFirstValue(ClaimTypes.Email);
        if (email == null)
        {
            var loginModel = new LoginViewModel
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };
            ModelState.AddModelError(string.Empty, "No se recibió un correo electrónico desde el proveedor externo.");
            return View("Login", loginModel);
        }

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new IdentityUser { UserName = email, Email = email };
            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                var loginModel = new LoginViewModel
                {
                    ReturnUrl = returnUrl,
                    ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
                };
                foreach (var error in createResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View("Login", loginModel);
            }
        }

        var addLoginResult = await _userManager.AddLoginAsync(user, info);
        if (!addLoginResult.Succeeded)
        {
            var loginModel = new LoginViewModel
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };
            foreach (var error in addLoginResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View("Login", loginModel);
        }

        await _signInManager.SignInAsync(user, isPersistent: false);
        var safeReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Content("~/");
        return LocalRedirect(safeReturnUrl);
    }

    [HttpPost("Logout")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}
