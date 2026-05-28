using Microsoft.AspNetCore.Http;

namespace Proyecto_Grupo_gris.Api.Services.Interfaces;

public interface ICloudinaryService
{
    Task<string?> UploadImageAsync(IFormFile file, string? folder = null);
}
