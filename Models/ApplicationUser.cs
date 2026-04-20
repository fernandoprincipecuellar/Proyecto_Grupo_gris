using Microsoft.AspNetCore.Identity;

namespace Proyecto_Grupo_gris.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Nombre { get; set; } = string.Empty;
    }
}