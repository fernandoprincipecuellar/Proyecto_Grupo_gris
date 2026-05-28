using Proyecto_Grupo_gris.Api.DTOs.Weather;

namespace Proyecto_Grupo_gris.Api.Services.Interfaces;

public interface IWeatherService
{
    Task<WeatherDto> GetCurrentWeatherAsync(string cityName);
}
