using System.Net.Http.Json;
using System.Text.Json;
using Proyecto_Grupo_gris.Api.DTOs.Weather;
using Proyecto_Grupo_gris.Api.Services.Interfaces;

namespace Proyecto_Grupo_gris.Api.Services;

public class WeatherService : IWeatherService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public WeatherService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<WeatherDto> GetCurrentWeatherAsync(string cityName)
    {
        var apiKey = _configuration["OpenWeather:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("OpenWeather:ApiKey no está configurada.");

        var requestUrl = $"https://api.openweathermap.org/data/2.5/weather?q={Uri.EscapeDataString(cityName)}&units=metric&appid={apiKey}";
        using var response = await _httpClient.GetAsync(requestUrl);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"OpenWeather request failed: {response.StatusCode}. {body}");
        }

        var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
        if (payload.ValueKind == JsonValueKind.Undefined)
            throw new InvalidOperationException("No se recibió respuesta válida de OpenWeather.");

        var weatherArray = payload.GetProperty("weather")[0];
        var main = payload.GetProperty("main");
        var wind = payload.GetProperty("wind");

        return new WeatherDto
        {
            Condition = weatherArray.GetProperty("description").GetString() ?? "Unknown",
            TemperatureC = main.GetProperty("temp").GetDouble(),
            Humidity = main.GetProperty("humidity").GetInt32(),
            WindSpeed = wind.GetProperty("speed").GetDouble()
        };
    }
}
