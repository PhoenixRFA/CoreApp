using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreApp.Middleware;
using CoreApp.Services;
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
        private IServiceCollection _services;

        //Из конструктора Startup можно получить объекты: IWebHostEnvironment, IHostEnvironment и IConfiguration (ни одного или все сразу)
        public Startup(IHostEnvironment env2, IConfiguration config)
        {

        }

        //Настройка служб приложения - НЕ обязательный метод, вызывается ПЕРЕД Configure
        //Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            //По умолчанию приложение содержит ряд сервисов (78 штук)
            _services = services;

            ////Можно настроить параметры переадресации (порт и статус переадресации)
            //services.AddHttpsRedirection(options =>
            //{
            //    options.HttpsPort = 44334;
            //    options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
            //});

            ////Настройки для HTST
            //services.AddHsts(opts =>
            //{
            //    opts.ExcludedHosts.Add("ignore.me");
            //    opts.IncludeSubDomains = true;
            //    opts.MaxAge = TimeSpan.FromDays(366);
            //    opts.Preload = true;
            //});

            //Подключение своего сервиса
            //services.AddTransient<IMessageSender, EmailMessageSender>();
            //services.AddTransient<IMessageSender, SmsMessageSender>();
            services.AddSingleton<IMessageSender, RandomMessageSender>();
            
            //Пример фабрики сервисов. Нужная реализация будет возвращаться в замисимости от времени
            services.AddTransient<IMessageSender>(services =>
            {
                int seconds = DateTime.Now.Second;
                if (seconds < 20)
                {
                    return new EmailMessageSender();
                }
                else if (seconds < 40)
                {
                    return new SmsMessageSender();
                }
                else
                {
                    return new RandomMessageSender();
                }
            });
            //Для удобства можно использовать методы расширения
            //services.AddEmailMessageSender();
            //Сервисы можно подключать не используя интерфейс
            //И потом использовать ServiceUsingExample sender
            //в этом случае в конструктор класа подставятся необходимые зависимости
            services.AddTransient<ServiceUsingExample>();
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
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IMessageSender sender, ServiceUsingExample senderService)
        {
            app.UsePerformanceTimer();

            //если проиложение в разработке
            if (env.IsDevelopment())
            {
                //выводить специальную страницу с описанием ошибки
                app.UseDeveloperExceptionPage();
            }
            else
            {
                //обработчик ошибок, используется для production окружения
                //можно передать путь к обработчику ошибок
                //!Обработчик не производит редирект, т.е. путь и статус остаются теже
                app.UseExceptionHandler("/errorHandler");

                //Добавляет заголовки HSTS. Настраивается в ConfigureServices
                //app.UseHsts();
            }

            //Производит переадресацию на https. Параметры можно настроить в ConfigureServices
            app.UseHttpsRedirection();

            //Стандартный обработчик для Http ошибок (коды 400-599)
            //Можно заменить выводимое сообщение, вместо {0} будек статусный код
            app.UseStatusCodePages(/*"text/plain", "Code: {0}"*/);
            ////другой обработчик производит переадресацию на другую страницу (302 / Found). Тоже можно передать код ошибки через {0}
            //app.UseStatusCodePagesWithRedirects("/env{0}");
            //еще вариант - рендер ответа по тому же пути
            ////тут указывается путь к обработчику и строка с параметрами, где {0} статусный код
            //app.UseStatusCodePagesWithReExecute("/env", "?id={0}");

            //Объединяет вызов UseDefaultFiles, UseDirectoryBrowser и UseStaticFiles
            app.UseFileServer(new FileServerOptions
            {
                StaticFileOptions = {
                    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot")),
                    RequestPath = string.Empty
                },

                EnableDirectoryBrowsing = true,
                DirectoryBrowserOptions =
                {
                    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "content")),
                    RequestPath = "/admin/contentFolder"
                },

                EnableDefaultFiles = true,
                DefaultFilesOptions =
                {
                    DefaultFileNames = new List<string>{ "index.html" }
                }
            });

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
            //app.UseDefaultFiles(new DefaultFilesOptions
            //{
            //    DefaultFileNames = new List<string> { "index.html" }
            //});
            //кстати UseDefaultFiles не прерывает конвеер, а только подставляет нужный path 
            //app.Run(context=>context.Response.WriteAsync(context.Request.Path));

            //middleware для использования статичных файлов
            //app.UseStaticFiles();

            //включаем в конвеер свой компонент middleware
            //app.UseMiddleware<TokenMiddleware>();
            //либо через метод расширения
            //app.UseToken("1234");

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

                endpoints.MapGet("/err", context => throw new Exception("This is test exception"));
                
                endpoints.MapGet("/errorHandler", context => context.Response.WriteAsync("<h1>Oops!</h1><h2>Some error happened!<h2>"));
                endpoints.MapGet("/listServices", ListAllServices);
                
                //Замечание при работе с разным жизненным циклом:
                //Transient:
                //  в /send и /send2 значение не будет менятся между запросами, но у каждого будет своё значение
                //  это из-за того, что Configure вызывается один раз при построении приложения и переданные объекты сервисов остаются одни и теже
                //  в /send3 и /send4 каждый раз будет меняться значение, т.к. каждый раз запрашивается новый сервис
                //Scoped:
                //  в /send и /send2 значение будет одно и тоже, т.е. одинаковое у обоих причем даже в разных запросах
                //  это тоже из-за того, что Configure вызывается один раз, но в этот раз объект сервиса одинаковый в рамках запроса, т.е. senderService._sender == sender
                //  в /send3 каждый раз будет меняться значение
                //  в /send4 вернется ошибка. В чем причина - сложно сказать. Скорее всего дело в том, что объект сервиса привязывается к запросу.
                //      А т.к. тут вызывается ApplicationServices, созданный в первом запросе, то после завершения запроса объект удаляется сборщиком мусора и ссылка на сервис теряется.
                //      По идее тогда этот путь должен отработать первым запросом? Идея не сработала =(
                //Singleton:
                //  во всех случаях выводится одно и то же значение
                endpoints.MapGet("/send", context => context.Response.WriteAsync(sender.Send()));
                endpoints.MapGet("/send2", context =>
                {
                    string result = senderService.Send();
                    return context.Response.WriteAsync(result);
                });
                endpoints.MapGet("/send3", context =>
                {
                    var sender = context.RequestServices.GetService<IMessageSender>();
                    string result = sender.Send();
                    return context.Response.WriteAsync(result);
                });
                endpoints.MapGet("/send4", context =>
                {
                    var sender = app.ApplicationServices.GetService<IMessageSender>();
                    string result = sender.Send();
                    return context.Response.WriteAsync(result);
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
            //RequestDelegate delegateExample = context => context.Response.WriteAsync($"Not Found! x={x}");
            //app.Run(delegateExample);

            //app.Use(async (context, next) => await next());
        }

        /// <summary> Информация по установленным сервисах </summary>
        public async Task ListAllServices(HttpContext context)
        {
            var sb = new StringBuilder();
            sb.Append("<h1>Все сервисы</h1>");
            sb.Append("<table>");
            sb.Append("<tr><th>Тип</th><th>Lifetime</th><th>Реализация</th></tr>");
            foreach (ServiceDescriptor svc in _services)
            {
                sb.Append("<tr>");
                sb.Append($"<td>{svc.ServiceType.FullName}</td>");
                sb.Append($"<td>{svc.Lifetime}</td>");
                sb.Append($"<td>{svc.ImplementationType?.FullName}</td>");
                sb.Append("</tr>");
            }
            sb.Append("</table>");
            context.Response.ContentType = "text/html;charset=utf-8";
            await context.Response.WriteAsync(sb.ToString());
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

    /* Передача зависимостей
     *
     * Через конструктор класса (за исключением конструктора класса Startup)
     *      см. ServiceUsingExample
     * Через параметр метода Configure класса Startup
     * Через параметр метода Invoke компонента middleware
     *      см. TokenMiddleware
     * Через свойство RequestServices контекста запроса HttpContext в компонентах middleware
     *      антипаттерн - service locator, не рекомендуется к использованию
     * Через свойство ApplicationServices объекта IApplicationBuilder в классе Startup
     *      использование подобно пред. случаю. НО! Этот способ не годится при использовании Scoped сервисов
     *
     * Рекомендуется группировать связанные сервисы в методы расширения.
     * Также рекомендуется помещать методы расширения в namespace Microsoft.Extensions.DependencyInjection
     *
     * Больше информации: https://docs.microsoft.com/ru-ru/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-5.0
     */

    /* Жизненный цикл сервисов
     *
     * Transient: каждый раз создается новый объект сервиса
     *      В течение одного запроса может быть несколько обращений к сервису,
     *      соответственно при каждом обращении будет создаваться новый объект.
     *      Подобная модель наиболее подходит для !легковесных сервисов!,
     *      которые не хранят данных о состоянии.
     * Scoped: для каждого запроса создается свой объект сервиса
     *      В этом случае в рамках одного запроса все обращения будут направлены
     *      к одному и тому же объекту сервиса.
     * Singleton: тут всё просто - при первом обращении к сервису создается объект
     *      и "живет" пока приложение не закроется.
     *
     * Нельзя в Singleton сервисах использовать Scoped зависимости.
     * Потому что scoped привязывается к контексту запроса, а на момент создания
     * Singleton - запроса еще нет, и зависимоти тоже не будет.
     */

    /* IHostEnvironment VS IWebHostEnvironment
     * По возможности стоит использовать IHostEnvironment, за исключением тех случаев, когда нужен доступ к WebRootPath или WebRootFileProvider
     */
}
