namespace Proyecto_Grupo_gris.Models
{
    public class Prize
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int PointCost { get; set; }
        public string IconName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string BadgeText { get; set; } = string.Empty;
        public string GradientFrom { get; set; } = string.Empty;
        public string GradientTo { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
