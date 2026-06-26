using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ControlVehiculos.Mvc.Models;

namespace ControlVehiculos.Mvc.Controllers;

public class PerfilController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly IWebHostEnvironment _env; // Permite acceder de forma nativa a la carpeta wwwroot [117]

    public PerfilController(IHttpClientFactory httpClientFactory, IWebHostEnvironment env)
    {
        _httpClient = httpClientFactory.CreateClient("VehiculosApi");
        _env = env;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var trabajadorId = HttpContext.Session.GetInt32("TrabajadorId");
        if (trabajadorId == null) return RedirectToAction("Login", "Acceso");

        var responseStats = await _httpClient.GetAsync($"trabajadores/estadisticas?trabajadorId={trabajadorId}");
        var estadisticas = new EstadisticasTrabajadorViewModel();

        if (responseStats.IsSuccessStatusCode)
        {
            var json = await responseStats.Content.ReadAsStringAsync();
            var listaStats = JsonSerializer.Deserialize<List<EstadisticasTrabajadorViewModel>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            estadisticas = listaStats?.FirstOrDefault() ?? new EstadisticasTrabajadorViewModel();
        }

        var responseVehiculos = await _httpClient.GetAsync($"vehiculos?trabajadorId={trabajadorId}");
        var listaVehiculos = new List<VehiculoViewModel>();

        if (responseVehiculos.IsSuccessStatusCode)
        {
            var json = await responseVehiculos.Content.ReadAsStringAsync();
            listaVehiculos = JsonSerializer.Deserialize<List<VehiculoViewModel>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<VehiculoViewModel>();
        }

        ViewBag.Estadisticas = estadisticas;
        ViewBag.TrabajadorNombre = HttpContext.Session.GetString("TrabajadorNombre");
        ViewBag.TrabajadorPais = HttpContext.Session.GetString("TrabajadorPais");

        return View(listaVehiculos);
    }

    [HttpGet]
    public async Task<IActionResult> Registrar()
    {
        if (HttpContext.Session.GetInt32("TrabajadorId") == null) return RedirectToAction("Login", "Acceso");

        var responseEstados = await _httpClient.GetAsync("estados");
        var listaEstados = new List<EstadoVehiculoViewModel>();

        if (responseEstados.IsSuccessStatusCode)
        {
            var jsonEstados = await responseEstados.Content.ReadAsStringAsync();
            listaEstados = JsonSerializer.Deserialize<List<EstadoVehiculoViewModel>>(jsonEstados, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<EstadoVehiculoViewModel>();
        }

        ViewBag.Estados = listaEstados;
        return View();
    }

    // PROCESAMOS EL ARCHIVO SUBIDO DESDE EL EXPLORADOR DE ARCHIVOS DE WINDOWS (IFormFile)
    [HttpPost]
    public async Task<IActionResult> Registrar(VehiculoViewModel model, IFormFile archivoImagen)
    {
        var trabajadorId = HttpContext.Session.GetInt32("TrabajadorId");
        if (trabajadorId == null) return RedirectToAction("Login", "Acceso");

        // Método auxiliar local para recargar los estados de forma segura si algo falla
        async Task RecargarEstados()
        {
            var responseEstados = await _httpClient.GetAsync("estados");
            var listaEstados = new List<EstadoVehiculoViewModel>();
            if (responseEstados.IsSuccessStatusCode)
            {
                var jsonEstados = await responseEstados.Content.ReadAsStringAsync();
                listaEstados = JsonSerializer.Deserialize<List<EstadoVehiculoViewModel>>(jsonEstados, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<EstadoVehiculoViewModel>();
            }
            ViewBag.Estados = listaEstados;
        }

        if (archivoImagen != null && archivoImagen.Length > 0)
        {
            string nombreUnico = Guid.NewGuid().ToString() + Path.GetExtension(archivoImagen.FileName);
            string carpetaImages = Path.Combine(_env.WebRootPath, "images");
            if (!Directory.Exists(carpetaImages))
            {
                Directory.CreateDirectory(carpetaImages);
            }
            string rutaFisicaCompleta = Path.Combine(carpetaImages, nombreUnico);
            using (var stream = new FileStream(rutaFisicaCompleta, FileMode.Create))
            {
                await archivoImagen.CopyToAsync(stream);
            }
            model.ImagenUrl = "images/" + nombreUnico;
        }

        if (!ModelState.IsValid)
        {
            await RecargarEstados(); // ¡Carga defensiva!
            return View(model);
        }

        model.TrabajadorId = trabajadorId.Value;

        var stringContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("vehiculos", stringContent);

        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction("Index");
        }

        // ¡AQUÍ ESTABA EL DETALLE!: Si la API falla, recargamos los estados antes de retornar la vista
        await RecargarEstados(); // ¡Carga defensiva!
        ViewBag.Error = "Ocurrió un error al registrar el vehículo en el servidor.";
        return View(model);
    }
}