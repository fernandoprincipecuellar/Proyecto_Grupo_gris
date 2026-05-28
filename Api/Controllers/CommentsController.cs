using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proyecto_Grupo_gris.Api.DTOs.Comments;
using Proyecto_Grupo_gris.Api.Services.Interfaces;

namespace Proyecto_Grupo_gris.Api.Controllers;

[ApiController]
[Route("api/v1/comments")]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _service;

    public CommentsController(ICommentService service)
    {
        _service = service;
    }

    [HttpGet("forum/{forumPostId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByForumPost(int forumPostId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var comments = await _service.GetByForumPostAsync(forumPostId, page, pageSize);
        return Ok(comments);
    }

    [HttpGet("ecoroute/{ecoRouteId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByEcoRoute(int ecoRouteId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var comments = await _service.GetByEcoRouteAsync(ecoRouteId, page, pageSize);
        return Ok(comments);
    }

    [HttpPost]
    [Authorize(Policy = "ApiAccess")]
    public async Task<IActionResult> Create([FromBody] CreateCommentDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        try
        {
            var created = await _service.CreateAsync(request, userId);
            return CreatedAtAction(nameof(GetByForumPost), new { forumPostId = created.ForumPostId }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "ApiAccess")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCommentDto request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        if (request.Id != id) return BadRequest("El id del comentario debe coincidir con el cuerpo.");

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
