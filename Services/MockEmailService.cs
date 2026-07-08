using Microsoft.Extensions.Logging;

namespace PruebaTecnicaAhva.Services;

public class MockEmailService : IEmailService
{
    private readonly ILogger<MockEmailService> _logger;

    public MockEmailService(ILogger<MockEmailService> logger)
    {
        _logger = logger;
    }

    public Task SendAccountLockedEmailAsync(string toEmail)
    {
        _logger.LogWarning("==================================================");
        _logger.LogWarning("MOCK EMAIL SENT");
        _logger.LogWarning("To: {Email}", toEmail);
        _logger.LogWarning("Subject: Cuenta bloqueada temporalmente");
        _logger.LogWarning("Body: Su cuenta ha sido bloqueada debido a múltiples intentos fallidos de inicio de sesión.");
        _logger.LogWarning("==================================================");
        return Task.CompletedTask;
    }
}
