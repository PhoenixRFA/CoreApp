using System;
using System.IO;
using Microsoft.Extensions.Logging;

namespace CoreApp.Services.Loggers.FileLogger
{
    public class FileLogger : ILogger
    {
        private readonly string _path;
        private static readonly object _locker = new object();  

        public FileLogger(string path)
        {
            _path = path;

            string dir = Path.GetDirectoryName(_path);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        //??возвращает объект IDisposable, который представляет некоторую область видимости для логгера
        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        //указыват, доступен ли логгер для использования
        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        /* этот метод предназначен для выполнения логгирования. Он принимает пять параметров:
         *     LogLevel: уровень детализации текущего сообщения
         *     TState: некоторый объект состояния, который хранит сообщение
         *     Exception: информация об исключении
         *     formatter: функция форматирвания, которая с помощью двух предыдущих параметов позволяет получить собственно сообщение для логгирования
         */
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if(formatter == null || string.IsNullOrEmpty(_path)) return;

            lock (_locker)
            {
                File.AppendAllText(_path, formatter(state, exception) + Environment.NewLine);
            }
        }
    }
}