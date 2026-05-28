using Proyecto_Grupo_gris.Api.DTOs;
using Proyecto_Grupo_gris.Api.DTOs.Forum;

namespace Proyecto_Grupo_gris.Api.Services.Interfaces;

public interface IForumPostService
{
    Task<PaginatedResultDto<ForumPostDto>> GetPagedAsync(int page, int pageSize, string? search);
    Task<ForumPostDto?> GetByIdAsync(int id);
    Task<ForumPostDto> CreateAsync(CreateForumPostDto request, string userId);
    Task<ForumPostDto?> UpdateAsync(UpdateForumPostDto request, string userId);
    Task DeleteAsync(int id, string userId);
}
