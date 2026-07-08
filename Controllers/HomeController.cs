using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using PruebaTecnicaAhva.Data;
using PruebaTecnicaAhva.Models;
using PruebaTecnicaAhva.Models.Auth;
using PruebaTecnicaAhva.Models.Dashboard;
using PruebaTecnicaAhva.Models.Entities;
using PruebaTecnicaAhva.Models.Users;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace PruebaTecnicaAhva.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AppDbContext _dbContext;
    private readonly PruebaTecnicaAhva.Services.IEmailService _emailService;

    public HomeController(ILogger<HomeController> logger, AppDbContext dbContext, PruebaTecnicaAhva.Services.IEmailService emailService)
    {
        _logger = logger;
        _dbContext = dbContext;
        _emailService = emailService;
    }

    public IActionResult Index()
    {
        return RedirectToAction(nameof(ActivacionCuenta));
    }

    public IActionResult ActivacionCuenta()
    {
        var model = new UserSearchViewModel();
        return View(model);
    }

    public IActionResult ResultadoNoEncontrado(bool exists = false)
    {
        ViewBag.Exists = exists;
        return View();
    }

    public IActionResult ResultadoActivado()
    {
        return RedirectToAction(nameof(ActivacionCuenta));
    }

    public IActionResult CuentaBloqueada()
    {
        return View();
    }
    public IActionResult IniciarSesion(bool timeout = false)
    {
        var model = new LoginRequestViewModel();
        ViewBag.Timeout = timeout;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> IniciarSesion(LoginRequestViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _dbContext.Usuarios.FirstOrDefaultAsync(u =>
            u.NumeroDocumento == model.Username &&
            u.TipoDocumento == model.DocumentType);

        if (user is null)
        {
            ModelState.AddModelError("Username", "Usuario incorrecto");
            return View(model);
        }

        if (user.Estado.Equals("Bloqueado", StringComparison.OrdinalIgnoreCase) || 
            (user.BloqueadoHasta.HasValue && user.BloqueadoHasta.Value > DateTime.Now))
        {
            return RedirectToAction(nameof(CuentaBloqueada));
        }

        if (user.Estado.Equals("Inactivo", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError("Username", "Usuario inactivo. No está verificado aún.");
            return View(model);
        }

        if (user.BloqueadoHasta.HasValue && user.BloqueadoHasta.Value <= DateTime.Now)
        {
            user.BloqueadoHasta = null;
            user.IntentosFallidos = 0;
        }

        if (user.Contrasena != model.Password)
        {
            user.IntentosFallidos++;
            if (user.IntentosFallidos >= 5)
            {
                user.BloqueadoHasta = DateTime.Now.AddMinutes(15);
                user.IntentosFallidos = 0;
                await _dbContext.SaveChangesAsync();
                await _emailService.SendAccountLockedEmailAsync(user.CorreoPrincipal ?? "correo@desconocido.com");
                return RedirectToAction(nameof(CuentaBloqueada));
            }
            await _dbContext.SaveChangesAsync();
            ModelState.AddModelError("Password", "Contraseña incorrecta");
            return View(model);
        }

        user.IntentosFallidos = 0;
        user.BloqueadoHasta = null;
        await _dbContext.SaveChangesAsync();

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, $"{user.Nombres} {user.PrimerApellido} {user.SegundoApellido}".Trim()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Rol)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

        return RedirectToAction(nameof(Dashboard));
    }

    public async Task<IActionResult> Logout(bool timeout = false)
    {
        await HttpContext.SignOutAsync(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);
        if (timeout)
            return RedirectToAction(nameof(IniciarSesion), new { timeout = true });
        return RedirectToAction(nameof(ActivacionCuenta));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ActivacionCuenta(UserSearchViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _dbContext.Usuarios
            .FirstOrDefaultAsync(u => u.NumeroDocumento == model.DocumentNumber);

        if (user is null)
        {
            return RedirectToAction(nameof(ResultadoNoEncontrado), new { exists = false });
        }

        if (user.Estado.Equals("Inactivo", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToAction(nameof(ResultadoNoEncontrado), new { exists = true });
        }

        if (user.Estado.Equals("Bloqueado", StringComparison.OrdinalIgnoreCase) || 
            (user.BloqueadoHasta.HasValue && user.BloqueadoHasta.Value > DateTime.Now))
        {
            return RedirectToAction(nameof(CuentaBloqueada));
        }

        if (user.BloqueadoHasta.HasValue && user.BloqueadoHasta.Value <= DateTime.Now)
        {
            user.BloqueadoHasta = null;
            user.IntentosFallidos = 0;
            await _dbContext.SaveChangesAsync();
            return RedirectToAction(nameof(IniciarSesion));
        }

        return RedirectToAction(nameof(ResultadoActivadoUsuario), new { id = user.Id });
    }

    public async Task<IActionResult> ResultadoActivadoUsuario(int id)
    {
        var user = await _dbContext.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user is null)
        {
            return RedirectToAction(nameof(ResultadoNoEncontrado));
        }

        var greeting = user.Sexo.Equals("Masculino", StringComparison.OrdinalIgnoreCase)
            ? "Bienvenido"
            : "Bienvenida";

        var model = new ActivationResultViewModel
        {
            Greeting = greeting,
            Nombres = user.Nombres
        };

        return View("ResultadoActivado", model);
    }

    public async Task<IActionResult> Dashboard()
    {
        var model = await BuildDashboardViewModel();
        return View(model);
    }

    public async Task<IActionResult> GestionUsuarios()
    {
        var model = await BuildDashboardViewModel();
        return View(model);
    }

    public async Task<IActionResult> Buscar(string? criterio, string? termino)
    {
        ViewBag.Criterio = string.IsNullOrWhiteSpace(criterio) ? "dni" : criterio.ToLowerInvariant();
        ViewBag.Termino = termino ?? string.Empty;

        IQueryable<Usuario> query = _dbContext.Usuarios.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(termino))
        {
            var term = termino.Trim();
            query = ViewBag.Criterio switch
            {
                "ce" => query.Where(u => u.TipoDocumento == "CE" && u.NumeroDocumento.Contains(term)),
                _ => query.Where(u => u.TipoDocumento == "DNI" && u.NumeroDocumento.Contains(term))
            };
        }

        var users = await query.OrderBy(u => u.PrimerApellido).ThenBy(u => u.Nombres).ToListAsync();
        var baseModel = await BuildDashboardViewModel();
        var model = new DashboardViewModel
        {
            Indicators = baseModel.Indicators,
            Users = users.Select(MapUser).ToList()
        };
        return View(model);
    }
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    public async Task<IActionResult> PerfilUsuario(int? id)
    {
        var userIdString = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        int currentUserId = 0;
        int.TryParse(userIdString, out currentUserId);

        int targetId = id ?? currentUserId;
        
        if (!User.IsInRole("Operador") && targetId != currentUserId)
        {
            return Forbid();
        }

        var user = await _dbContext.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == targetId);

        if (user is null)
        {
            return RedirectToAction(nameof(ResultadoNoEncontrado));
        }

        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PerfilUsuario(Usuario model)
    {
        var userIdString = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        int currentUserId = 0;
        int.TryParse(userIdString, out currentUserId);

        if (model.Id != currentUserId)
        {
            return Forbid();
        }

        var userToUpdate = await _dbContext.Usuarios.FirstOrDefaultAsync(u => u.Id == currentUserId);
        if (userToUpdate != null)
        {
            userToUpdate.CorreoPrincipal = model.CorreoPrincipal;
            userToUpdate.CorreoSecundario = model.CorreoSecundario;
            userToUpdate.TelefonoMovil = model.TelefonoMovil;
            userToUpdate.TelefonoSecundario = model.TelefonoSecundario;
            
            await _dbContext.SaveChangesAsync();
            ViewBag.Mensaje = "Perfil actualizado con éxito.";
            return View(userToUpdate);
        }

        return View(model);
    }

    private async Task<DashboardViewModel> BuildDashboardViewModel()
    {
        var usuarios = await _dbContext.Usuarios.AsNoTracking().ToListAsync();
        var activos = usuarios.Count(u => u.Estado.Equals("Activo", StringComparison.OrdinalIgnoreCase) && !(u.BloqueadoHasta.HasValue && u.BloqueadoHasta.Value > DateTime.Now));
        var bloqueados = usuarios.Count(u => u.Estado.Equals("Bloqueado", StringComparison.OrdinalIgnoreCase) || (u.BloqueadoHasta.HasValue && u.BloqueadoHasta.Value > DateTime.Now));
        var pendientes = usuarios.Count(u => u.Estado.Equals("Pendiente", StringComparison.OrdinalIgnoreCase));

        var userIdString = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        int currentUserId = 0;
        int.TryParse(userIdString, out currentUserId);

        var notificaciones = await _dbContext.Notificaciones.AsNoTracking()
            .Where(n => n.UsuarioId == currentUserId)
            .OrderByDescending(n => n.FechaCreacion)
            .ToListAsync();

        return new DashboardViewModel
        {
            Indicators =
            [
                new DashboardIndicatorViewModel { Title = "Usuarios Activos", Value = activos.ToString(), Trend = "Estado actual", IsPositiveTrend = true },
                new DashboardIndicatorViewModel { Title = "Cuentas Bloqueadas", Value = bloqueados.ToString(), Trend = "Control acceso", IsPositiveTrend = bloqueados == 0 },
                new DashboardIndicatorViewModel { Title = "Validaciones Totales", Value = usuarios.Count.ToString(), Trend = "Registros BD", IsPositiveTrend = true },
                new DashboardIndicatorViewModel { Title = "Solicitudes Pendientes", Value = pendientes.ToString(), Trend = "Seguimiento", IsPositiveTrend = pendientes == 0 }
            ],
            Users = usuarios.Select(MapUser).ToList(),
            Notificaciones = notificaciones
        };
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnviarNotificacion(int usuarioId, string mensaje)
    {
        if (User.IsInRole("Operador") && !string.IsNullOrWhiteSpace(mensaje))
        {
            var notificacion = new Notificacion 
            { 
                UsuarioId = usuarioId, 
                Mensaje = mensaje 
            };
            _dbContext.Notificaciones.Add(notificacion);
            await _dbContext.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Dashboard));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(int id)
    {
        if (!User.IsInRole("Operador")) return Forbid();
        var user = await _dbContext.Usuarios.FindAsync(id);
        if (user != null)
        {
            user.Estado = user.Estado == "Activo" ? "Inactivo" : "Activo";
            await _dbContext.SaveChangesAsync();
        }
        return RedirectToAction(nameof(GestionUsuarios));
    }

    public IActionResult NuevoUsuario()
    {
        if (!User.IsInRole("Operador")) return Forbid();
        var model = new Usuario();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> NuevoUsuario(Usuario model)
    {
        if (!User.IsInRole("Operador")) return Forbid();
        
        if (await _dbContext.Usuarios.AnyAsync(u => u.NumeroDocumento == model.NumeroDocumento))
        {
            ModelState.AddModelError("NumeroDocumento", "Este número de documento ya está registrado en el sistema.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }
        
        model.Rol = "Empleado";
        model.Estado = "Activo";
        model.IntentosFallidos = 0;
        
        _dbContext.Usuarios.Add(model);
        await _dbContext.SaveChangesAsync();
        
        return RedirectToAction(nameof(GestionUsuarios));
    }

    [HttpGet]
    public async Task<IActionResult> GetUsuario(int id)
    {
        var user = await _dbContext.Usuarios.FindAsync(id);
        if (user == null) return NotFound();
        return Json(user);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateUsuarioAdmin([FromBody] Usuario model)
    {
        if (!User.IsInRole("Operador")) return Forbid();
        var user = await _dbContext.Usuarios.FindAsync(model.Id);
        if (user == null) return NotFound();

        user.Nombres = model.Nombres;
        user.PrimerApellido = model.PrimerApellido;
        user.SegundoApellido = model.SegundoApellido;
        user.TipoDocumento = model.TipoDocumento;
        user.NumeroDocumento = model.NumeroDocumento;
        user.FechaNacimiento = model.FechaNacimiento;
        user.Nacionalidad = model.Nacionalidad;
        user.Sexo = model.Sexo;
        user.CorreoPrincipal = model.CorreoPrincipal;
        user.CorreoSecundario = model.CorreoSecundario;
        user.TelefonoMovil = model.TelefonoMovil;
        user.TelefonoSecundario = model.TelefonoSecundario;
        user.TipoContratacion = model.TipoContratacion;
        user.FechaContratacion = model.FechaContratacion;
        
        await _dbContext.SaveChangesAsync();
        return Json(new { success = true });
    }

    [HttpPost]
    public async Task<IActionResult> MarcarNotificacionesLeidas()
    {
        var userIdString = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdString, out int currentUserId))
        {
            var unread = await _dbContext.Notificaciones.Where(n => n.UsuarioId == currentUserId && !n.Leida).ToListAsync();
            foreach (var n in unread) n.Leida = true;
            await _dbContext.SaveChangesAsync();
        }
        return Ok();
    }

    private static UserListItemViewModel MapUser(Usuario user)
    {
        var fullName = $"{user.PrimerApellido} {user.SegundoApellido}, {user.Nombres}".Replace(" ,", ",");
        
        string status = user.Estado;
        if (user.BloqueadoHasta.HasValue && user.BloqueadoHasta.Value > DateTime.Now)
        {
            status = "Bloqueado Temporalmente";
        }

        return new UserListItemViewModel
        {
            Id = user.Id,
            FullName = fullName,
            DocumentNumber = $"{user.TipoDocumento} {user.NumeroDocumento}",
            Role = user.Rol,
            Status = status,
            Email = user.CorreoPrincipal
        };
    }
}



