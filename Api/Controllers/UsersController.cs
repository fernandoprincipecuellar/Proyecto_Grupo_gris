using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proyecto_Grupo_gris.Api.DTOs.Users;
using Proyecto_Grupo_gris.Api.Services.Interfaces;

namespace Proyecto_Grupo_gris.Api.Controllers;

[ApiController]
[Route("api/v1/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
    {
        var result = await _userService.GetUsersAsync(page, pageSize, search);
        return Ok(result);
    }

    [HttpGet("me")]
    [Authorize(Policy = "ApiAccess")]
    public async Task<IActionResult> GetMe()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var profile = await _userService.GetProfileAsync(userId);
        if (profile == null) return NotFound();
        return Ok(profile);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "ApiAccess")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _userService.GetByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPut("me")]
    [Authorize(Policy = "ApiAccess")]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateUserDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        if (request.Id != userId) return Forbid();

        var result = await _userService.UpdateAsync(request);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "Admin")]
    public async Task<IActionResult> Delete(string id)
    {
        await _userService.DeleteAsync(id);
        return NoContent();
    }
}
