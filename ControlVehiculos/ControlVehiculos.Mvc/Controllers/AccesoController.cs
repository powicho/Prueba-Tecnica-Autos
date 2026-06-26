using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using ControlVehiculos.Mvc.Models;

namespace ControlVehiculos.Mvc.Controllers;

public class AccesoController : Controller
{
    private readonly HttpClient _httpClient;

    // Inyectamos el cliente HTTP que configuramos previamente
    public AccesoController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("VehiculosApi");
    }

    // 1. GET: Acceso/Login (Muestra la pantalla de inicio de sesión)
    [HttpGet]
    public IActionResult Login()
    {
        // Si el trabajador ya tiene una sesión activa, lo mandamos directo al catálogo (Dashboard)
        if (HttpContext.Session.GetInt32("TrabajadorId") != null)
        {
            return RedirectToAction("Index", "Home");
        }
        return View();
    }

    // 2. POST: Acceso/Login (Procesa el formulario enviando los datos a la API REST)
    [HttpPost]
    public async Task<IActionResult> Login(LoginRequestModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Convertimos el modelo de C# a formato JSON nativo usando System.Text.Json [101]
        var stringContent = new StringContent(
            JsonSerializer.Serialize(model),
            Encoding.UTF8,
            "application/json"
        );

        // Hacemos la petición POST asíncrona a la API REST [102]
        var response = await _httpClient.PostAsync("trabajadores/login", stringContent);

        if (response.IsSuccessStatusCode)
        {
            var responseString = await response.Content.ReadAsStringAsync();

            // Reconstruimos el JSON recibido en un objeto de tipo Trabajador
            var trabajador = JsonSerializer.Deserialize<TrabajadorViewModel>(responseString, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true // Permite leer las propiedades sin importar mayúsculas/minúsculas
            });

            // Guardamos los datos esenciales en la memoria de sesión del servidor web
            if (trabajador != null)
            {
                HttpContext.Session.SetInt32("TrabajadorId", trabajador.Id);
                HttpContext.Session.SetString("TrabajadorNombre", trabajador.Nombre);
                HttpContext.Session.SetString("TrabajadorPais", trabajador.Pais);
            }

            return RedirectToAction("Index", "Home"); // Redirige a la pantalla principal (Dashboard)
        }

        // Si la API responde con un error (ej. credenciales inválidas), mostramos un mensaje
        ViewBag.Error = "Correo electrónico o contraseña incorrectos.";
        return View(model);
    }

    // 3. GET: Acceso/Logout (Cierra la sesión del trabajador)
    [HttpGet]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear(); // Borra todas las variables de sesión en memoria
        return RedirectToAction("Login");
    }

    // 4. GET: Acceso/Registro (Muestra la pantalla de registro de nuevo trabajador)
    [HttpGet]
    public IActionResult Registro()
    {
        return View();
    }

    // 5. POST: Acceso/Registro (Manda los datos de registro a la API REST)
    [HttpPost]
    public async Task<IActionResult> Registro(TrabajadorViewModel model, string contrasena)
    {
        if (!ModelState.IsValid || string.IsNullOrEmpty(contrasena))
        {
            ViewBag.Error = "Todos los campos son obligatorios.";
            return View(model);
        }

        // Creamos un objeto anónimo temporal que incluya la contraseña para enviarlo a la API
        var nuevoTrabajador = new
        {
            Nombre = model.Nombre,
            Correo = model.Correo,
            Contrasena = contrasena,
            Pais = model.Pais
        };

        var stringContent = new StringContent(JsonSerializer.Serialize(nuevoTrabajador), Encoding.UTF8, "application/json");

        // Consumimos el nuevo endpoint de registro de la API REST [101]
        var response = await _httpClient.PostAsync("trabajadores", stringContent);

        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction("Login"); // Redirige al login tras registrarse con éxito
        }

        var errorMsg = await response.Content.ReadAsStringAsync();
        ViewBag.Error = errorMsg.Contains("ya está registrado") ? "El correo electrónico ya está registrado." : "Error al registrarse en el servidor.";
        return View(model);
    }
}