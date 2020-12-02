using Microsoft.Extensions.DependencyInjection;

namespace CoreApp.Services
{
    public class EmailMessageSender : IMessageSender
    {
        public string Send()
        {
            return "Sent by Email";
        }
    }

    public static class EmailMessageSenderExtension
    {
        public static void AddEmailMessageSender(this IServiceCollection services)
        {
            services.AddTransient<IMessageSender, EmailMessageSender>();
        }
    }
}
