namespace ControlVehiculos.Mvc.Models;

public class TrabajadorViewModel
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Pais { get; set; } = string.Empty;
}

public class LoginRequestModel
{
    public string Correo { get; set; } = string.Empty;
    public string Contrasena { get; set; } = string.Empty;
}