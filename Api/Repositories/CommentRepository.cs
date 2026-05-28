using Microsoft.EntityFrameworkCore;
using Proyecto_Grupo_gris.Api.Repositories.Interfaces;
using Proyecto_Grupo_gris.Data;
using Proyecto_Grupo_gris.Models;

namespace Proyecto_Grupo_gris.Api.Repositories;

public class CommentRepository : RepositoryBase<Comment>, ICommentRepository
{
    public CommentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Comment>> GetByForumPostAsync(int forumPostId, int page, int pageSize)
    {
        return await Context.Comments
            .Include(c => c.Author)
            .Where(c => c.ForumPostId == forumPostId)
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Comment>> GetByEcoRouteAsync(int ecoRouteId, int page, int pageSize)
    {
        return await Context.Comments
            .Include(c => c.Author)
            .Where(c => c.EcoRouteId == ecoRouteId)
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
    }
}
