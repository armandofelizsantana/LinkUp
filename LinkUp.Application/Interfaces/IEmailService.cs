namespace LinkUp.Application.Interfaces;

/// <summary>
/// Contrato para enviar correos transaccionales (activación y recuperación de cuenta).
/// </summary>
public interface IEmailService
{
    Task SendEmailAsync(
        string recipientEmail,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default);
}
