using System.ComponentModel.DataAnnotations;

namespace Proyecto_Grupo_gris.Api.DTOs.Comments;

public class UpdateCommentDto
{
    [Required]
    public int Id { get; set; }

    [Required]
    [MinLength(3)]
    public string Content { get; set; } = string.Empty;
}
