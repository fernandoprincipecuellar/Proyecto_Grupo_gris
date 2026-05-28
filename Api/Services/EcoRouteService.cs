using AutoMapper;
using Proyecto_Grupo_gris.Api.DTOs;
using Proyecto_Grupo_gris.Api.DTOs.EcoRoutes;
using Proyecto_Grupo_gris.Api.Repositories.Interfaces;
using Proyecto_Grupo_gris.Api.Services.Interfaces;
using Proyecto_Grupo_gris.Models;

namespace Proyecto_Grupo_gris.Api.Services;

public class EcoRouteService : IEcoRouteService
{
    private readonly IEcoRouteRepository _repository;
    private readonly IMapper _mapper;

    public EcoRouteService(IEcoRouteRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PaginatedResultDto<EcoRouteDto>> GetPagedAsync(int page, int pageSize, string? search)
    {
        var routes = await _repository.GetPagedAsync(page, pageSize, search);
        var total = await _repository.CountAsync(search);
        return new PaginatedResultDto<EcoRouteDto>
        {
            Items = _mapper.Map<IEnumerable<EcoRouteDto>>(routes),
            Page = page,
            PageSize = pageSize,
            TotalItems = total
        };
    }

    public async Task<EcoRouteDto?> GetByIdAsync(int id)
    {
        var route = await _repository.GetByIdAsync(id);
        return route == null ? null : _mapper.Map<EcoRouteDto>(route);
    }

    public async Task<EcoRouteDto> CreateAsync(CreateEcoRouteDto request, string userId)
    {
        var ecoRoute = _mapper.Map<EcoRoute>(request);
        ecoRoute.CreatedById = userId;
        ecoRoute.CreatedAt = DateTime.UtcNow;
        ecoRoute.UpdatedAt = DateTime.UtcNow;
        var created = await _repository.AddAsync(ecoRoute);
        return _mapper.Map<EcoRouteDto>(created);
    }

    public async Task<EcoRouteDto?> UpdateAsync(UpdateEcoRouteDto request, string userId)
    {
        var existing = await _repository.GetByIdAsync(request.Id);
        if (existing == null) return null;
        if (existing.CreatedById != userId) throw new UnauthorizedAccessException("No tienes permiso para actualizar esta ruta.");

        existing.Title = request.Title;
        existing.Description = request.Description;
        existing.StartLocation = request.StartLocation;
        existing.EndLocation = request.EndLocation;
        existing.DistanceKm = request.DistanceKm;
        existing.Difficulty = request.Difficulty;
        existing.IsPublished = request.IsPublished;
        existing.UpdatedAt = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(existing);
        return _mapper.Map<EcoRouteDto>(updated);
    }

    public async Task DeleteAsync(int id, string userId)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null) return;
        if (existing.CreatedById != userId) throw new UnauthorizedAccessException("No tienes permiso para eliminar esta ruta.");
        await _repository.DeleteAsync(existing);
    }
}
