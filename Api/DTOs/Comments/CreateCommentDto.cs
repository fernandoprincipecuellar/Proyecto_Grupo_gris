using System.ComponentModel.DataAnnotations;

namespace Proyecto_Grupo_gris.Api.DTOs.Comments;

public class CreateCommentDto
{
    [Required]
    [MinLength(3)]
    public string Content { get; set; } = string.Empty;

    public int? ForumPostId { get; set; }
    public int? EcoRouteId { get; set; }
}
