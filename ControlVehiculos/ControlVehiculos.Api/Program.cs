// Importamos los namespaces necesarios
using Microsoft.EntityFrameworkCore;
using ControlVehiculos.Api.Data;
// Importamos el namespace de servicios
using ControlVehiculos.Api.Services;


var builder = WebApplication.CreateBuilder(args);

// =========================================================================
// REGISTRO DEL DBCONTEXT CON LA CADENA DE CONEXIÓN (Inyección de Dependencias)
// =========================================================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// =========================================================================
// REGISTRO DEL SERVICIO VALUADOR
// =========================================================================
builder.Services.AddSingleton<IValuadorService, ValuadorService>();





// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
