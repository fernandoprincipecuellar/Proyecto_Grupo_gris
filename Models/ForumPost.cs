using System.ComponentModel.DataAnnotations;

namespace Proyecto_Grupo_gris.Models
{
    public class ForumPost
    {
        public int Id { get; set; }
        
        [Required]
        [Display(Name = "Tipo de reporte")]
        public string? ReportType { get; set; }
        
        [Required]
        [Display(Name = "Descripción")]
        public string? Description { get; set; }
        
        public string? ImageUrl { get; set; }
        
        public string? Location { get; set; }
        
        public string? Urgency { get; set; }
        
        public string? UserId { get; set; }
        public virtual Microsoft.AspNetCore.Identity.IdentityUser? User { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public int CommentsCount { get; set; } = 0;
        public int LikesCount { get; set; } = 0;
    }
}
