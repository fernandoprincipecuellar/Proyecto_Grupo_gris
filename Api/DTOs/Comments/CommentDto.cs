namespace Proyecto_Grupo_gris.Api.DTOs.Comments;

public class CommentDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string AuthorId { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public int? ForumPostId { get; set; }
    public int? EcoRouteId { get; set; }
}
