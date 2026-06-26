namespace ControlVehiculos.Api.Services;

public class ValuadorService : IValuadorService
{
    public decimal CalcularPrecioReventaSugerido(decimal precioCompra, int estadoId)
    {
        // 1. Obtenemos el porcentaje de ganancia/riesgo según el daño
        decimal porcentajeRecargo = ObtenerPorcentajeRecargoPorEstado(estadoId);

        // 2. Calculamos el nuevo precio total
        decimal precioSugerido = precioCompra + (precioCompra * porcentajeRecargo);

        return precioSugerido;
    }

    private decimal ObtenerPorcentajeRecargoPorEstado(int estadoId)
    {
        // IDs de la DB: 1=Como nuevo, 2=Muy bueno, 3=Bueno, 4=Regular, 5=Dañado, 6=Muy dañado
        return estadoId switch
        {
            1 => 0.15m, // 15% de recargo
            2 => 0.20m, // 20%
            3 => 0.25m, // 25%
            4 => 0.35m, // 35%
            5 => 0.50m, // 50%
            6 => 0.70m, // 70%
            _ => 0.20m  // Por defecto (si envían un ID raro) aplica 20%
        };
    }
}