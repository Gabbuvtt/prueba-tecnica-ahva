using System.ComponentModel.DataAnnotations;

namespace PruebaTecnicaAhva.Models.Auth;

public class LoginRequestViewModel
{
    [Required(ErrorMessage = "Selecciona el tipo de documento.")]
    [RegularExpression("DNI|CE", ErrorMessage = "Tipo de documento no valido.")]
    public string DocumentType { get; set; } = "DNI";

    [Required(ErrorMessage = "El usuario es obligatorio.")]
    [StringLength(20, MinimumLength = 8, ErrorMessage = "El usuario debe tener entre 8 y 20 caracteres.")]
    [RegularExpression("^[0-9A-Za-z]+$", ErrorMessage = "El usuario solo permite letras y numeros.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contrasena es obligatoria.")]
    [StringLength(50, MinimumLength = 6, ErrorMessage = "La contrasena debe tener al menos 6 caracteres.")]
    public string Password { get; set; } = string.Empty;
}
