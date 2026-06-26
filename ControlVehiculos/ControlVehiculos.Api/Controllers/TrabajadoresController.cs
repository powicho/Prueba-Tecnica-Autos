using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ControlVehiculos.Api.Data;
using ControlVehiculos.Api.Models;

namespace ControlVehiculos.Api.Controllers;

[ApiController]
[Route("api/[controller]")] // La URL pública será: api/trabajadores
public class TrabajadoresController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TrabajadoresController(ApplicationDbContext context)
    {
        _context = context;
    }

    // 1. POST: api/trabajadores/login (Validación simple de credenciales)
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrEmpty(request.Correo) || string.IsNullOrEmpty(request.Contrasena))
        {
            return BadRequest("El correo y la contraseña son obligatorios.");
        }

        // Buscamos si existe un trabajador con ese correo y contraseña exactos
        var trabajador = await _context.Trabajadores
            .FirstOrDefaultAsync(t => t.Correo == request.Correo && t.Contrasena == request.Contrasena);

        if (trabajador == null)
        {
            return Unauthorized("Credenciales incorrectas.");
        }

        return Ok(trabajador); // Retorna los datos del trabajador (ID, Nombre, País) si es válido
    }

    // 2. GET: api/trabajadores/estadisticas (Métricas de rendimiento usando el SP analítico)
    [HttpGet("estadisticas")]
    public async Task<IActionResult> GetEstadisticas([FromQuery] int? trabajadorId)
    {
        var result = new List<EstadisticasTrabajadorDto>();

        // Abrimos de forma nativa la conexión para ejecutar el comando ADO.NET directamente
        using (var command = _context.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = "sp_ObtenerEstadisticasTrabajador";
            command.CommandType = CommandType.StoredProcedure; // Definimos que es un Stored Procedure

            // Agregamos el parámetro opcional
            var param = command.CreateParameter();
            param.ParameterName = "@TrabajadorId";

            // Si el ID es nulo, enviamos el equivalente a NULL en SQL Server (DBNull.Value)
            param.Value = (object)trabajadorId ?? DBNull.Value;
            command.Parameters.Add(param);

            // Abrimos la conexión física a la base de datos de forma asíncrona
            await _context.Database.OpenConnectionAsync();

            // Ejecutamos el comando y leemos las filas resultantes una por una
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    result.Add(new EstadisticasTrabajadorDto
                    {
                        TrabajadorId = reader.GetInt32(0),
                        NombreTrabajador = reader.GetString(1),
                        PaisTrabajador = reader.GetString(2),
                        TotalRegistrados = reader.GetInt32(3),
                        TotalEnVenta = reader.GetInt32(4),
                        TotalVendidos = reader.GetInt32(5),
                        GananciaTotalGenerada = reader.GetDecimal(6)
                    });
                }
            }
        }

        return Ok(result);
    }
}

// =========================================================================
// DTOS / CLASES DE TRANSFERENCIA DE DATOS (Nativas y Limpias)
// =========================================================================

public class LoginRequest
{
    public string Correo { get; set; } = string.Empty;
    public string Contrasena { get; set; } = string.Empty;
}

public class EstadisticasTrabajadorDto
{
    public int TrabajadorId { get; set; }
    public string NombreTrabajador { get; set; } = string.Empty;
    public string PaisTrabajador { get; set; } = string.Empty;
    public int TotalRegistrados { get; set; }
    public int TotalEnVenta { get; set; }
    public int TotalVendidos { get; set; }
    public decimal GananciaTotalGenerada { get; set; }
}