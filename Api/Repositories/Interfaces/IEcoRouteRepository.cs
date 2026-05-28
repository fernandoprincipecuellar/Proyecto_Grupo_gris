using Proyecto_Grupo_gris.Models;

namespace Proyecto_Grupo_gris.Api.Repositories.Interfaces;

public interface IEcoRouteRepository : IRepository<EcoRoute>
{
    Task<IEnumerable<EcoRoute>> GetPagedAsync(int page, int pageSize, string? search);
    Task<int> CountAsync(string? search);
}
