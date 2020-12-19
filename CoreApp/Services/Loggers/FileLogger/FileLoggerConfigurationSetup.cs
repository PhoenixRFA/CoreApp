using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace CoreApp.Services.Loggers.FileLogger
{
    /// <summary> Позволяет управлять настройкой FileLoggerConfiguration через appsettings.json </summary>
    public class FileLoggerConfigurationSetup : ConfigureFromConfigurationOptions<FileLoggerConfiguration>
    {
        public FileLoggerConfigurationSetup(IConfiguration config) : base(config) { }
    }
}
