using Proyecto_Grupo_gris.Api.DTOs.Users;

namespace Proyecto_Grupo_gris.Api.DTOs.Auth;

public class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = new UserDto();
}
