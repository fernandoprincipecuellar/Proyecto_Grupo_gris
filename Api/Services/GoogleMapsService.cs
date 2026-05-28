using System.Text.Json;
using Proyecto_Grupo_gris.Api.DTOs.GoogleMaps;
using Proyecto_Grupo_gris.Api.Services.Interfaces;

namespace Proyecto_Grupo_gris.Api.Services;

public class GoogleMapsService : IGoogleMapsService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public GoogleMapsService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<GoogleMapsRouteDataDto> GetRouteDataAsync(string originAddress, string destinationAddress)
    {
        var apiKey = _configuration["GoogleMaps:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("GoogleMaps:ApiKey no está configurada.");

        var origin = Uri.EscapeDataString(originAddress);
        var destination = Uri.EscapeDataString(destinationAddress);
        var requestUrl = $"https://maps.googleapis.com/maps/api/directions/json?origin={origin}&destination={destination}&mode=walking&key={apiKey}";

        using var response = await _httpClient.GetAsync(requestUrl);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Google Maps Directions request failed: {response.StatusCode}. {body}");
        }

        using var contentStream = await response.Content.ReadAsStreamAsync();
        using var json = await JsonDocument.ParseAsync(contentStream);

        var status = json.RootElement.GetProperty("status").GetString();
        if (status != "OK")
        {
            var errorMessage = json.RootElement.TryGetProperty("error_message", out var errorElement)
                ? errorElement.GetString()
                : "No se pudo obtener la ruta desde Google Maps.";
            throw new InvalidOperationException($"Google Maps error: {status}. {errorMessage}");
        }

        var route = json.RootElement.GetProperty("routes")[0];
        var leg = route.GetProperty("legs")[0];
        var distanceMeters = leg.GetProperty("distance").GetProperty("value").GetInt32();
        var startLocation = leg.GetProperty("start_location");
        var endLocation = leg.GetProperty("end_location");

        return new GoogleMapsRouteDataDto
        {
            OriginAddress = leg.GetProperty("start_address").GetString() ?? originAddress,
            DestinationAddress = leg.GetProperty("end_address").GetString() ?? destinationAddress,
            StartLocation = new LocationDto
            {
                Lat = startLocation.GetProperty("lat").GetDouble(),
                Lng = startLocation.GetProperty("lng").GetDouble()
            },
            EndLocation = new LocationDto
            {
                Lat = endLocation.GetProperty("lat").GetDouble(),
                Lng = endLocation.GetProperty("lng").GetDouble()
            },
            DistanceKm = Math.Round(distanceMeters / 1000.0, 2),
            Summary = route.GetProperty("summary").GetString() ?? string.Empty,
            MapUrl = $"https://www.google.com/maps/dir/?api=1&origin={origin}&destination={destination}&travelmode=walking"
        };
    }
}
