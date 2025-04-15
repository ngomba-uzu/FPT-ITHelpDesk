using ITHelpDesk.Models;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace ITHelpDesk.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly MailKitEmailSender _mailKitSender;

        public EmailSender(MailKitEmailSender mailKitSender)
        {
            _mailKitSender = mailKitSender;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return _mailKitSender.SendEmailAsync(email, subject, htmlMessage);
        }
    }
}
