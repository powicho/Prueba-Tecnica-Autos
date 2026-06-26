using Microsoft.EntityFrameworkCore;
using ControlVehiculos.Api.Models;

namespace ControlVehiculos.Api.Data
{
    // Heredamos de la clase base DbContext de Entity Framework
    public class ApplicationDbContext : DbContext
    {
        // Constructor global obligatorio para recibir la configuración de conexión
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets: Son las propiedades que representan físicamente a tus tablas
        public DbSet<EstadosVehiculo> EstadosVehiculo { get; set; }
        public DbSet<Trabajador> Trabajadores { get; set; }
        public DbSet<Vehiculo> Vehiculos { get; set; }
    }
}