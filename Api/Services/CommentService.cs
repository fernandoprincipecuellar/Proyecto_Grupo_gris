using AutoMapper;
using Proyecto_Grupo_gris.Api.DTOs.Comments;
using Proyecto_Grupo_gris.Api.Repositories.Interfaces;
using Proyecto_Grupo_gris.Api.Services.Interfaces;
using Proyecto_Grupo_gris.Models;

namespace Proyecto_Grupo_gris.Api.Services;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _repository;
    private readonly IMapper _mapper;

    public CommentService(ICommentRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CommentDto>> GetByForumPostAsync(int forumPostId, int page, int pageSize)
    {
        var comments = await _repository.GetByForumPostAsync(forumPostId, page, pageSize);
        return _mapper.Map<IEnumerable<CommentDto>>(comments);
    }

    public async Task<IEnumerable<CommentDto>> GetByEcoRouteAsync(int ecoRouteId, int page, int pageSize)
    {
        var comments = await _repository.GetByEcoRouteAsync(ecoRouteId, page, pageSize);
        return _mapper.Map<IEnumerable<CommentDto>>(comments);
    }

    public async Task<CommentDto> CreateAsync(CreateCommentDto request, string userId)
    {
        if (request.ForumPostId == null && request.EcoRouteId == null)
        {
            throw new ArgumentException("El comentario debe pertenecer a una ruta ecológica o a un post de foro.");
        }

        var comment = _mapper.Map<Comment>(request);
        comment.AuthorId = userId;
        comment.CreatedAt = DateTime.UtcNow;

        var created = await _repository.AddAsync(comment);
        return _mapper.Map<CommentDto>(created);
    }

    public async Task<CommentDto?> UpdateAsync(UpdateCommentDto request, string userId)
    {
        var comment = await _repository.GetByIdAsync(request.Id);
        if (comment == null) return null;
        if (comment.AuthorId != userId) throw new UnauthorizedAccessException("No puedes modificar este comentario.");

        comment.Content = request.Content;
        var updated = await _repository.UpdateAsync(comment);
        return _mapper.Map<CommentDto>(updated);
    }

    public async Task DeleteAsync(int id, string userId)
    {
        var comment = await _repository.GetByIdAsync(id);
        if (comment == null) return;
        if (comment.AuthorId != userId) throw new UnauthorizedAccessException("No puedes eliminar este comentario.");
        await _repository.DeleteAsync(comment);
    }
}
