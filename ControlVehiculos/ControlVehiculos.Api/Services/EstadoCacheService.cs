using Microsoft.EntityFrameworkCore;
using ControlVehiculos.Api.Data;
using ControlVehiculos.Api.Models;

namespace ControlVehiculos.Api.Services;

// Implementamos la interfaz para cumplir con el contrato establecido
public class EstadoCacheService : IEstadoCacheService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private List<EstadosVehiculo> _cache;

    public EstadoCacheService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task<List<EstadosVehiculo>> ObtenerEstadosAsync()
    {
        // 1. Si la lista en memoria ya existe, la devolvemos inmediatamente
        if (_cache != null)
        {
            return _cache;
        }

        // 2. Si es la primera vez, creamos un scope temporal seguro para usar el DbContext
        using (var scope = _scopeFactory.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            _cache = await context.EstadosVehiculo.ToListAsync();
        }

        return _cache;
    }

    public void LimpiarCache()
    {
        _cache = null; // Reinicia la caché si es necesario recargar datos reales
    }
}