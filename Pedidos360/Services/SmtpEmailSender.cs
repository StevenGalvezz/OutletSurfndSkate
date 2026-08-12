using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Pedidos360.Services
{
    // Implementación real de IEmailSender (la interfaz que ya usa Identity
    // para los correos de cuenta) usando SMTP con System.Net.Mail, que ya
    // viene con .NET — no hace falta agregar ningún paquete nuevo.
    //
    // La configuración vive en dos lugares:
    //  - appsettings.json: Smtp:Host, Smtp:Port, Smtp:From, Smtp:FromName
    //    (no son secretos, se pueden versionar).
    //  - user-secrets (o variables de entorno en producción): Smtp:User y
    //    Smtp:Password, para no dejar credenciales en el repo.
    public class SmtpEmailSender : IEmailSender
    {
        private readonly IConfiguration _config;
        private readonly ILogger<SmtpEmailSender> _logger;

        public SmtpEmailSender(IConfiguration config, ILogger<SmtpEmailSender> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var host = _config["Smtp:Host"];
            var user = _config["Smtp:User"];
            var password = _config["Smtp:Password"];
            var from = _config["Smtp:From"] ?? user;

            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(from))
            {
                _logger.LogWarning("Smtp no está configurado (Smtp:Host/User/Password/From); no se envió el correo a {Email} con asunto \"{Subject}\".", email, subject);
                return;
            }

            var port = int.TryParse(_config["Smtp:Port"], out var p) ? p : 587;
            var fromName = _config["Smtp:FromName"] ?? "Outlet Surf&Skate";

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(user, password),
                EnableSsl = true
            };

            using var mensaje = new MailMessage
            {
                From = new MailAddress(from, fromName),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };
            mensaje.To.Add(email);

            try
            {
                await client.SendMailAsync(mensaje);
                _logger.LogInformation("Correo \"{Subject}\" enviado a {Email}.", subject, email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "No se pudo enviar el correo \"{Subject}\" a {Email}.", subject, email);
                throw;
            }
        }
    }
}
