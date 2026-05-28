using System.ComponentModel.DataAnnotations;

namespace Proyecto_Grupo_gris.Api.DTOs.Auth;

public class AuthRequestDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
