using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ControlVehiculos.Api.Data;
using ControlVehiculos.Api.Models;
using ControlVehiculos.Api.Services;

namespace ControlVehiculos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VehiculosController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IValuadorService _valuador;

    public VehiculosController(ApplicationDbContext context, IValuadorService valuador)
    {
        _context = context;
        _valuador = valuador;
    }

    // 1. GET: api/vehiculos (Consulta dinámica usando lectura nativa ultra rápida)
    [HttpGet]
    public async Task<IActionResult> GetVehiculos(
        [FromQuery] int? estadoId,
        [FromQuery] bool? vendido,
        [FromQuery] string? buscar,
        [FromQuery] int? trabajadorId)
    {
        var result = new List<VehiculoDto>();

        // Usamos el comando nativo para ejecutar el SP de forma asíncrona libre de restricciones de mapeo [104]
        using (var command = _context.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = "sp_ConsultarVehiculos";
            command.CommandType = CommandType.StoredProcedure;

            // Agregamos los parámetros opcionales de forma segura
            var p1 = command.CreateParameter(); p1.ParameterName = "@EstadoId"; p1.Value = (object)estadoId ?? DBNull.Value; command.Parameters.Add(p1);
            var p2 = command.CreateParameter(); p2.ParameterName = "@Vendido"; p2.Value = (object)vendido ?? DBNull.Value; command.Parameters.Add(p2);
            var p3 = command.CreateParameter(); p3.ParameterName = "@TextoBuscar"; p3.Value = (object)buscar ?? DBNull.Value; command.Parameters.Add(p3);
            var p4 = command.CreateParameter(); p4.ParameterName = "@TrabajadorId"; p4.Value = (object)trabajadorId ?? DBNull.Value; command.Parameters.Add(p4);

            await _context.Database.OpenConnectionAsync();

            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    result.Add(new VehiculoDto
                    {
                        Id = reader.GetInt32(0),
                        Marca = reader.GetString(1),
                        Color = reader.GetString(2),
                        ModeloAño = reader.GetInt32(3),
                        PrecioCompra = reader.GetDecimal(4),
                        PrecioReventa = reader.IsDBNull(5) ? null : (decimal?)reader.GetDecimal(5),
                        FechaRecepcion = reader.GetDateTime(6),
                        Vendido = reader.GetBoolean(7),
                        Descripcion = reader.IsDBNull(8) ? null : reader.GetString(8),
                        ImagenUrl = reader.IsDBNull(9) ? null : reader.GetString(9),
                        EstadoFisico = reader.IsDBNull(10) ? string.Empty : reader.GetString(10),
                        RegistradoPor = reader.IsDBNull(11) ? string.Empty : reader.GetString(11)
                    });
                }
            }
        }

        return Ok(result);
    }

    // 2. POST: api/vehiculos (Registro individual con valuación automática)
    [HttpPost]
    public async Task<IActionResult> RegistrarVehiculo([FromBody] Vehiculo vehiculo)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        vehiculo.PrecioReventa = _valuador.CalcularPrecioReventaSugerido(vehiculo.PrecioCompra, vehiculo.EstadoId);
        vehiculo.FechaRecepcion = DateTime.Now;

        _context.Vehiculos.Add(vehiculo);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetVehiculos), new { id = vehiculo.Id }, vehiculo);
    }
}

// =========================================================================
// DTO LOCAL PARA EL RETORNO DE LA API
// =========================================================================
public class VehiculoDto
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
    public string EstadoFisico { get; set; } = string.Empty;
    public string RegistradoPor { get; set; } = string.Empty;
}