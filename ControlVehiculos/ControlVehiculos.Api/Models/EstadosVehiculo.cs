using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ControlVehiculos.Api.Models
{
    [Table("EstadosVehiculo")] // Mapea globalmente esta clase a la tabla física de SQL
    public class EstadosVehiculo
    {
        [Key] // Indica que esta columna es la Llave Primaria (Primary Key)
        public int Id { get; set; }

        [Required] // Indica que la columna no acepta nulos (NOT NULL)
        [StringLength(50)] // Límite de caracteres equivalente a VARCHAR(50)
        public string Nombre { get; set; } = string.Empty;
    }
}