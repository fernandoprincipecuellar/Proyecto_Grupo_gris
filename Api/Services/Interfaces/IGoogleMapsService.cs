using Proyecto_Grupo_gris.Api.DTOs.GoogleMaps;

namespace Proyecto_Grupo_gris.Api.Services.Interfaces;

public interface IGoogleMapsService
{
    Task<GoogleMapsRouteDataDto> GetRouteDataAsync(string originAddress, string destinationAddress);
}
