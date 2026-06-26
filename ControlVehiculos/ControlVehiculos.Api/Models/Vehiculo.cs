using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ControlVehiculos.Api.Models
{
    [Table("Vehiculos")]
    public class Vehiculo
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Marca { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Color { get; set; } = string.Empty;

        [Required]
        public int ModeloAnio { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")] // Mapea al tipo de datos exacto de SQL Server
        public decimal PrecioCompra { get; set; }

        public decimal? PrecioReventa { get; set; } // El signo "?" indica que permite valores nulos (null)

        public DateTime FechaRecepcion { get; set; } = DateTime.Now;

        public bool Vendido { get; set; } = false; // El tipo 'bool' de C# se mapea automáticamente a 'BIT' en SQL Server

        public string Descripcion { get; set; }

        [StringLength(500)]
        public string ImagenUrl { get; set; }

        // =========================================================================
        //  -- RELACIONES (CLAVES FORÁNEAS)
        // =========================================================================

        public int EstadoId { get; set; }
        
        [ForeignKey("EstadoId")] // Indica la regla de integridad de llave foránea
        public virtual EstadosVehiculo Estado { get; set; }

        public int TrabajadorId { get; set; }
        
        [ForeignKey("TrabajadorId")]
        public virtual Trabajador Trabajador { get; set; }
    }
}