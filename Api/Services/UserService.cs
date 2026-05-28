using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Proyecto_Grupo_gris.Api.DTOs;
using Proyecto_Grupo_gris.Api.DTOs.Users;
using Proyecto_Grupo_gris.Api.Repositories.Interfaces;
using Proyecto_Grupo_gris.Api.Services.Interfaces;
using Proyecto_Grupo_gris.Models;

namespace Proyecto_Grupo_gris.Api.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IMapper _mapper;

    public UserService(IUserRepository repository, UserManager<ApplicationUser> userManager, IMapper mapper)
    {
        _repository = repository;
        _userManager = userManager;
        _mapper = mapper;
    }

    public async Task<PaginatedResultDto<UserDto>> GetUsersAsync(int page, int pageSize, string? search)
    {
        var items = await _repository.GetAllAsync(page, pageSize, search);
        var total = await _repository.CountAsync(search);
        var dtos = _mapper.Map<IEnumerable<UserDto>>(items);

        foreach (var dto in dtos)
        {
            var user = await _userManager.FindByIdAsync(dto.Id);
            if (user != null)
            {
                dto.Roles = (await _userManager.GetRolesAsync(user)).ToList();
            }
        }

        return new PaginatedResultDto<UserDto>
        {
            Items = dtos,
            Page = page,
            PageSize = pageSize,
            TotalItems = total
        };
    }

    public async Task<UserDto?> GetByIdAsync(string id)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user == null) return null;

        var dto = _mapper.Map<UserDto>(user);
        dto.Roles = (await _userManager.GetRolesAsync(user)).ToList();
        return dto;
    }

    public async Task<UserDto?> GetByEmailAsync(string email)
    {
        var user = await _repository.GetByEmailAsync(email);
        if (user == null) return null;

        var dto = _mapper.Map<UserDto>(user);
        dto.Roles = (await _userManager.GetRolesAsync(user)).ToList();
        return dto;
    }

    public async Task<UserDto?> UpdateAsync(UpdateUserDto request)
    {
        var user = await _repository.GetByIdAsync(request.Id);
        if (user == null) return null;

        user.Email = request.Email;
        user.UserName = request.Email;
        user.Nombre = request.Nombre;
        user.City = request.City;
        user.Country = request.Country;
        user.Bio = request.Bio;

        var updated = await _repository.UpdateAsync(user);
        var dto = _mapper.Map<UserDto>(updated);
        dto.Roles = (await _userManager.GetRolesAsync(updated)).ToList();
        return dto;
    }

    public async Task DeleteAsync(string id)
    {
        var user = await _repository.GetByIdAsync(id);
        if (user == null) return;
        await _repository.DeleteAsync(user);
    }

    public async Task<UserProfileDto?> GetProfileAsync(string userId)
    {
        var user = await _repository.GetByIdAsync(userId);
        return user == null ? null : _mapper.Map<UserProfileDto>(user);
    }
}
