namespace Proyecto_Grupo_gris.Api.DTOs.Weather;

public class WeatherDto
{
    public string Condition { get; set; } = string.Empty;
    public double TemperatureC { get; set; }
    public int Humidity { get; set; }
    public double WindSpeed { get; set; }
}
