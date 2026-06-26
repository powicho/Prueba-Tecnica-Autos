using ControlVehiculos.Api.Models;

namespace ControlVehiculos.Api.Services;

// Definimos el contrato de nuestro servicio
public interface IEstadoCacheService
{
    Task<List<EstadosVehiculo>> ObtenerEstadosAsync();
    void LimpiarCache();
}