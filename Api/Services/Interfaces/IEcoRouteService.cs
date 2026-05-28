using Proyecto_Grupo_gris.Api.DTOs;
using Proyecto_Grupo_gris.Api.DTOs.EcoRoutes;

namespace Proyecto_Grupo_gris.Api.Services.Interfaces;

public interface IEcoRouteService
{
    Task<PaginatedResultDto<EcoRouteDto>> GetPagedAsync(int page, int pageSize, string? search);
    Task<EcoRouteDto?> GetByIdAsync(int id);
    Task<EcoRouteDto> CreateAsync(CreateEcoRouteDto request, string userId);
    Task<EcoRouteDto?> UpdateAsync(UpdateEcoRouteDto request, string userId);
    Task DeleteAsync(int id, string userId);
}
