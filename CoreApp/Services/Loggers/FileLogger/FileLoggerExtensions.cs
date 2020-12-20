using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;

namespace CoreApp.Services.Loggers.FileLogger
{
    /// <summary> ������ ���������� ��� ������������� ������ � ���� </summary>
    public static class FileLoggerExtensions
    {
        /// <summary> ��������� ������������ ��������� ������ � ���� </summary>
        public static ILoggingBuilder AddFile(this ILoggingBuilder builder)
        {
            //��������� ������� ����������� ��� ������������� ILoggerProviderConfigurationFactory � ILoggerProviderConfiguration (��������� �������� ������ � ������� ������������, ���������� � ����������� ������) ?�� ����� �� ������ �������
            builder.AddConfiguration();

            //����������� ��� ���������
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, FileLoggerProvider>());
            //����������� ��������� ������������ (application.json)
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<FileLoggerConfiguration>, FileLoggerConfigurationSetup>());
            //��������� �� ������� ��������� ������������
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IOptionsChangeTokenSource<FileLoggerConfiguration>, LoggerProviderOptionsChangeTokenSource<FileLoggerConfiguration, FileLoggerProvider>>());

            return builder;
        }

        /// <summary> ��������� ������������ ��������� ������ � ����. � ��������� ������ ����� <paramref name="configure"/></summary>
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

        //���������� ����������
        //public static ILoggerFactory AddFile(this ILoggerFactory factory, string path)
        //{
        //    factory.AddProvider(new FileLoggerProvider(path));
        //    return factory;
        //}
    }
}