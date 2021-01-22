using IdentitySandboxApp.Models;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using System.Threading.Tasks;

namespace IdentitySandboxApp.Infrastructure
{
    public class MailKitEmailSender : IEmailSender
    {
        private readonly IOptionsSnapshot<SmtpOptions> _options;

        public MailKitEmailSender(IOptionsSnapshot<SmtpOptions> options)
        {
            _options = options;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            SmtpOptions opts = _options.Value;

            var mail = new MimeMessage
            {
                Subject = subject
            };
            mail.To.Add(MailboxAddress.Parse(email));
            mail.From.Add(new MailboxAddress("IdentitySandboxApp", opts.Login));
            mail.Body = new TextPart(TextFormat.Html)
            {
                Text = htmlMessage
            };

            using var client = new SmtpClient();
            await client.ConnectAsync(opts.Server, opts.Port);
            await client.AuthenticateAsync(opts.Login, opts.Password);
            await client.SendAsync(mail);
            await client.DisconnectAsync(true);
        }
    }
}
