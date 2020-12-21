using System;
using Microsoft.Extensions.Logging;

namespace CoreApp.Services.Loggers
{
    /// <summary> Пример высокопроизводительного ведения журналов с использованием кэшируемых делегатов </summary>
    public static class LoggerExtensions
    {
        private static readonly Action<ILogger, Exception> _withoutParameters;
        private static readonly Action<ILogger, int, Exception> _oneParameter;
        private static readonly Action<ILogger, string, int, Exception> _twoParameters;
        private static readonly Action<ILogger, int, Exception> _withExceptionAndParameter;
        private static readonly Func<ILogger, int, IDisposable> _scopeExample;

        private static readonly EventId EventNo1 = new EventId(1, nameof(WithoutParameters));
        private static readonly EventId EventNo2 = new EventId(2, nameof(OneParameter));
        private static readonly EventId EventNo3 = new EventId(3, nameof(TwoParameters));
        private static readonly EventId EventNo4 = new EventId(4, nameof(WithException));

        static LoggerExtensions()
        {
            _withoutParameters = LoggerMessage.Define(LogLevel.Information, EventNo1, "Simple log example");
            
            _oneParameter = LoggerMessage.Define<int>(LogLevel.Information, EventNo2, "Parametered exaple Id={Id})");
            
            _twoParameters = LoggerMessage.Define<string, int>(LogLevel.Information, EventNo3, "Parameters Foo = '{Foo}' Bar = {Bar})");

            _withExceptionAndParameter = LoggerMessage.Define<int>(LogLevel.Error, EventNo4, "Exception on Id={Id}");
            
            _scopeExample = LoggerMessage.DefineScope<int>("Scope example (Id = {Id})");
        }

        public static void WithoutParameters(this ILogger logger)
        {
            _withoutParameters(logger, null);
        }

        public static void OneParameter(this ILogger logger, int id)
        {
            _oneParameter(logger, id, null);
        }

        public static void TwoParameters(this ILogger logger, string foo, int bar)
        {
            _twoParameters(logger, foo, bar, null);
        }

        public static void WithException(this ILogger logger, int id, Exception ex)
        {
            _withExceptionAndParameter(logger, id, ex);
        }

        public static IDisposable ScopeExample(this ILogger logger, int id) => _scopeExample(logger, id);
    }
}
