using System.ComponentModel.DataAnnotations;

namespace Proyecto_Grupo_gris.Models
{
    public class EcoRoute
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string StartLocation { get; set; } = string.Empty;

        [Required]
        public string EndLocation { get; set; } = string.Empty;

        public double DistanceKm { get; set; }
        public string Difficulty { get; set; } = string.Empty;
        public bool IsPublished { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public string CreatedById { get; set; } = string.Empty;
        public virtual ApplicationUser? CreatedBy { get; set; }

        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
