using System.ComponentModel.DataAnnotations;

namespace Proyecto_Grupo_gris.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string AuthorId { get; set; } = string.Empty;
        public virtual ApplicationUser? Author { get; set; }

        public int? ForumPostId { get; set; }
        public virtual ForumPost? ForumPost { get; set; }

        public int? EcoRouteId { get; set; }
        public virtual EcoRoute? EcoRoute { get; set; }
    }
}
