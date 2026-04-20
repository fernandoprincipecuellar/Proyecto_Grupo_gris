namespace Proyecto_Grupo_gris.Models;

public class DashboardViewModel
{
    public double ReciclajeTotalKg { get; set; }
    public string Crecimiento { get; set; } = string.Empty;
    public int Entregas { get; set; }
    public int Puntos { get; set; }
    public string RecolectorNombre { get; set; } = string.Empty;
    public string RecolectorDireccion { get; set; } = string.Empty;
    public string Distancia { get; set; } = string.Empty;
    public double Co2AhorradoKg { get; set; }
    public int ArbolesSalvados { get; set; }
    public int AguaAhorradaL { get; set; }

    public string NombreUsuario { get; set; } = string.Empty;
    public string CargoUsuario { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public string PerfilIniciales { get; set; } = string.Empty;
    public string Logro1Titulo { get; set; } = string.Empty;
    public string Logro1Descripcion { get; set; } = string.Empty;
    public string Logro2Titulo { get; set; } = string.Empty;
    public string Logro2Descripcion { get; set; } = string.Empty;
}