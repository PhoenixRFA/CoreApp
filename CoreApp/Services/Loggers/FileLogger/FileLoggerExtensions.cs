using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;

namespace CoreApp.Services.Loggers.FileLogger
{
    /// <summary> Методы расширения для использования логера в файл </summary>
    public static class FileLoggerExtensions
    {
        /// <summary> Позволяет использовать провайдер логера в файл </summary>
        public static ILoggingBuilder AddFile(this ILoggingBuilder builder)
        {
            //Добавляет сервисы необходимые для использования ILoggerProviderConfigurationFactory и ILoggerProviderConfiguration (позволяет получить доступ к разделу конфигурации, связанному с поставщиком логера) ?Всё равно не совсем понятно
            builder.AddConfiguration();

            //Регистрирую сам провайдер
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, FileLoggerProvider>());
            //Регистрирую привязчик конфигурации (application.json)
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<FileLoggerConfiguration>, FileLoggerConfigurationSetup>());
            //Подиписка на событие изменения конфигурации
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IOptionsChangeTokenSource<FileLoggerConfiguration>, LoggerProviderOptionsChangeTokenSource<FileLoggerConfiguration, FileLoggerProvider>>());

            return builder;
        }

        /// <summary> Позволяет использовать провайдер логера в файл. И настроить конфиг через <paramref name="configure"/></summary>
        public static ILoggingBuilder AddFile(this ILoggingBuilder builder, Action<FileLoggerConfiguration> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            builder.AddFile();
            builder.Services.Configure(configure);

            return builder;
        }

        //Устаревшая реализация
        //public static ILoggerFactory AddFile(this ILoggerFactory factory, string path)
        //{
        //    factory.AddProvider(new FileLoggerProvider(path));
        //    return factory;
        //}
    }
}