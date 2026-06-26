using System;

namespace ControlVehiculos.Mvc.Models;

public class VehiculoViewModel
{
    public int Id { get; set; }
    public string Marca { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public int ModeloAño { get; set; }
    public decimal PrecioCompra { get; set; }
    public decimal? PrecioReventa { get; set; }
    public DateTime FechaRecepcion { get; set; }
    public bool Vendido { get; set; }
    public string Descripcion { get; set; }
    public string ImagenUrl { get; set; }
    public int EstadoId { get; set; }
    public int TrabajadorId { get; set; }

    // Propiedades adicionales planas ya procesadas por la API para pintar en la tabla
    public string EstadoFisico { get; set; } = string.Empty;
    public string RegistradoPor { get; set; } = string.Empty;
}