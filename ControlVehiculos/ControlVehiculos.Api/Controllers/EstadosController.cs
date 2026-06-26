using Microsoft.AspNetCore.Mvc;
using ControlVehiculos.Api.Services;

namespace ControlVehiculos.Api.Controllers;

[ApiController]
[Route("api/[controller]")] // La URL global será: api/estados
public class EstadosController : ControllerBase
{
    private readonly IEstadoCacheService _cacheService;

    // Inyectamos nuestro Singleton de caché
    public EstadosController(IEstadoCacheService cacheService)
    {
        _cacheService = cacheService;
    }

    [HttpGet]
    public async Task<IActionResult> GetEstados()
    {
        var estados = await _cacheService.ObtenerEstadosAsync();
        return Ok(estados); // Retorna la lista de la caché en formato JSON
    }
}