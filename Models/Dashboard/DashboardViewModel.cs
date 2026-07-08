using PruebaTecnicaAhva.Models.Users;

namespace PruebaTecnicaAhva.Models.Dashboard;

public class DashboardViewModel
{
    public IReadOnlyCollection<DashboardIndicatorViewModel> Indicators { get; set; } = [];
    public IReadOnlyCollection<UserListItemViewModel> Users { get; set; } = [];
    public IReadOnlyCollection<PruebaTecnicaAhva.Models.Entities.Notificacion> Notificaciones { get; set; } = [];
}
