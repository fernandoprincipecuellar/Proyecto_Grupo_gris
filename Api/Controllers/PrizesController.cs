using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto_Grupo_gris.Data;
using Proyecto_Grupo_gris.Models;
using System.Security.Claims;

namespace Proyecto_Grupo_gris.Api.Controllers
{
    [ApiController]
    [Route("api/v1/prizes")]
    public class PrizesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PrizesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var prizes = await _context.Prizes
                .Where(p => p.IsActive)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Description,
                    p.PointCost,
                    p.IconName,
                    p.Category,
                    p.BadgeText,
                    p.GradientFrom,
                    p.GradientTo
                })
                .ToListAsync();

            return Ok(prizes);
        }

        [HttpPost("{id}/redeem")]
        [Authorize]
        public async Task<IActionResult> Redeem(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _context.AspNetUsers.FindAsync(userId);
            if (user == null)
                return NotFound("Usuario no encontrado.");

            var prize = await _context.Prizes.FindAsync(id);
            if (prize == null || !prize.IsActive)
                return NotFound("Premio no encontrado.");

            if (user.Puntos < prize.PointCost)
                return BadRequest(new { error = "No tienes suficientes EcoPuntos.", puntos = user.Puntos, costo = prize.PointCost });

            user.Puntos -= prize.PointCost;

            var redemption = new PrizeRedemption
            {
                UserId = userId,
                PrizeId = id,
                PointsSpent = prize.PointCost,
                RedeemedAt = DateTime.UtcNow
            };

            _context.PrizeRedemptions.Add(redemption);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, puntosRestantes = user.Puntos, premio = prize.Name });
        }
    }
}
