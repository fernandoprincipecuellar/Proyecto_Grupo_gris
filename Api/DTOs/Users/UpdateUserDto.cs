using System.ComponentModel.DataAnnotations;

namespace Proyecto_Grupo_gris.Api.DTOs.Users;

public class UpdateUserDto
{
    [Required]
    public string Id { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
}
