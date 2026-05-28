using AutoMapper;
using Proyecto_Grupo_gris.Api.DTOs;
using Proyecto_Grupo_gris.Api.DTOs.Forum;
using Proyecto_Grupo_gris.Api.Repositories.Interfaces;
using Proyecto_Grupo_gris.Api.Services.Interfaces;
using Proyecto_Grupo_gris.Models;

namespace Proyecto_Grupo_gris.Api.Services;

public class ForumPostService : IForumPostService
{
    private readonly IForumPostRepository _repository;
    private readonly IMapper _mapper;

    public ForumPostService(IForumPostRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PaginatedResultDto<ForumPostDto>> GetPagedAsync(int page, int pageSize, string? search)
    {
        var items = await _repository.GetPagedAsync(page, pageSize, search);
        var total = await _repository.CountAsync(search);

        return new PaginatedResultDto<ForumPostDto>
        {
            Items = _mapper.Map<IEnumerable<ForumPostDto>>(items),
            Page = page,
            PageSize = pageSize,
            TotalItems = total
        };
    }

    public async Task<ForumPostDto?> GetByIdAsync(int id)
    {
        var post = await _repository.GetByIdAsync(id);
        return post == null ? null : _mapper.Map<ForumPostDto>(post);
    }

    public async Task<ForumPostDto> CreateAsync(CreateForumPostDto request, string userId)
    {
        var forumPost = _mapper.Map<ForumPost>(request);
        forumPost.UserId = userId;
        forumPost.CreatedAt = DateTime.UtcNow;
        var created = await _repository.AddAsync(forumPost);
        return _mapper.Map<ForumPostDto>(created);
    }

    public async Task<ForumPostDto?> UpdateAsync(UpdateForumPostDto request, string userId)
    {
        var existing = await _repository.GetByIdAsync(request.Id);
        if (existing == null) return null;
        if (existing.UserId != userId) throw new UnauthorizedAccessException("No tienes permiso para actualizar este post.");

        existing.ReportType = request.ReportType;
        existing.Description = request.Description;
        existing.ImageUrl = request.ImageUrl;
        existing.Location = request.Location;
        existing.Urgency = request.Urgency;

        var updated = await _repository.UpdateAsync(existing);
        return _mapper.Map<ForumPostDto>(updated);
    }

    public async Task DeleteAsync(int id, string userId)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return;
        if (existing.UserId != userId) throw new UnauthorizedAccessException("No tienes permiso para eliminar este post.");
        await _repository.DeleteAsync(existing);
    }
}
