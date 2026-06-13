using System.Text.Json;
using Microsoft.SemanticKernel;

namespace Proyecto_Grupo_gris.Services.Agents.Plugins
{
    public class AirQualityPlugin
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public AirQualityPlugin(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        [KernelFunction]
        public async Task<string> GetAirQualityAsync(string city)
        {
            try
            {
                var apiKey = _configuration["OpenWeather:ApiKey"];
                if (string.IsNullOrWhiteSpace(apiKey))
                    return "Error: OpenWeather API key no configurada.";

                var geoUrl = $"http://api.openweathermap.org/geo/1.0/direct?q={Uri.EscapeDataString(city)}&limit=1&appid={apiKey}";
                using var geoResponse = await _httpClient.GetAsync(geoUrl);
                var geoPayload = await geoResponse.Content.ReadFromJsonAsync<JsonElement>();

                if (geoPayload.GetArrayLength() == 0)
                    return $"No se encontraron coordenadas para {city}";

                var lat = geoPayload[0].GetProperty("lat").GetDouble();
                var lon = geoPayload[0].GetProperty("lon").GetDouble();

                return await GetAirQualityByCoordsInternalAsync(lat, lon, city);
            }
            catch (Exception ex)
            {
                return $"Error al obtener calidad del aire de {city}: {ex.Message}";
            }
        }

        [KernelFunction]
        public async Task<string> GetAirQualityByCoordsAsync(double lat, double lon)
        {
            try
            {
                return await GetAirQualityByCoordsInternalAsync(lat, lon, $"coordenadas ({lat}, {lon})");
            }
            catch (Exception ex)
            {
                return $"Error al obtener calidad del aire: {ex.Message}";
            }
        }

        private async Task<string> GetAirQualityByCoordsInternalAsync(double lat, double lon, string locationName)
        {
            var apiKey = _configuration["OpenWeather:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
                return "Error: OpenWeather API key no configurada.";

            var requestUrl = $"https://api.openweathermap.org/data/2.5/air_pollution?lat={lat}&lon={lon}&appid={apiKey}";
            using var response = await _httpClient.GetAsync(requestUrl);

            if (!response.IsSuccessStatusCode)
                return $"Error al consultar calidad del aire: {response.StatusCode}";

            var payload = await response.Content.ReadFromJsonAsync<JsonElement>();
            var list = payload.GetProperty("list")[0];
            var main = list.GetProperty("main");
            var components = list.GetProperty("components");

            var aqi = main.GetProperty("aqi").GetInt32();
            var aqiLabel = GetAqiLabel(aqi);

            return JsonSerializer.Serialize(new
            {
                ubicacion = locationName,
                indice_calidad_aire = aqi,
                nivel = aqiLabel,
                pm2_5 = components.GetProperty("pm2_5").GetDouble(),
                pm10 = components.GetProperty("pm10").GetDouble(),
                monoxido_carbono = components.GetProperty("co").GetDouble(),
                nitrogeno_dioxido = components.GetProperty("no2").GetDouble(),
                ozono = components.GetProperty("o3").GetDouble(),
                dioxido_azufre = components.GetProperty("so2").GetDouble()
            });
        }

        private string GetAqiLabel(int aqi)
        {
            return aqi switch
            {
                1 => "Buena",
                2 => "Aceptable",
                3 => "Moderada",
                4 => "Mala",
                5 => "Muy mala",
                _ => "Desconocida"
            };
        }
    }
}
