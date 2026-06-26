namespace ControlVehiculos.Api.Services;

public interface IValuadorService
{
    // Solo definimos qué entra (precio y estado) y qué sale (el precio calculado)
    decimal CalcularPrecioReventaSugerido(decimal precioCompra, int estadoId);
}