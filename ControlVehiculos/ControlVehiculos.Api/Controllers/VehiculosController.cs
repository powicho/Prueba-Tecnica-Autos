using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ControlVehiculos.Api.Data;
using ControlVehiculos.Api.Models;
using ControlVehiculos.Api.Services;

namespace ControlVehiculos.Api.Controllers;

[ApiController]
[Route("api/[controller]")] // La URL pública será: api/vehiculos
public class VehiculosController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IValuadorService _valuador;

    // Inyectamos el DbContext para interactuar con SQL y el Valuador para la matemática
    public VehiculosController(ApplicationDbContext context, IValuadorService valuador)
    {
        _context = context;
        _valuador = valuador;
    }

    // 1. GET: api/vehiculos (Consulta dinámica usando tu Procedimiento Almacenado)
    [HttpGet]
    public async Task<IActionResult> GetVehiculos(
        [FromQuery] int? estadoId,
        [FromQuery] bool? vendido,
        [FromQuery] string? buscar,
        [FromQuery] int? trabajadorId)
    {
        // Ejecutamos tu SP 'sp_ConsultarVehiculos' mapeándolo directamente con Entity Framework
        var vehiculos = await _context.Vehiculos
            .FromSqlRaw("EXEC sp_ConsultarVehiculos @EstadoId={0}, @Vendido={1}, @TextoBuscar={2}, @TrabajadorId={3}",
                         estadoId, vendido, buscar, trabajadorId)
            .ToListAsync();

        return Ok(vehiculos); // Devuelve la lista resultante en formato JSON
    }

    // 2. POST: api/vehiculos (Registro de un vehículo aplicando valuación automática)
    [HttpPost]
    public async Task<IActionResult> RegistrarVehiculo([FromBody] Vehiculo vehiculo)
    {
        // Validación nativa de C# para comprobar que todos los campos obligatorios cumplan con los límites
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // AUTOMATIZACIÓN: Calculamos y asignamos el precio de reventa sugerido antes de guardar
        vehiculo.PrecioReventa = _valuador.CalcularPrecioReventaSugerido(vehiculo.PrecioCompra, vehiculo.EstadoId);

        // Forzamos a que la fecha de recepción sea la actual del servidor
        vehiculo.FechaRecepcion = DateTime.Now;

        // Agregamos y guardamos el cambio físicamente en SQL Server de Docker
        _context.Vehiculos.Add(vehiculo);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetVehiculos), new { id = vehiculo.Id }, vehiculo);
    }
}