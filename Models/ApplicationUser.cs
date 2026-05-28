using Microsoft.AspNetCore.Identity;

namespace Proyecto_Grupo_gris.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Nombre { get; set; } = string.Empty;
        public string Bio { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<ForumPost> ForumPosts { get; set; } = new List<ForumPost>();
        public virtual ICollection<EcoRoute> EcoRoutes { get; set; } = new List<EcoRoute>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}