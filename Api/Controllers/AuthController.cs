using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proyecto_Grupo_gris.Api.DTOs.Auth;
using Proyecto_Grupo_gris.Api.DTOs.Users;
using Proyecto_Grupo_gris.Api.Services.Interfaces;

namespace Proyecto_Grupo_gris.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] AuthRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var response = await _authService.AuthenticateAsync(request);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] CreateUserDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var user = await _authService.RegisterAsync(request);
            return CreatedAtAction(nameof(Register), new { id = user.Id }, user);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("profile")]
    [Authorize(Policy = "ApiAccess")]
    public IActionResult Profile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Ok(new { userId });
    }
}
