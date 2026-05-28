using Microsoft.EntityFrameworkCore;
using Proyecto_Grupo_gris.Api.Repositories.Interfaces;
using Proyecto_Grupo_gris.Data;
using Proyecto_Grupo_gris.Models;

namespace Proyecto_Grupo_gris.Api.Repositories;

public class ForumPostRepository : RepositoryBase<ForumPost>, IForumPostRepository
{
    public ForumPostRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ForumPost>> GetPagedAsync(int page, int pageSize, string? search)
    {
        var query = Context.ForumPosts
            .Include(p => p.User)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => (p.ReportType != null && p.ReportType.Contains(search))
                || (p.Description != null && p.Description.Contains(search))
                || (p.Location != null && p.Location.Contains(search)));
        }

        return await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountAsync(string? search)
    {
        var query = Context.ForumPosts.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => (p.ReportType != null && p.ReportType.Contains(search))
                || (p.Description != null && p.Description.Contains(search))
                || (p.Location != null && p.Location.Contains(search)));
        }

        return await query.CountAsync();
    }
}
