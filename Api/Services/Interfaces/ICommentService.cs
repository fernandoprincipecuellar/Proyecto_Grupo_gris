using Proyecto_Grupo_gris.Api.DTOs.Comments;

namespace Proyecto_Grupo_gris.Api.Services.Interfaces;

public interface ICommentService
{
    Task<IEnumerable<CommentDto>> GetByForumPostAsync(int forumPostId, int page, int pageSize);
    Task<IEnumerable<CommentDto>> GetByEcoRouteAsync(int ecoRouteId, int page, int pageSize);
    Task<CommentDto> CreateAsync(CreateCommentDto request, string userId);
    Task<CommentDto?> UpdateAsync(UpdateCommentDto request, string userId);
    Task DeleteAsync(int id, string userId);
}
