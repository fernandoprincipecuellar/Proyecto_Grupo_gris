namespace Proyecto_Grupo_gris.Models;

public class DashboardViewModel
{
    public int ReciclajeTotalKg { get; set; }
    public string Crecimiento { get; set; } = string.Empty;
    public int Entregas { get; set; }
    public int Puntos { get; set; }
    public string RecolectorNombre { get; set; } = string.Empty;
    public string RecolectorDireccion { get; set; } = string.Empty;
    public string Distancia { get; set; } = string.Empty;
    public int Co2AhoradoKg { get; set; }
    public int ArbolesSalvados { get; set; }
    public int AguaAhorradaL { get; set; }
}