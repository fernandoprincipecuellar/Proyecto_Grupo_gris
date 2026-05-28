using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proyecto_Grupo_gris.Api.DTOs.EcoRoutes;
using Proyecto_Grupo_gris.Api.Services.Interfaces;

namespace Proyecto_Grupo_gris.Api.Controllers;

[ApiController]
[Route("api/v1/ecoroutes")]
public class EcoRoutesController : ControllerBase
{
    private readonly IEcoRouteService _service;

    public EcoRoutesController(IEcoRouteService service)
    {
        _service = service;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null)
    {
        var result = await _service.GetPagedAsync(page, pageSize, search);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var route = await _service.GetByIdAsync(id);
        if (route == null) return NotFound();
        return Ok(route);
    }

    [HttpPost]
    [Authorize(Policy = "ApiAccess")]
    public async Task<IActionResult> Create([FromBody] CreateEcoRouteDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var result = await _service.CreateAsync(request, userId);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "ApiAccess")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEcoRouteDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (request.Id != id) return BadRequest("El id de la ruta y la ruta en el cuerpo deben coincidir.");

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        try
        {
            var updated = await _service.UpdateAsync(request, userId);
            if (updated == null) return NotFound();
            return Ok(updated);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "ApiAccess")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        try
        {
            await _service.DeleteAsync(id, userId);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }
}
