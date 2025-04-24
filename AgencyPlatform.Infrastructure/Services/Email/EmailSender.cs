using AgencyPlatform.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace AgencyPlatform.Infrastructure.Services.Email
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;

        public EmailSender(IConfiguration config)
        {
            _config = config;
        }
        public async Task SendEmailAsync(string toEmail, string subject, string bodyHtml)
        {
            var smtpSection = _config.GetSection("Smtp");

            var host = smtpSection["Host"];
            var portString = smtpSection["Port"];
            var username = smtpSection["Username"];
            var password = smtpSection["Password"];
            var enableSsl = smtpSection["EnableSsl"];
            var senderName = smtpSection["SenderName"];
            var senderEmail = smtpSection["SenderEmail"];

            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(portString) || string.IsNullOrWhiteSpace(username))
                throw new Exception("Configuración SMTP incompleta en appsettings.json");

            var smtpClient = new SmtpClient(host, int.Parse(portString))
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = bool.Parse(enableSsl ?? "true")
            };

            var message = new MailMessage
            {
                From = new MailAddress(senderEmail!, senderName),
                Subject = subject,
                Body = bodyHtml,
                IsBodyHtml = true
            };

            message.To.Add(toEmail);

            await smtpClient.SendMailAsync(message);
        }

        public async Task SendEmailWithEmbeddedImageAsync(string toEmail, string subject, string bodyHtml, string imagePath)
        {
            var smtpSection = _config.GetSection("Smtp");

            var host = smtpSection["Host"];
            var portString = smtpSection["Port"];
            var username = smtpSection["Username"];
            var password = smtpSection["Password"];
            var enableSsl = smtpSection["EnableSsl"];
            var senderName = smtpSection["SenderName"];
            var senderEmail = smtpSection["SenderEmail"];

            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(portString) || string.IsNullOrWhiteSpace(username))
                throw new Exception("Configuración SMTP incompleta en appsettings.json");

            var smtpClient = new SmtpClient(host, int.Parse(portString))
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = bool.Parse(enableSsl ?? "true")
            };

            var message = new MailMessage
            {
                From = new MailAddress(senderEmail!, senderName),
                Subject = subject,
                Body = bodyHtml,
                IsBodyHtml = true
            };

            message.To.Add(toEmail);

            // Insertar imagen como archivo adjunto
            if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
            {
                var inlineAttachment = new Attachment(imagePath)
                {
                    ContentDisposition = { Inline = true, FileName = Path.GetFileName(imagePath) },
                    ContentType = new System.Net.Mime.ContentType("image/png")
                };
                message.Attachments.Add(inlineAttachment);

                // Referenciar la imagen en el cuerpo del correo
                bodyHtml = bodyHtml.Replace("cid:image001", "cid:" + inlineAttachment.ContentId);
            }

            await smtpClient.SendMailAsync(message);
        }

    }
}
