namespace Proyecto_Grupo_gris.Api.DTOs.GoogleMaps;

public class GoogleMapsRouteDataDto
{
    public string OriginAddress { get; set; } = string.Empty;
    public string DestinationAddress { get; set; } = string.Empty;
    public LocationDto StartLocation { get; set; } = new();
    public LocationDto EndLocation { get; set; } = new();
    public double DistanceKm { get; set; }
    public string MapUrl { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
}
