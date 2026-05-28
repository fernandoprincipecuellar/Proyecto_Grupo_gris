using System.ComponentModel.DataAnnotations;

namespace Proyecto_Grupo_gris.Api.DTOs.EcoRoutes;

public class CreateEcoRouteDto
{
    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string StartLocation { get; set; } = string.Empty;

    [Required]
    public string EndLocation { get; set; } = string.Empty;

    [Range(0.1, 1000)]
    public double DistanceKm { get; set; }

    public string Difficulty { get; set; } = "Moderate";
    public bool IsPublished { get; set; } = true;
}
