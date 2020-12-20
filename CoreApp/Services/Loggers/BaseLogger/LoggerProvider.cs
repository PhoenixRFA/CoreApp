using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace CoreApp.Services.Loggers.BaseLogger
{
    /// <summary>
    /// База для реализации провайдера логов
    /// Провайдер логов представляет посредника который отображает или сохраняет информацию
    /// Данный класс может использоваться как основа для провайдера файлового или БД логгера
    /// </summary>
    public abstract class LoggerProvider : ILoggerProvider, ISupportExternalScope
    {
        #region Dispose

        /// <summary> Флаг того, что ресурсы уже освобождены </summary>
        public bool IsDisposed { get; private set; }
        
        /// <summary>
        /// НЕ обязательный финализатор
        /// Не обязательный потому что метод Dispose вызовется через GC
        /// </summary>
        ~LoggerProvider()
        {
            if (!IsDisposed)
            {
                Dispose(false);
            }
        }

        public void Dispose()
        {
            //Метод Dispose всегда реализуется следующим образом:
            //вначале вызывается метод Dispose(true), а затем может следовать вызов метода GC.SuppressFinalize
            if (IsDisposed) return;
            
            try
            {
                Dispose(true);
            }
            catch { /* ignored */ }

            IsDisposed = true;
                
            //предотвращает вызов финализатора, т.к. в нем нет надобности (Dispose(true) выполнил всю работу)
            GC.SuppressFinalize(this);
        }

        /// <summary> Метод, который делает всю работу по освобождению ресурсов </summary>
        /// <param name="disposing">говорит о том, вызывается ли этот метод из метода Dispose или из финализатора</param>
        protected virtual void Dispose(bool disposing)
        {
            if(IsDisposed) return;
            
            if (disposing)
            {
                //Освобождение только управляемых ресурсов
                if (SettingsChangeToken != null)
                {
                    SettingsChangeToken.Dispose();
                    SettingsChangeToken = null;
                }
            }

            //Освобождение НЕ управляемых ресурсов
        }

        #endregion

        #region SupportExternalScope
        
        private IExternalScopeProvider _scopeProvider;

        /// <summary> Возвращает заданный провайдер области или провайдер по-умолчанию, если он не был задан </summary>
        internal IExternalScopeProvider ScopeProvider => _scopeProvider ??= new LoggerExternalScopeProvider();

        /// <summary> Устанавливает провайдер внешней области </summary>
        public void SetScopeProvider(IExternalScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }

        #endregion

        /// <summary> Хранилище логгеров </summary>
        private readonly ConcurrentDictionary<string, Logger> _loggers = new ConcurrentDictionary<string, Logger>();
        
        /// <summary> Токен IOptionsMonitor.OnChange Что бы "отписаться" от событий изменения его нужно утилизировать (Dispose) </summary>
        protected IDisposable SettingsChangeToken;

        /// <summary> Создание логгера для определенной категории </summary>
        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, name => new Logger(this, categoryName));
        }

        /// <summary> Доступен ли провайдер? </summary>
        public abstract bool IsEnabled(LogLevel logLevel);
        
        /// <summary> Метод записи/вывода лога </summary>
        public abstract void WriteLog(LogEntry logEntry);
    }
}
