using Proyecto_Grupo_gris.Api.DTOs;
using Proyecto_Grupo_gris.Api.DTOs.Users;

namespace Proyecto_Grupo_gris.Api.Services.Interfaces;

public interface IUserService
{
    Task<PaginatedResultDto<UserDto>> GetUsersAsync(int page, int pageSize, string? search);
    Task<UserDto?> GetByIdAsync(string id);
    Task<UserDto?> UpdateAsync(UpdateUserDto request);
    Task DeleteAsync(string id);
    Task<UserProfileDto?> GetProfileAsync(string userId);
    Task<UserDto?> GetByEmailAsync(string email);
}
