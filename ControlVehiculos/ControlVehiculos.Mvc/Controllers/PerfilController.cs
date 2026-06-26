using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ControlVehiculos.Mvc.Models;

namespace ControlVehiculos.Mvc.Controllers;

public class PerfilController : Controller
{
    private readonly HttpClient _httpClient;

    // Inyectamos el cliente HTTP configurado [102]
    public PerfilController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("VehiculosApi");
    }

    // 1. GET: Perfil/Index (Vista 3: Perfil, KPIs y Mis Registros)
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var trabajadorId = HttpContext.Session.GetInt32("TrabajadorId");
        if (trabajadorId == null)
        {
            return RedirectToAction("Login", "Acceso");
        }

        // A. CONSUMIR SERVICIO REST: Obtener estadísticas analíticas del trabajador (sp_ObtenerEstadisticasTrabajador)
        var responseStats = await _httpClient.GetAsync($"trabajadores/estadisticas?trabajadorId={trabajadorId}");
        var estadisticas = new EstadisticasTrabajadorViewModel();

        if (responseStats.IsSuccessStatusCode)
        {
            var json = await responseStats.Content.ReadAsStringAsync();
            var listaStats = JsonSerializer.Deserialize<List<EstadisticasTrabajadorViewModel>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            estadisticas = listaStats?.FirstOrDefault() ?? new EstadisticasTrabajadorViewModel();
        }

        // B. CONSUMIR SERVICIO REST: Obtener la lista exclusiva de vehículos registrados por este trabajador
        var responseVehiculos = await _httpClient.GetAsync($"vehiculos?trabajadorId={trabajadorId}");
        var listaVehiculos = new List<VehiculoViewModel>();

        if (responseVehiculos.IsSuccessStatusCode)
        {
            var json = await responseVehiculos.Content.ReadAsStringAsync();
            listaVehiculos = JsonSerializer.Deserialize<List<VehiculoViewModel>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<VehiculoViewModel>();
        }

        // Pasamos las estadísticas y datos personales en el ViewBag, y los vehículos como el modelo de la vista [3]
        ViewBag.Estadisticas = estadisticas;
        ViewBag.TrabajadorNombre = HttpContext.Session.GetString("TrabajadorNombre");
        ViewBag.TrabajadorPais = HttpContext.Session.GetString("TrabajadorPais");

        return View(listaVehiculos);
    }

    // 2. GET: Perfil/Registrar (Vista 5: Formulario de Registro de Vehículo)
    [HttpGet]
    public async Task<IActionResult> Registrar()
    {
        if (HttpContext.Session.GetInt32("TrabajadorId") == null)
        {
            return RedirectToAction("Login", "Acceso");
        }

        // Consumimos el catálogo de estados desde la caché de la API para llenar el selector [3]
        var responseEstados = await _httpClient.GetAsync("estados");
        var listaEstados = new List<EstadoVehiculoViewModel>();

        if (responseEstados.IsSuccessStatusCode)
        {
            var jsonEstados = await responseEstados.Content.ReadAsStringAsync();
            listaEstados = JsonSerializer.Deserialize<List<EstadoVehiculoViewModel>>(jsonEstados, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<EstadoVehiculoViewModel>();
        }

        ViewBag.Estados = listaEstados;
        return View();
    }

    // 3. POST: Perfil/Registrar (Procesa el registro del vehículo y lo manda a la API)
    [HttpPost]
    public async Task<IActionResult> Registrar(VehiculoViewModel model)
    {
        var trabajadorId = HttpContext.Session.GetInt32("TrabajadorId");
        if (trabajadorId == null)
        {
            return RedirectToAction("Login", "Acceso");
        }

        // CONTROL DE ERRORES: Validación de modelo nativa antes de enviar los datos [103]
        if (!ModelState.IsValid)
        {
            var responseEstados = await _httpClient.GetAsync("estados");
            var listaEstados = new List<EstadoVehiculoViewModel>();
            if (responseEstados.IsSuccessStatusCode)
            {
                var jsonEstados = await responseEstados.Content.ReadAsStringAsync();
                listaEstados = JsonSerializer.Deserialize<List<EstadoVehiculoViewModel>>(jsonEstados, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<EstadoVehiculoViewModel>();
            }
            ViewBag.Estados = listaEstados;
            return View(model);
        }

        // Asignamos de forma interna el ID del trabajador que está registrando el vehículo
        model.TrabajadorId = trabajadorId.Value;

        // Convertimos el modelo de C# a formato JSON de forma nativa [101]
        var stringContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");

        // Enviamos el objeto JSON a la API REST mediante una petición POST [101]
        var response = await _httpClient.PostAsync("vehiculos", stringContent);

        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction("Index"); // Redirige a su perfil para que vea su lista actualizada
        }

        ViewBag.Error = "Ocurrió un error al registrar el vehículo en el servidor.";
        return View(model);
    }
}