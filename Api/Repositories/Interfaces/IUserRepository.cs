using Proyecto_Grupo_gris.Models;

namespace Proyecto_Grupo_gris.Api.Repositories.Interfaces;

public interface IUserRepository
{
    Task<ApplicationUser?> GetByIdAsync(string id);
    Task<ApplicationUser?> GetByEmailAsync(string email);
    Task<IEnumerable<ApplicationUser>> GetAllAsync(int page, int pageSize, string? search);
    Task<int> CountAsync(string? search);
    Task<ApplicationUser> AddAsync(ApplicationUser user);
    Task<ApplicationUser> UpdateAsync(ApplicationUser user);
    Task DeleteAsync(ApplicationUser user);
}
