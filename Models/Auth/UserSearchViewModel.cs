using System.ComponentModel.DataAnnotations;

namespace PruebaTecnicaAhva.Models.Auth;

public class UserSearchViewModel
{
    [Required(ErrorMessage = "El numero de documento es obligatorio.")]
    [RegularExpression("^[A-Za-z0-9]{8,20}$", ErrorMessage = "Ingresa un DNI o CE valido.")]
    [Display(Name = "Numero de DNI o CE")]
    public string DocumentNumber { get; set; } = string.Empty;
}
