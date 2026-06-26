using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ControlVehiculos.Mvc.Models;
using System.Text;

namespace ControlVehiculos.Mvc.Controllers;

public class HomeController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly IWebHostEnvironment _env; // Permite acceder de forma nativa a la carpeta wwwroot [117]


    // Inyectamos la fábrica de conexiones HTTP apuntando a la API REST [102]
    public HomeController(IHttpClientFactory httpClientFactory, IWebHostEnvironment env) // El constructor DEBE recibir y asignar ambos parámetros [122]
    {
        _httpClient = httpClientFactory.CreateClient("VehiculosApi");
        _env = env; // ¡SÚPER IMPORTANTE!: Aquí se guarda la herramienta para poder usarla en todo el archivo [122]

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
    // =========================================================================
    // NUEVAS ACCIONES AGREGA-DAS (CRUD: EDITAR Y ELIMINAR)
    // =========================================================================

    // 1. GET: Home/Editar/{id} (Muestra el formulario con los datos actuales del auto)
    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        if (HttpContext.Session.GetInt32("TrabajadorId") == null) return RedirectToAction("Login", "Acceso");

        // Buscamos el auto actual de la API REST
        var response = await _httpClient.GetAsync("vehiculos");
        VehiculoViewModel vehiculo = null;
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            var lista = JsonSerializer.Deserialize<List<VehiculoViewModel>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            vehiculo = lista?.FirstOrDefault(v => v.Id == id);
        }
        if (vehiculo == null) return NotFound();

        // Cargamos el catálogo de estados para el Dropdown
        var responseEstados = await _httpClient.GetAsync("estados");
        var listaEstados = new List<EstadoVehiculoViewModel>();
        if (responseEstados.IsSuccessStatusCode)
        {
            var jsonEstados = await responseEstados.Content.ReadAsStringAsync();
            listaEstados = JsonSerializer.Deserialize<List<EstadoVehiculoViewModel>>(jsonEstados, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<EstadoVehiculoViewModel>();
        }

        ViewBag.Estados = listaEstados;
        return View(vehiculo);
    }

    // 2. POST: Home/Editar/{id} (Procesa la actualización y la envía con PUT a la API) [1.1]
    [HttpPost]
    public async Task<IActionResult> Editar(int id, VehiculoViewModel model, IFormFile archivoImagen)
    {
        if (HttpContext.Session.GetInt32("TrabajadorId") == null) return RedirectToAction("Login", "Acceso");

        // Si el trabajador decidió subir una nueva fotografía, la procesamos físicamente [117, 118]
        if (archivoImagen != null && archivoImagen.Length > 0)
        {
            string nombreUnico = Guid.NewGuid().ToString() + Path.GetExtension(archivoImagen.FileName);
            string carpetaImages = Path.Combine(_env.WebRootPath, "images");
            if (!Directory.Exists(carpetaImages)) Directory.CreateDirectory(carpetaImages);
            string rutaFisicaCompleta = Path.Combine(carpetaImages, nombreUnico);
            using (var stream = new FileStream(rutaFisicaCompleta, FileMode.Create))
            {
                await archivoImagen.CopyToAsync(stream);
            }
            model.ImagenUrl = "images/" + nombreUnico;
        }

        // Serializamos los cambios y hacemos la petición PUT a la API REST [101]
        var stringContent = new StringContent(JsonSerializer.Serialize(model), Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync($"vehiculos/{id}", stringContent);

        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction("Detalle", new { id = id }); // Te redirige de vuelta a ver la ficha técnica ya actualizada
        }

        ViewBag.Error = "Ocurrió un error al actualizar el vehículo.";
        return RedirectToAction("Editar", new { id = id });
    }

    // 3. POST: Home/Eliminar/{id} (Llamada asíncrona DELETE a la API para borrar el auto) [1.1]
    [HttpPost]
    public async Task<IActionResult> Eliminar(int id)
    {
        if (HttpContext.Session.GetInt32("TrabajadorId") == null) return RedirectToAction("Login", "Acceso");

        var response = await _httpClient.DeleteAsync($"vehiculos/{id}");
        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction("Index"); // Redirige al catálogo general tras borrar con éxito
        }

        return BadRequest("Error al eliminar el vehículo.");
    }
}
