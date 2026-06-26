namespace ControlVehiculos.Mvc.Models;

public class EstadisticasTrabajadorViewModel
{
    public int TrabajadorId { get; set; }
    public string NombreTrabajador { get; set; } = string.Empty;
    public string PaisTrabajador { get; set; } = string.Empty;
    public int TotalRegistrados { get; set; }
    public int TotalEnVenta { get; set; }
    public int TotalVendidos { get; set; }
    public decimal GananciaTotalGenerada { get; set; }
}

public class EstadoVehiculoViewModel
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}