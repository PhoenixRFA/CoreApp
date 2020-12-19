using System;
using Microsoft.Extensions.Logging;

namespace CoreApp.Services.Loggers.ColorConsole
{
    public class ColorConsoleLoggerConfiguration
    {
        public LogLevel LogLevel { get; set; } = LogLevel.Warning;
        //public int EventId { get; set; } = 0;
        public ConsoleColor Color { get; set; } = ConsoleColor.Green;
    }
}
