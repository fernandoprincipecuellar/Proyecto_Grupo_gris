using Microsoft.EntityFrameworkCore;
using Proyecto_Grupo_gris.Api.Repositories.Interfaces;
using Proyecto_Grupo_gris.Data;
using Proyecto_Grupo_gris.Models;

namespace Proyecto_Grupo_gris.Api.Repositories;

public class EcoRouteRepository : RepositoryBase<EcoRoute>, IEcoRouteRepository
{
    public EcoRouteRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<EcoRoute>> GetPagedAsync(int page, int pageSize, string? search)
    {
        var query = Context.EcoRoutes
            .Include(r => r.CreatedBy)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(r => r.Title.Contains(search) || r.Description.Contains(search) || r.StartLocation.Contains(search) || r.EndLocation.Contains(search));
        }

        return await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountAsync(string? search)
    {
        var query = Context.EcoRoutes.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(r => r.Title.Contains(search) || r.Description.Contains(search) || r.StartLocation.Contains(search) || r.EndLocation.Contains(search));
        }

        return await query.CountAsync();
    }
}
