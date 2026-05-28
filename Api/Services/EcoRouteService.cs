using AutoMapper;
using Proyecto_Grupo_gris.Api.DTOs;
using Proyecto_Grupo_gris.Api.DTOs.EcoRoutes;
using Proyecto_Grupo_gris.Api.DTOs.Weather;
using Proyecto_Grupo_gris.Api.Repositories.Interfaces;
using Proyecto_Grupo_gris.Api.Services.Interfaces;
using Proyecto_Grupo_gris.Models;

namespace Proyecto_Grupo_gris.Api.Services;

public class EcoRouteService : IEcoRouteService
{
    private readonly IEcoRouteRepository _repository;
    private readonly IMapper _mapper;
    private readonly IGoogleMapsService _googleMapsService;
    private readonly IWeatherService _weatherService;

    public EcoRouteService(
        IEcoRouteRepository repository,
        IMapper mapper,
        IGoogleMapsService googleMapsService,
        IWeatherService weatherService)
    {
        _repository = repository;
        _mapper = mapper;
        _googleMapsService = googleMapsService;
        _weatherService = weatherService;
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
        var routeData = await _googleMapsService.GetRouteDataAsync(request.StartLocation, request.EndLocation);
        var weather = await SafeGetWeatherAsync(request.StartLocation);

        var ecoRoute = _mapper.Map<EcoRoute>(request);
        ecoRoute.CreatedById = userId;
        ecoRoute.CreatedAt = DateTime.UtcNow;
        ecoRoute.UpdatedAt = DateTime.UtcNow;
        ecoRoute.DistanceKm = routeData.DistanceKm;
        ecoRoute.StartCoordinates = $"{routeData.StartLocation.Lat},{routeData.StartLocation.Lng}";
        ecoRoute.EndCoordinates = $"{routeData.EndLocation.Lat},{routeData.EndLocation.Lng}";
        ecoRoute.MapUrl = routeData.MapUrl;
        ecoRoute.WeatherCondition = weather.Condition;
        ecoRoute.WeatherTemperatureC = weather.TemperatureC;
        ecoRoute.WeatherHumidity = weather.Humidity;
        ecoRoute.WeatherWindSpeed = weather.WindSpeed;

        var created = await _repository.AddAsync(ecoRoute);
        return _mapper.Map<EcoRouteDto>(created);
    }

    public async Task<EcoRouteDto?> UpdateAsync(UpdateEcoRouteDto request, string userId)
    {
        var existing = await _repository.GetByIdAsync(request.Id);
        if (existing == null) return null;
        if (existing.CreatedById != userId) throw new UnauthorizedAccessException("No tienes permiso para actualizar esta ruta.");

        var hasLocationChanged = !string.Equals(existing.StartLocation, request.StartLocation, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(existing.EndLocation, request.EndLocation, StringComparison.OrdinalIgnoreCase);

        existing.Title = request.Title;
        existing.Description = request.Description;
        existing.StartLocation = request.StartLocation;
        existing.EndLocation = request.EndLocation;
        existing.Difficulty = request.Difficulty;
        existing.IsPublished = request.IsPublished;
        existing.UpdatedAt = DateTime.UtcNow;

        if (hasLocationChanged)
        {
            var routeData = await _googleMapsService.GetRouteDataAsync(request.StartLocation, request.EndLocation);
            var weather = await SafeGetWeatherAsync(request.StartLocation);
            existing.DistanceKm = routeData.DistanceKm;
            existing.StartCoordinates = $"{routeData.StartLocation.Lat},{routeData.StartLocation.Lng}";
            existing.EndCoordinates = $"{routeData.EndLocation.Lat},{routeData.EndLocation.Lng}";
            existing.MapUrl = routeData.MapUrl;
            existing.WeatherCondition = weather.Condition;
            existing.WeatherTemperatureC = weather.TemperatureC;
            existing.WeatherHumidity = weather.Humidity;
            existing.WeatherWindSpeed = weather.WindSpeed;
        }

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

    private async Task<WeatherDto> SafeGetWeatherAsync(string location)
    {
        try
        {
            return await _weatherService.GetCurrentWeatherAsync(location);
        }
        catch
        {
            return new WeatherDto
            {
                Condition = "Unavailable",
                TemperatureC = 0,
                Humidity = 0,
                WindSpeed = 0
            };
        }
    }
}
