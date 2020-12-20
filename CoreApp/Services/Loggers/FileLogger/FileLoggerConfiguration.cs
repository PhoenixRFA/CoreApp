using System;
using Microsoft.Extensions.Logging;

namespace CoreApp.Services.Loggers.FileLogger
{
    /// <summary> Объект, отвечающий за конфигурацию логера </summary>
    public class FileLoggerConfiguration
    {
        private string _path;

        public string LogsFolderPath
        {
            get => !string.IsNullOrWhiteSpace(_path) ? _path : Environment.CurrentDirectory; //System.IO.Path.GetDirectoryName(GetType().Assembly.Location)

            set => _path = value;
        }

        public int MaxFileSizeKb { get; set; } = 2 * 1024;
        public LogLevel LogLevel { get; set; } = LogLevel.Warning;
        /// <summary> Политика хранения. Сколько файлов логов сохранять</summary>
        public int RetainPolicyFileCount { get; set; } = 5;
    }
}
