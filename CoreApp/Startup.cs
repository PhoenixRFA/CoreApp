using System.Collections.Generic;
using System.IO;
using CoreApp.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace CoreApp
{
    //Cодержит логику обработки входящих запросов
    //For more information on how to configure your application, visit https://docs.microsoft.com/ru-ru/aspnet/core/fundamentals/startup?view=aspnetcore-5.0
    //Является входной точкой в приложение ASP.NET Core
    public class Startup
    {
        //Из конструктора Startup можно получить объекты: IWebHostEnvironment, IHostEnvironment и IConfiguration (ни одного или все сразу)
        public Startup(IHostEnvironment env2, IConfiguration config)
        {

        }

        //Настройка служб приложения - НЕ обязательный метод, вызывается ПЕРЕД Configure
        //Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
        }

        //В классе Startup могут присутствовать методы вида Configure{EnvironmentName}Services и Configure{EnvironmentName}
        //которые будут вызываться в соответствующих средах
        //если такие методы не будут найдены, то будут использованы простые ConfigureServices и Configure
        /*public void ConfigureDevelopmentServices(IServiceCollection services) { }
        public void ConfigureDevelopment(IApplicationBuilder app)
        {
            app.Run(context=>context.Response.WriteAsync("<h1>This is Develoment!</h1>"));
        }*/

        //Настройка конвейера обработки запросов
        //Use this method to configure the HTTP request pipeline
        //  IApplicationBuilder - обязательный параметр для метода Configure
        //  IWebHostEnvironment - НЕ обязательный параметр. Позволяет получить информацию о среде
        //  ILoggerFactory      - НЕ обязательный параметр.
        //  в качестве параметров можно передавать любой сервис, зарегистрированный в методе ConfigureServices
        //Выполняется один раз при создании объекта класса Startup
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UsePerformanceTimer();

            //если проиложение в разработке
            if (env.IsDevelopment())
            {
                //выводить специальную страницу с описанием ошибки
                app.UseDeveloperExceptionPage();
            }

            //Объединяет вызов UseDefaultFiles, UseDirectoryBrowser и UseStaticFiles
            //app.UseFileServer(new FileServerOptions
            //{
            //    StaticFileOptions = {
            //        FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")),
            //        RequestPath = string.Empty
            //    },

            //    EnableDirectoryBrowsing = true,
            //    DirectoryBrowserOptions =
            //    {
            //        FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "content")),
            //        RequestPath = "/admin/contentFolder"
            //    },

            //    EnableDefaultFiles = true,
            //    DefaultFilesOptions =
            //    {
            //        DefaultFileNames = new List<string>{ "index.html" }
            //    }
            //});

            ////позволяет пользователям просматривать содержимое каталога wwwroot
            ////также можно переопределить на какую папку будет указывать DirectoryBrowser
            ////и по какому url DirectoryBrowser будет "открываться"
            //app.UseDirectoryBrowser(new DirectoryBrowserOptions
            //{
            //    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"content")),
            //    RequestPath = "/admin/contentFolder"
            //});
            ////при обращении к корню ищет статические файлы: default.htm, default.html, index.htm, index.html
            ////файлы по-умолчанию можно переопределить
            app.UseDefaultFiles(new DefaultFilesOptions
            {
                DefaultFileNames = new List<string> { "index.html" }
            });
            //кстати UseDefaultFiles не прерывает конвеер, а только подставляет нужный path 
            app.Run(context=>context.Response.WriteAsync(context.Request.Path));
            
            //middleware для использования статичных файлов
            app.UseStaticFiles();
            
            //включаем в конвеер свой компонент middleware
            //app.UseMiddleware<TokenMiddleware>();
            //либо через метод расширения
            app.UseToken("1234");

            //включаем возможность маршрутизации, например - использование метода UseEndpoints
            app.UseRouting();

            //! Компоненты middleware создаются один раз и живут в течение всего жизненного цикла приложения
            //т.е. значение x не будет сбрасываться между запросами
            int x = 1;
            
            //установка маршрутов
            app.UseEndpoints(endpoints =>
            {
                //обработка Get запроса по адресу "/"
                endpoints.MapGet("/", async context =>
                {
                    x += 7;
                    //отправка ответа прямо в контекст
                    await context.Response.WriteAsync($"Hello World! x={x}");
                });

                endpoints.MapGet("/env", async context =>
                {
                    bool rootExists = env.ContentRootFileProvider.GetDirectoryContents("/").Exists;
                    string result = $"<h1>IWebHostEnvironment</h1><p>ApplicationName: {env.ApplicationName}<br>EnvironmentName: {env.EnvironmentName}<br>WebRootPath: {env.WebRootPath}<br>ContentRootPath: {env.ContentRootPath}<br>Is root exists: {rootExists}</p>";
                    await context.Response.WriteAsync(result);
                });
            });

            app.Use(async (context, next) =>
            {
                x -= 3;
                await next.Invoke();
                x -= 1;
            });

            app.Map("/index", builder =>
            {
                builder.Run(context=>context.Response.WriteAsync("This is Index"));
            });

            app.MapWhen(context => context.Request.Path.Value.Contains("foo"), builder =>
            {
                builder.Run(context => context.Response.WriteAsync("<h1>BAR!!</h1>"));
            });
            
            //Для создания компонентов middleware используется делегат RequestDelegate
            //Он выполняет некоторое действие и принимает контекст запроса
            RequestDelegate delegateExample = context => context.Response.WriteAsync($"Not Found! x={x}");

            //pp.Run(delegateExample);

            app.Use(async (context, next) => await next());
        }
    }

    public class StartupDevelopment
    {
        public void Configure(IApplicationBuilder app)
        {
            app.Run(context=>context.Response.WriteAsync("<h1>This is Develoment</h1>"));
        }
    }

    /* встроенные компоненты middleware:
     *
     * Authentication:                          предоставляет поддержку аутентификации
     * Cookie Policy:                           отслеживает согласие пользователя на хранение связанной с ним информации в куках
     * CORS:                                    обеспечивает поддержку кроссдоменных запросов
     * Diagnostics:                             предоставляет страницы статусных кодов, функционал обработки исключений, страницу исключений разработчика
     * Forwarded Headers:                       перенаправляет зголовки запроса
     * Health Check:                            проверяет работоспособность приложения asp.net core
     * HTTP Method Override:                    позволяет входящему POST-запросу переопределить метод
     * HTTPS Redirection:                       перенаправляет все запросы HTTP на HTTPS
     * HTTP Strict Transport Security (HSTS):   для улучшения безопасности приложения добавляет специальный заголовок ответа
     * MVC:                                     обеспечивает функционал фреймворка MVC
     * Request Localization:                    обеспечивает поддержку локализации
     * Response Caching:                        позволяет кэшировать результаты запросов
     * Response Compression:                    обеспечивает сжатие ответа клиенту
     * URL Rewrite:                             предоставляет функциональность URL Rewriting
     * Endpoint Routing:                        предоставляет механизм маршрутизации
     * Session:                                 предоставляет поддержку сессий
     * Static Files:                            предоставляет поддержку обработки статических файлов
     * WebSockets:                              добавляет поддержку протокола WebSockets
     */
}
