using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proyecto_Grupo_gris.Api.DTOs.Forum;
using Proyecto_Grupo_gris.Api.Services.Interfaces;

namespace Proyecto_Grupo_gris.Api.Controllers;

[ApiController]
[Route("api/v1/forum")]
public class ForumPostsController : ControllerBase
{
    private readonly IForumPostService _service;

    public ForumPostsController(IForumPostService service)
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
        var item = await _service.GetByIdAsync(id);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpPost]
    [Authorize(Policy = "ApiAccess")]
    public async Task<IActionResult> Create([FromBody] CreateForumPostDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();
        var created = await _service.CreateAsync(request, userId);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "ApiAccess")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateForumPostDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (request.Id != id) return BadRequest("El id del post debe coincidir con el cuerpo.");

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
