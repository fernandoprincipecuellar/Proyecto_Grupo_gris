using System;
using Microsoft.AspNetCore.Identity;

namespace Proyecto_Grupo_gris.Models
{
    public class ForumLike
    {
        public int Id { get; set; }

        public int PostId { get; set; }
        public virtual ForumPost? Post { get; set; }

        public string? UserId { get; set; }
        public virtual IdentityUser? User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
