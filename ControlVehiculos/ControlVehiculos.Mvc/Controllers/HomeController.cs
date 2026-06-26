using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ControlVehiculos.Mvc.Models;

namespace ControlVehiculos.Mvc.Controllers;

public class HomeController : Controller
{
    private readonly HttpClient _httpClient;

    // Inyectamos la fábrica de conexiones HTTP apuntando a la API REST [102]
    public HomeController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("VehiculosApi");
    }

    // 1. GET: Home/Index (Vista 2: El Catálogo / Dashboard principal con Filtros)
    [HttpGet]
    public async Task<IActionResult> Index(int? estadoId, bool? vendido, string? buscar)
    {
        // CONTROL DE ACCESO GLOBAL: Si el trabajador no ha iniciado sesión, lo redirigimos al login [3]
        if (HttpContext.Session.GetInt32("TrabajadorId") == null)
        {
            return RedirectToAction("Login", "Acceso");
        }

        // A. CONSUMIR SERVICIO REST: Obtener la lista de vehículos filtrada
        var urlVehiculos = $"vehiculos?estadoId={estadoId}&vendido={vendido}&buscar={buscar}";
        var responseVehiculos = await _httpClient.GetAsync(urlVehiculos);
        var listaVehiculos = new List<VehiculoViewModel>();

        if (responseVehiculos.IsSuccessStatusCode)
        {
            var json = await responseVehiculos.Content.ReadAsStringAsync();
            listaVehiculos = JsonSerializer.Deserialize<List<VehiculoViewModel>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<VehiculoViewModel>();
        }

        // B. CONSUMIR SERVICIO REST: Obtener catálogo de estados para llenar el Dropdown del Filtro (Usa la caché Singleton de la API)
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

        // Guardamos los filtros seleccionados, los estados y el nombre del trabajador en el ViewBag para enviarlos a la vista [3]
        ViewBag.Estados = listaEstados;
        ViewBag.FiltroEstadoId = estadoId;
        ViewBag.FiltroVendido = vendido;
        ViewBag.FiltroBuscar = buscar;
        ViewBag.TrabajadorNombre = HttpContext.Session.GetString("TrabajadorNombre");

        return View(listaVehiculos); // Pasa la lista de autos a la vista Index.cshtml [3]
    }

    // 2. GET: Home/Detalle/{id} (Vista 4: Ficha Técnica del Vehículo)
    [HttpGet]
    public async Task<IActionResult> Detalle(int id)
    {
        // CONTROL DE ACCESO GLOBAL
        if (HttpContext.Session.GetInt32("TrabajadorId") == null)
        {
            return RedirectToAction("Login", "Acceso");
        }

        // Consumimos todos los vehículos y buscamos el específico por ID usando LINQ de forma nativa en memoria
        var response = await _httpClient.GetAsync("vehiculos");
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            var lista = JsonSerializer.Deserialize<List<VehiculoViewModel>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            var vehiculo = lista?.FirstOrDefault(v => v.Id == id);
            if (vehiculo != null)
            {
                return View(vehiculo); // Pasa el carro específico a la vista Detalle.cshtml [3]
            }
        }

        return NotFound();
    }
}