using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace IdentitySandboxApp.Infrastructure
{
    public class DebugEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            Debug.WriteLine($"New email to: {email}");
            Debug.WriteLine($"Subject: {subject}");
            Debug.WriteLine("Body:");
            Debug.WriteLine($"{htmlMessage}");
            
            return Task.CompletedTask;
        }
    }
}
