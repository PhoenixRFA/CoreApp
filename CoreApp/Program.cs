using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CoreApp
{
    //������� ���� ����������, � ���� ���������� ����������
    //��� ������������ � ����������� ���-����, � ������ �������� ��������������� ����������
    public class Program
    {
        public static void Main(string[] args)
        {
            /* ������������� �������� �������:
             *      �������� ������� ������������ �����, ��� ����� ������������� ����� ���������� �����������, ��������, �������������.
             *      UseContentRoot(Directory.GetCurrentDirectory());
             * ������������� ������������ �����:
             *      ��� ����� ����������� ���������� ����� � ��������� "DOTNET_" � ��������� ��������� ������.
             *      ConfigureHostConfiguration(config =>
                    {
                        config.AddEnvironmentVariables(prefix: "DOTNET_");
                        if (args != null) config.AddCommandLine(args);
                    })
             * ������������� ������������ ����������:
             *      ��� ����� ����������� ���������� �� ������ appsettings.json � appsettings.{Environment}.json,
             *      � ����� ���������� ����� � ��������� ��������� ������.
             *      ���� ���������� � ������� ����������, �� ����� ������������ ������ Secret Manager, ������� ��������� ��������� ���������������� ������, ������������ ��� ����������.
             *      ConfigureAppConfiguration((hostingContext, config) =>
                    {
                        var env = hostingContext.HostingEnvironment;

                        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                              .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
                        
                        config.AddEnvironmentVariables();
                        
                        if (args != null) config.AddCommandLine(args);

                        if (env.IsDevelopment() && !string.IsNullOrEmpty(env.ApplicationName))
                        {
                            var appAssembly = Assembly.Load(new AssemblyName(env.ApplicationName));
                            if (appAssembly != null) config.AddUserSecrets(appAssembly, optional: true);
                        }


                    })
             * ��������� ���������� �����������:
             *      ConfigureLogging((hostingContext, logging) =>
                    {
                        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

                        if (isWindows) logging.AddFilter<EventLogLoggerProvider>(level => level >= LogLevel.Warning);

                        logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                        logging.AddConsole();
                        logging.AddDebug();
                        logging.AddEventSourceLogger();

                        if (isWindows) logging.AddEventLog();
                    })
             * ���� ������ � ������� ����������, �� ����� ������������ ��������� �������� (?)
             *      UseDefaultServiceProvider((context, options) =>
                    {
                        var isDevelopment = context.HostingEnvironment.IsDevelopment();
                        options.ValidateScopes = isDevelopment;
                        options.ValidateOnBuild = isDevelopment;
                    })
             */
            Host.CreateDefaultBuilder(args)
                /* ����� ��������� ������������ ���������� �����:
                 * ��������� ������������ �� ���������� ����� � ��������� "ASPNETCORE_":
                 *      ConfigurationBuilder().AddEnvironmentVariables(prefix: "ASPNETCORE_").Build()
                 * ��������� � ����������� ���-������ Kestrel, � ������ �������� ����� ��������������� ����������:
                 *      UseKestrel((builderContext, options) =>
                        {
                            options.Configure(builderContext.Configuration.GetSection("Kestrel"));
                        })
                 * ��������� ��������� Host Filtering, ������� ��������� ����������� ������ ��� ���-������� Kestrel
                 *      ConfigureWebDefaults:217
                 * ���� ���������� ��������� ASPNETCORE_FORWARDEDHEADERS_ENABLED ����� true, ��������� ��������� Forwarded Headers, ������� ��������� ��������� �� ������� ��������� "X-Forwarded-"
                 *      ConfigureWebDefaults:240
                 * ���� ��� ������ ���������� ��������� IIS, �� ������ ����� ����� ������������ ���������� � IIS:
                 *      UseIIS().UseIISIntegration()
                 */
                .ConfigureWebHostDefaults(webBuilder =>
                //������������ ������������� ���-������� ��� ������������� ���-����������
                {
                    //��������������� ��������� ����� ���������� - ����� Startup, � �������� � ����� ���������� ��������� �������� ��������
                    webBuilder.UseStartup<Startup>();
                    
                    //���� ����� ������������ ����� UseStartup(IWebHostBuilder, String)
                    //� ����� ������ Startup ����� �������������� Startup{EnvironmentName}, (�������� StartupDevelopment)
                    //���� ����� Startup{EnvironmentName} �� ����� ������, �� ����� ����������� ����� Startup
                    //string assemblyName = typeof(Startup).GetTypeInfo().Assembly.FullName;
                    //webBuilder.UseStartup(assemblyName);

                    //�� ��������� webRoot ��������� �� ����� wwwroot, �� ����� ������� ���� �����
                    //webBuilder.UseWebRoot("Content");
                })
                //��� ����� �������� ������ ��������� ������������, � ����� ��� ����� �������� � IConfigure
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        {"_custom1", "foo"},
                        {"_custom2", "bar"}
                    });
                })
                //������������ ������� �������� ������ ������������� ������ Logging � ������ appsettings.{Environment}.json
                //������������ ������������
                .ConfigureLogging(logging => {
                    //�������� ����������� ��� ���������������
                    //logging.ClearProviders();
                    //��������� ������������ ������ ������������. ����������� ���� �� ������ ������� � �������
                    logging.SetMinimumLevel(LogLevel.Error);
                })
                //������� ���� - ������ IHost
                .Build()
                //� IHost ���������� ����� Run
                .Run();
        }
    }
}
