using Proyecto_Grupo_gris.Models;

namespace Proyecto_Grupo_gris.Api.Repositories.Interfaces;

public interface IForumPostRepository : IRepository<ForumPost>
{
    Task<IEnumerable<ForumPost>> GetPagedAsync(int page, int pageSize, string? search);
    Task<int> CountAsync(string? search);
}
