namespace PruebaTecnicaAhva.Models.Entities;

public class Notificacion
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public bool Leida { get; set; } = false;

    // Navegación
    public virtual Usuario? Usuario { get; set; }
}
