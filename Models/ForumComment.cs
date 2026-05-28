using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Proyecto_Grupo_gris.Models
{
    public class ForumComment
    {
        public int Id { get; set; }

        public int PostId { get; set; }
        public virtual ForumPost? Post { get; set; }

        public string? UserId { get; set; }
        public virtual IdentityUser? User { get; set; }

        [Required]
        public string Text { get; set; } = string.Empty;

        public string? Sentiment { get; set; }
        
        public float SentimentConfidence { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
