using System.Net.Http.Json;
using CoParenting.Application.Common.Interfaces;
using CoParenting.Application.Common.Models;
using Microsoft.Extensions.Options;

namespace CoParenting.Infrastructure.Email;

// Envia emails reais via a API HTTP do Resend (https://resend.com/docs/api-reference/emails/send-email).
// Sem pacote NuGet dedicado: é só um POST simples, mantém a dependência mínima.
public class ResendEmailSender(HttpClient httpClient, IOptions<ResendOptions> options) : IEmailSender
{
    public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var resendOptions = options.Value;

        var payload = new
        {
            from = $"{resendOptions.FromName} <{resendOptions.FromAddress}>",
            to = new[] { toEmail },
            subject,
            html = htmlBody
        };

        var response = await httpClient.PostAsJsonAsync("emails", payload, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
