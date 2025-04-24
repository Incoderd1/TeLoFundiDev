using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyPlatform.Application.Interfaces.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string toEmail, string subject, string bodyHtml);

        Task SendEmailWithEmbeddedImageAsync(string toEmail, string subject, string bodyHtml, string imagePath);

    }
}
