namespace Proyecto_Grupo_gris.Api.DTOs.Forum;

public class ForumPostDto
{
    public int Id { get; set; }
    public string ReportType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? Location { get; set; }
    public string? Urgency { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int CommentsCount { get; set; }
    public int LikesCount { get; set; }
}
