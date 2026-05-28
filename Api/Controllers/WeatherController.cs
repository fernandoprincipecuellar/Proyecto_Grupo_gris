using Microsoft.AspNetCore.Mvc;
using Proyecto_Grupo_gris.Api.Services.Interfaces;

namespace Proyecto_Grupo_gris.Api.Controllers;

[ApiController]
[Route("api/v1/weather")]
public class WeatherController : ControllerBase
{
    private readonly IWeatherService _weatherService;

    public WeatherController(IWeatherService weatherService)
    {
        _weatherService = weatherService;
    }

    [HttpGet("{city}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetByCity(string city)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            return BadRequest(new { message = "Se requiere el nombre de la ciudad." });
        }

        try
        {
            var weather = await _weatherService.GetCurrentWeatherAsync(city.Trim());
            return Ok(new
            {
                route = city,
                weather
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
