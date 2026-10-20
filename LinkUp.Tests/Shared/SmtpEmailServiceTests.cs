using LinkUp.Shared.Options;
using LinkUp.Shared.Services;

namespace LinkUp.Tests.Shared;

public sealed class SmtpEmailServiceTests
{
    private static SmtpOptions ValidOptions() => new()
    {
        Host = "smtp.example.test",
        Port = 587,
        EnableSsl = true,
        FromAddress = "no-reply@example.test",
        FromName = "LinkUp Pro",
        Username = "mailer",
        Password = "secret-for-test-only"
    };

    [Fact]
    public void Constructor_AcceptsValidConfiguration()
    {
        var service = new SmtpEmailService(ValidOptions());
        Assert.NotNull(service);
    }

    [Fact]
    public void Constructor_RejectsNullConfiguration()
    {
        Assert.Throws<ArgumentNullException>(() => new SmtpEmailService(null!));
    }

    [Fact]
    public void Constructor_RejectsMissingHost()
    {
        var options = ValidOptions();
        options.Host = " ";
        Assert.Throws<ArgumentException>(() => new SmtpEmailService(options));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(65536)]
    public void Constructor_RejectsInvalidPort(int port)
    {
        var options = ValidOptions();
        options.Port = port;
        Assert.Throws<ArgumentException>(() => new SmtpEmailService(options));
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid address")]
    public void Constructor_RejectsInvalidSender(string sender)
    {
        var options = ValidOptions();
        options.FromAddress = sender;
        Assert.ThrowsAny<Exception>(() => new SmtpEmailService(options));
    }

    [Fact]
    public void Constructor_RejectsPartialCredentials()
    {
        var options = ValidOptions();
        options.Password = "";
        Assert.Throws<ArgumentException>(() => new SmtpEmailService(options));
    }

    [Fact]
    public void Constructor_AllowsAnonymousSmtp()
    {
        var options = ValidOptions();
        options.Username = "";
        options.Password = "";
        Assert.NotNull(new SmtpEmailService(options));
    }

    [Theory]
    [InlineData("", "Asunto", "<p>Mensaje</p>")]
    [InlineData("destinatario@example.test", "", "<p>Mensaje</p>")]
    [InlineData("destinatario@example.test", "Asunto", "")]
    public async Task SendEmailAsync_RejectsEmptyRequiredFields(
        string recipient, string subject, string body)
    {
        var service = new SmtpEmailService(ValidOptions());
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.SendEmailAsync(recipient, subject, body));
    }

    [Fact]
    public async Task SendEmailAsync_RejectsInvalidRecipientWithoutConnecting()
    {
        var service = new SmtpEmailService(ValidOptions());
        await Assert.ThrowsAsync<FormatException>(
            () => service.SendEmailAsync("invalid address", "Asunto", "<p>Mensaje</p>"));
    }
}
