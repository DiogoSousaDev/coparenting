using CoParenting.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace CoParenting.Infrastructure.Email;

// Implementação de desenvolvimento: loga o email em vez de o enviar.
// Trocar por um provedor real (ex. SendGrid) é uma alteração isolada a este ficheiro
// + uma linha de DI em Program.cs.
public class ConsoleEmailSender(ILogger<ConsoleEmailSender> logger) : IEmailSender
{
    public Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "[DEV EMAIL] Para: {ToEmail} | Assunto: {Subject}\n{Body}",
            toEmail, subject, htmlBody);

        return Task.CompletedTask;
    }
}
