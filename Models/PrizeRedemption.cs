namespace Proyecto_Grupo_gris.Models
{
    public class PrizeRedemption
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int PrizeId { get; set; }
        public int PointsSpent { get; set; }
        public DateTime RedeemedAt { get; set; } = DateTime.UtcNow;

        public virtual ApplicationUser User { get; set; } = null!;
        public virtual Prize Prize { get; set; } = null!;
    }
}
