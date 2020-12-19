using System;
using Microsoft.Extensions.Logging;

namespace CoreApp.Services.Loggers.ColorConsole
{
    public class ColorConsoleLogger : ILogger
    {
        private readonly string _name;
        private readonly ColorConsoleLoggerConfiguration _config;

        public ColorConsoleLogger(string name, ColorConsoleLoggerConfiguration config = null)
        {
            _name = name;
            _config = config ?? new ColorConsoleLoggerConfiguration();
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= _config.LogLevel;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if(!IsEnabled(logLevel)) return;

            ConsoleColor color = Console.ForegroundColor;
            Console.ForegroundColor = _config.Color;
            Console.WriteLine($"{logLevel} [${eventId}] '${_name}' ${formatter(state, exception)}");
            Console.ForegroundColor = color;
        }
    }
}
