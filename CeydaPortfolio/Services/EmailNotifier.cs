using CeydaPortfolio.Models;
using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace CeydaPortfolio.Services
{
    public class EmailNotifier
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailNotifier> _logger;

        public EmailNotifier(IOptions<EmailSettings> settings, ILogger<EmailNotifier> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task SendContactNotificationAsync(ContactMessage message)
        {
            _logger.LogWarning("Mail bildirimi başladı. Host={Host}, Port={Port}, SifreVar={SifreVar}",
           _settings.Host, _settings.Port, !string.IsNullOrWhiteSpace(_settings.Password));
            if (string.IsNullOrWhiteSpace(_settings.Host) ||
                string.IsNullOrWhiteSpace(_settings.Password) ||
                string.IsNullOrWhiteSpace(_settings.To))
            {
                _logger.LogWarning("E-posta ayarları eksik, bildirim gönderilmedi.");
                return;
            }

            try
            {
                var safeName = message.Name.Replace("\r", " ").Replace("\n", " ");

                var mail = new MimeMessage();
                mail.From.Add(new MailboxAddress("Portföy sitesi", _settings.User));
                mail.To.Add(MailboxAddress.Parse(_settings.To));
                mail.ReplyTo.Add(new MailboxAddress(safeName, message.Email));
                mail.Subject = $"Yeni mesaj: {safeName}";
                mail.Body = new TextPart("plain")
                {
                    Text = $"Ad: {safeName}\nE-posta: {message.Email}\n\nMesaj:\n{message.Message}"
                };

                using var client = new SmtpClient();
                client.Timeout = 10000;
                await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.SslOnConnect);
                await client.AuthenticateAsync(_settings.User, _settings.Password);
                await client.SendAsync(mail);
                await client.DisconnectAsync(true);
                _logger.LogInformation("E-posta bildirimi gönderildi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "E-posta bildirimi gönderilemedi.");
            }
        }
    }                               
}


    