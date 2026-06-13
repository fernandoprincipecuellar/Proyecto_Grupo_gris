using System.Text.Json;
using Microsoft.SemanticKernel;
using Proyecto_Grupo_gris.Api.Services.Interfaces;

namespace Proyecto_Grupo_gris.Services.Agents.Plugins
{
    public class WeatherPlugin
    {
        private readonly IWeatherService _weatherService;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public WeatherPlugin(IWeatherService weatherService, HttpClient httpClient, IConfiguration configuration)
        {
            _weatherService = weatherService;
            _httpClient = httpClient;
            _configuration = configuration;
        }

        [KernelFunction]
        public async Task<string> GetWeatherAsync(string city)
        {
            try
            {
                var weather = await _weatherService.GetCurrentWeatherAsync(city);
                return JsonSerializer.Serialize(new
                {
                    ciudad = city,
                    temperatura = weather.TemperatureC,
                    condicion = weather.Condition,
                    humedad = weather.Humidity,
                    viento = weather.WindSpeed
                });
            }
            catch (Exception ex)
            {
                return $"Error al obtener el clima de {city}: {ex.Message}";
            }
        }

        [KernelFunction]
        public async Task<string> GetForecastAsync(string city, int days = 5)
        {
            try
            {
                var apiKey = _configuration["OpenWeather:ApiKey"];
                if (string.IsNullOrWhiteSpace(apiKey))
                    return "Error: OpenWeather API key no configurada.";

                var requestUrl = $"https://api.openweathermap.org/data/2.5/forecast?q={Uri.EscapeDataString(city)}&units=metric&cnt={days * 8}&appid={apiKey}";
                using var response = await _httpClient.GetAsync(requestUrl);

                if (!response.IsSuccessStatusCode)
                    return $"Error al obtener pronóstico: {response.StatusCode}";

                var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
                var forecasts = new List<object>();

                foreach (var item in payload.GetProperty("list").EnumerateArray().Where((_, i) => i % 8 == 0).Take(days))
                {
                    forecasts.Add(new
                    {
                        fecha = item.GetProperty("dt_txt").GetString(),
                        temperatura = item.GetProperty("main").GetProperty("temp").GetDouble(),
                        condicion = item.GetProperty("weather")[0].GetProperty("description").GetString()
                    });
                }

                return JsonSerializer.Serialize(new { ciudad = city, pronostico = forecasts });
            }
            catch (Exception ex)
            {
                return $"Error al obtener pronóstico de {city}: {ex.Message}";
            }
        }
    }
}
