using System.Net;
using System.Net.Mail;
using LinkUp.Application.Interfaces;
using LinkUp.Shared.Options;

namespace LinkUp.Shared.Services;


public sealed class SmtpEmailService : IEmailService
{
    private readonly SmtpOptions _options;

    public SmtpEmailService(SmtpOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.Host))
            throw new ArgumentException("Debe configurar Smtp:Host.", nameof(options));

        if (options.Port is < 1 or > 65535)
            throw new ArgumentException("El puerto SMTP debe estar entre 1 y 65535.", nameof(options));

        if (string.IsNullOrWhiteSpace(options.FromAddress))
            throw new ArgumentException("Debe configurar Smtp:FromAddress.", nameof(options));

        _ = new MailAddress(options.FromAddress);

        if (string.IsNullOrWhiteSpace(options.Username) != string.IsNullOrWhiteSpace(options.Password))
            throw new ArgumentException("Configure tanto Smtp:Username como Smtp:Password, o ninguno de ellos.", nameof(options));

        _options = options;
    }

    public async Task SendEmailAsync(
        string recipientEmail,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(recipientEmail);
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);
        ArgumentException.ThrowIfNullOrWhiteSpace(htmlBody);

        using var message = new MailMessage
        {
            From = new MailAddress(_options.FromAddress, _options.FromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };

        message.To.Add(new MailAddress(recipientEmail));

        using var client = new SmtpClient(_options.Host, _options.Port)
        {
            DeliveryMethod = SmtpDeliveryMethod.Network,
            EnableSsl = _options.EnableSsl,
            UseDefaultCredentials = false
        };

        if (!string.IsNullOrWhiteSpace(_options.Username))
        {
            client.Credentials = new NetworkCredential(_options.Username, _options.Password);
        }

        await client.SendMailAsync(message, cancellationToken);
    }
}
