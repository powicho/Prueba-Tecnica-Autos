using System;

namespace ControlVehiculos.Mvc.Models;

public class VehiculoViewModel
{
    public int Id { get; set; }

    // Datos obligatorios en el formulario
    public string Marca { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public int ModeloAño { get; set; }
    public decimal PrecioCompra { get; set; }
    public int EstadoId { get; set; }

    // Datos autogenerados o calculados por el sistema (Deben ser opcionales con '?' para el validador) [103]
    public decimal? PrecioReventa { get; set; }
    public DateTime FechaRecepcion { get; set; }
    public bool Vendido { get; set; }
    public string? Descripcion { get; set; }
    public string? ImagenUrl { get; set; }
    public int TrabajadorId { get; set; }

    // Propiedades de lectura obtenidas de la API (Deben ser opcionales) [103]
    public string? EstadoFisico { get; set; }
    public string? RegistradoPor { get; set; }
}