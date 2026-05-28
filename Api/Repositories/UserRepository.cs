using Microsoft.EntityFrameworkCore;
using Proyecto_Grupo_gris.Api.Repositories.Interfaces;
using Proyecto_Grupo_gris.Data;
using Proyecto_Grupo_gris.Models;

namespace Proyecto_Grupo_gris.Api.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApplicationUser?> GetByIdAsync(string id)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<ApplicationUser?> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<IEnumerable<ApplicationUser>> GetAllAsync(int page, int pageSize, string? search)
    {
        var query = _context.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(u => (u.Email != null && u.Email.Contains(search))
                || (u.Nombre != null && u.Nombre.Contains(search))
                || (u.City != null && u.City.Contains(search))
                || (u.Country != null && u.Country.Contains(search)));
        }

        return await query
            .OrderBy(u => u.UserName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountAsync(string? search)
    {
        var query = _context.Users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(u => (u.Email != null && u.Email.Contains(search))
                || (u.Nombre != null && u.Nombre.Contains(search))
                || (u.City != null && u.City.Contains(search))
                || (u.Country != null && u.Country.Contains(search)));
        }

        return await query.CountAsync();
    }

    public async Task<ApplicationUser> AddAsync(ApplicationUser user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<ApplicationUser> UpdateAsync(ApplicationUser user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task DeleteAsync(ApplicationUser user)
    {
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
    }
}
