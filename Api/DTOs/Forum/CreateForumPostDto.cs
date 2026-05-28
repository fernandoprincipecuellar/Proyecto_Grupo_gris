using System.ComponentModel.DataAnnotations;

namespace Proyecto_Grupo_gris.Api.DTOs.Forum;

public class CreateForumPostDto
{
    [Required]
    public string ReportType { get; set; } = string.Empty;

    [Required]
    [MinLength(10)]
    public string Description { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }
    public string? Location { get; set; }
    public string? Urgency { get; set; }
}
