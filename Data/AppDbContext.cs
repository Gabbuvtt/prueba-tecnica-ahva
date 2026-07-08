using Microsoft.EntityFrameworkCore;
using PruebaTecnicaAhva.Models.Entities;

namespace PruebaTecnicaAhva.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Notificacion> Notificaciones => Set<Notificacion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var usuario = modelBuilder.Entity<Usuario>();
        usuario.ToTable("Usuarios");
        usuario.HasKey(u => u.Id);
        
        var notificacion = modelBuilder.Entity<Notificacion>();
        notificacion.ToTable("Notificaciones");
        notificacion.HasKey(n => n.Id);
        notificacion.HasOne(n => n.Usuario)
            .WithMany()
            .HasForeignKey(n => n.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        usuario.Property(u => u.Id);
        usuario.Property(u => u.NumeroDocumento).HasMaxLength(20).IsRequired();
        usuario.HasIndex(u => u.NumeroDocumento).IsUnique();
        usuario.Property(u => u.TipoDocumento).HasMaxLength(10).IsRequired();
        usuario.Property(u => u.Contrasena).HasMaxLength(255).IsRequired();
        usuario.Property(u => u.Nombres).HasMaxLength(100).IsRequired();
        usuario.Property(u => u.PrimerApellido).HasMaxLength(100).IsRequired();
        usuario.Property(u => u.SegundoApellido).HasMaxLength(100);
        usuario.Property(u => u.FechaNacimiento).HasColumnType("date").IsRequired();
        usuario.Property(u => u.Nacionalidad).HasMaxLength(50).IsRequired();
        usuario.Property(u => u.Sexo).HasMaxLength(20).IsRequired();
        usuario.Property(u => u.CorreoPrincipal).HasMaxLength(150).IsRequired();
        usuario.Property(u => u.CorreoSecundario).HasMaxLength(150);
        usuario.Property(u => u.TelefonoMovil).HasMaxLength(20).IsRequired();
        usuario.Property(u => u.TelefonoSecundario).HasMaxLength(20);
        usuario.Property(u => u.TipoContratacion).HasMaxLength(50).IsRequired();
        usuario.Property(u => u.FechaContratacion).HasColumnType("date").IsRequired();
        usuario.Property(u => u.Estado).HasMaxLength(20).HasDefaultValue("Activo").IsRequired();

        usuario.HasData(
            new Usuario
            {
                Id = 1,
                NumeroDocumento = "32740977",
                TipoDocumento = "DNI",
                Contrasena = "123456",
                Nombres = "Gabriel",
                PrimerApellido = "Perez",
                SegundoApellido = "Lopez",
                FechaNacimiento = new DateOnly(1995, 5, 12),
                Nacionalidad = "Peruana",
                Sexo = "Masculino",
                CorreoPrincipal = "gabriel.perez@example.com",
                CorreoSecundario = null,
                TelefonoMovil = "+51 999 111 222",
                TelefonoSecundario = null,
                TipoContratacion = "CAS",
                FechaContratacion = new DateOnly(2020, 1, 10),
                Estado = "Activo",
                Rol = "Operador"
            },
            new Usuario
            {
                Id = 2,
                NumeroDocumento = "46844596",
                TipoDocumento = "DNI",
                Contrasena = "123456",
                Nombres = "July",
                PrimerApellido = "Mendoza",
                SegundoApellido = "Quispe",
                FechaNacimiento = new DateOnly(1994, 4, 15),
                Nacionalidad = "Peruana",
                Sexo = "Femenino",
                CorreoPrincipal = "july.mendoza@example.com",
                CorreoSecundario = null,
                TelefonoMovil = "+51 999 333 444",
                TelefonoSecundario = null,
                TipoContratacion = "CAS",
                FechaContratacion = new DateOnly(2019, 3, 9),
                Estado = "Activo",
                Rol = "Empleado"
            },
            new Usuario
            {
                Id = 3,
                NumeroDocumento = "99999999",
                TipoDocumento = "DNI",
                Contrasena = "123456",
                Nombres = "Inactivo",
                PrimerApellido = "Test",
                SegundoApellido = "User",
                FechaNacimiento = new DateOnly(1990, 1, 1),
                Nacionalidad = "Peruana",
                Sexo = "Masculino",
                CorreoPrincipal = "inactivo@example.com",
                CorreoSecundario = null,
                TelefonoMovil = "+51 000 000 000",
                TelefonoSecundario = null,
                TipoContratacion = "CAS",
                FechaContratacion = new DateOnly(2023, 1, 1),
                Estado = "Inactivo",
                Rol = "Empleado"
            });
    }
}
