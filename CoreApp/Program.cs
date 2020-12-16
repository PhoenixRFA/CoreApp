using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CoreApp
{
    //Главный файл приложения, с него начинается выполнение
    //Тут настривается и запускается веб-хост, в рамках которого разворачивается приложение
    public class Program
    {
        public static void Main(string[] args)
        {
            /* Устанавливает корневой каталог:
             *      Корневой каталог представляет папку, где будет производиться поиск различного содержимого, например, представлений.
             *      UseContentRoot(Directory.GetCurrentDirectory());
             * Устанавливает конфигурацию хоста:
             *      Для этого загружаются переменные среды с префиксом "DOTNET_" и аргументы командной строки.
             *      ConfigureHostConfiguration(config =>
                    {
                        config.AddEnvironmentVariables(prefix: "DOTNET_");
                        if (args != null) config.AddCommandLine(args);
                    })
             * Устанавливает конфигурацию приложения:
             *      Для этого загружается содержимое из файлов appsettings.json и appsettings.{Environment}.json,
             *      а также переменные среды и аргументы командной строки.
             *      Если приложение в статусе разработки, то также используются данные Secret Manager, который позволяет сохранить конфиденциальные данные, используемые при разработке.
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
             * Добавляет провайдеры логирования:
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
             * Если проект в статусе разработки, то также обеспечивает валидацию сервисов (?)
             *      UseDefaultServiceProvider((context, options) =>
                    {
                        var isDevelopment = context.HostingEnvironment.IsDevelopment();
                        options.ValidateScopes = isDevelopment;
                        options.ValidateOnBuild = isDevelopment;
                    })
             */
            Host.CreateDefaultBuilder(args)
                /* Метод выполняет конфигурацию параметров хоста:
                 * Загружает конфигурацию из переменных среды с префиксом "ASPNETCORE_":
                 *      ConfigurationBuilder().AddEnvironmentVariables(prefix: "ASPNETCORE_").Build()
                 * Запускает и настраивает веб-сервер Kestrel, в рамках которого будет разворачиваться приложение:
                 *      UseKestrel((builderContext, options) =>
                        {
                            options.Configure(builderContext.Configuration.GetSection("Kestrel"));
                        })
                 * Добавляет компонент Host Filtering, который позволяет настраивать адреса для веб-сервера Kestrel
                 *      ConfigureWebDefaults:217
                 * Если переменная окружения ASPNETCORE_FORWARDEDHEADERS_ENABLED равна true, добавляет компонент Forwarded Headers, который позволяет считывать из запроса заголовки "X-Forwarded-"
                 *      ConfigureWebDefaults:240
                 * Если для работы приложения требуется IIS, то данный метод также обеспечивает интеграцию с IIS:
                 *      UseIIS().UseIISIntegration()
                 */
                .ConfigureWebHostDefaults(webBuilder =>
                //производится инициализация веб-сервера для развертывания веб-приложения
                {
                    //устанавливается стартовый класс приложения - класс Startup, с которого и будет начинаться обработка входящих запросов
                    webBuilder.UseStartup<Startup>();
                    
                    //либо можно использовать метод UseStartup(IWebHostBuilder, String)
                    //и тогда вместо Startup будет использоваться Startup{EnvironmentName}, (например StartupDevelopment)
                    //если класс Startup{EnvironmentName} не будет найден, то будет использован класс Startup
                    //string assemblyName = typeof(Startup).GetTypeInfo().Assembly.FullName;
                    //webBuilder.UseStartup(assemblyName);

                    //По умолчанию webRoot указывает на папку wwwroot, но можно указать свою папку
                    //webBuilder.UseWebRoot("Content");
                })
                //Тут можно добавить другие источники конфигурации, и тогда они будут доступны в IConfigure
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string>
                    {
                        {"_custom1", "foo"},
                        {"_custom2", "bar"}
                    });
                })
                //Конфигурацию ведения журналов обычно предоставляет раздел Logging в файлах appsettings.{Environment}.json
                //Конфигурация логгирования
                .ConfigureLogging(logging => {
                    //удаление провайдеров для переопределения
                    //logging.ClearProviders();
                    //Установка минимального уровня логгирования. Применяется если не указан уровень в конфиге
                    logging.SetMinimumLevel(LogLevel.Error);
                })
                //создает хост - объект IHost
                .Build()
                //у IHost вызывается метод Run
                .Run();
        }
    }
}
