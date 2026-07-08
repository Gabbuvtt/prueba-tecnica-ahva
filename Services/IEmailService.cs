namespace PruebaTecnicaAhva.Services;

public interface IEmailService
{
    Task SendAccountLockedEmailAsync(string toEmail);
}
