using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace CoreApp.Services.Loggers.BaseLogger
{
    public class Logger : ILogger
    {
        public LoggerProvider Provider { get; }
        public string CategoryName { get; }

        public Logger(LoggerProvider provider, string categoryName)
        {
            Provider = provider;
            CategoryName = categoryName;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return Provider.ScopeProvider.Push(state);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return Provider.IsEnabled(logLevel);
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            //формируем элемент лога
            var logItem = new LogEntry
            {
                Category = CategoryName,
                Level = logLevel,
                Text = formatter(state, exception), //exception?.Message ?? state.ToString()
                Exception = exception,
                EventId = eventId,
                State = state
            };

            switch (state)
            {
                case string _: //Нет гарантий, что state - строка
                    logItem.StateText = state.ToString(); //если используется структурированный лог
                    break;
                case IEnumerable<KeyValuePair<string, object>> props: //если state 
                {
                    //props = (IEnumerable<KeyValuePair<string, object>>) state
                    logItem.StateProperties = new Dictionary<string, object>();
                    
                    //KeyValuePair<string, object> item; string key = item.Key; object value = item.Value;
                    foreach ((string key, object value) in props)
                    {
                        logItem.StateProperties[key] = value;
                    }

                    break;
                }
            }

            //Сбор информации о областях, если они есть
            Provider.ScopeProvider?.ForEachScope((scope, logProps) =>
            {
                logItem.Scopes ??= new List<LogScopeInfo>();

                var scopeInfo = new LogScopeInfo();
                logItem.Scopes.Add(scopeInfo);

                switch (scope)
                {
                    case string _:
                        scopeInfo.Text = scope.ToString();
                        break;
                    case IEnumerable<KeyValuePair<string, object>> props:
                    {
                        scopeInfo.Properties ??= new Dictionary<string, object>();

                        foreach ((string key, object value) in props)
                        {
                            scopeInfo.Properties[key] = value;
                        }

                        break;
                    }
                }
            }, state);

            //Запись лога
            Provider.WriteLog(logItem);
        }
    }
}
