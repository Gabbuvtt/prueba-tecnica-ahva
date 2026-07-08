namespace PruebaTecnicaAhva.Models.Entities;

public class Usuario
{
    public int Id { get; set; }
    public string NumeroDocumento { get; set; } = string.Empty;
    public string TipoDocumento { get; set; } = string.Empty;
    public string Contrasena { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string PrimerApellido { get; set; } = string.Empty;
    public string? SegundoApellido { get; set; }
    public DateOnly FechaNacimiento { get; set; }
    public string Nacionalidad { get; set; } = string.Empty;
    public string Sexo { get; set; } = string.Empty;
    public string CorreoPrincipal { get; set; } = string.Empty;
    public string? CorreoSecundario { get; set; }
    public string TelefonoMovil { get; set; } = string.Empty;
    public string? TelefonoSecundario { get; set; }
    public string TipoContratacion { get; set; } = string.Empty;
    public DateOnly FechaContratacion { get; set; }
    public string Estado { get; set; } = "Activo";
    public int IntentosFallidos { get; set; } = 0;
    public DateTime? BloqueadoHasta { get; set; }
    public string Rol { get; set; } = "Empleado";
}
