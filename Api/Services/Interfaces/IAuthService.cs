using Proyecto_Grupo_gris.Api.DTOs.Auth;
using Proyecto_Grupo_gris.Api.DTOs.Users;
using Proyecto_Grupo_gris.Models;

namespace Proyecto_Grupo_gris.Api.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> AuthenticateAsync(AuthRequestDto request);
    Task<AuthResponseDto> AuthenticateExternalAsync(ApplicationUser user);
    Task<UserDto> RegisterAsync(CreateUserDto request);
}
