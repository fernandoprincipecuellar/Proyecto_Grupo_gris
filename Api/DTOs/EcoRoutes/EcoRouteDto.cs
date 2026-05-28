using Proyecto_Grupo_gris.Api.DTOs.Weather;

namespace Proyecto_Grupo_gris.Api.DTOs.EcoRoutes;

public class EcoRouteDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string StartLocation { get; set; } = string.Empty;
    public string EndLocation { get; set; } = string.Empty;
    public string StartCoordinates { get; set; } = string.Empty;
    public string EndCoordinates { get; set; } = string.Empty;
    public string MapUrl { get; set; } = string.Empty;
    public double DistanceKm { get; set; }
    public string Difficulty { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
    public WeatherDto? Weather { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string CreatedById { get; set; } = string.Empty;
    public string CreatedByName { get; set; } = string.Empty;
}
