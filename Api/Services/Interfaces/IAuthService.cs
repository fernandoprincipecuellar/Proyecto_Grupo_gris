using Proyecto_Grupo_gris.Api.DTOs.Auth;
using Proyecto_Grupo_gris.Api.DTOs.Users;

namespace Proyecto_Grupo_gris.Api.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> AuthenticateAsync(AuthRequestDto request);
    Task<UserDto> RegisterAsync(CreateUserDto request);
}
