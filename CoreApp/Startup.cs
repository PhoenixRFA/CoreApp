using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreApp.Middleware;
using CoreApp.Models;
using CoreApp.Services;
using CoreApp.Services.Loggers;
using CoreApp.Services.Loggers.ColorConsole;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CoreApp
{
    //C������� ������ ��������� �������� ��������
    //For more information on how to configure your application, visit https://docs.microsoft.com/ru-ru/aspnet/core/fundamentals/startup?view=aspnetcore-5.0
    //�������� ������� ������ � ���������� ASP.NET Core
    public class Startup
    {
        private IServiceCollection _services;
        private readonly IConfiguration _exampleConfig;
        private readonly IConfiguration _appConfig;

        //�� ������������ Startup ����� �������� �������: IWebHostEnvironment, IHostEnvironment � IConfiguration (�� ������ ��� ��� �����)
        public Startup(IHostEnvironment env2, IConfiguration config)
        {
            _appConfig = config;

            _exampleConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"login", "user1"},
                    {"password", "qwerty"},
                    {"foobar", "123"}
                })
                .Build();
        }

        //��������� ����� ���������� - �� ������������ �����, ���������� ����� Configure
        //Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            //�� ��������� ���������� �������� ��� �������� (78 ����)
            _services = services;

            ////����� ��������� ��������� ������������� (���� � ������ �������������)
            //services.AddHttpsRedirection(options =>
            //{
            //    options.HttpsPort = 44334;
            //    options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
            //});

            ////��������� ��� HTST
            //services.AddHsts(opts =>
            //{
            //    opts.ExcludedHosts.Add("ignore.me");
            //    opts.IncludeSubDomains = true;
            //    opts.MaxAge = TimeSpan.FromDays(366);
            //    opts.Preload = true;
            //});

            //����������� ������ �������
            //services.AddTransient<IMessageSender, EmailMessageSender>();
            //services.AddTransient<IMessageSender, SmsMessageSender>();
            services.AddSingleton<IMessageSender, RandomMessageSender>();
            
            //������ ������� ��������. ������ ���������� ����� ������������ � ����������� �� �������
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
            //��� �������� ����� ������������ ������ ����������
            //services.AddEmailMessageSender();
            //������� ����� ���������� �� ��������� ���������
            //� ����� ������������ ServiceUsingExample sender
            //� ���� ������ � ����������� ����� ����������� ����������� �����������
            services.AddTransient<ServiceUsingExample>();

            //�������� ������� � ������� � ���������
            //services.Configure<ExampleOptions>(_exampleConfig);
            services.Configure<ExampleOptions>(_appConfig.GetSection("exampleSection"));
            //services.Configure<ExampleOptions>("exampleSection", _appConfig);

            services.AddDistributedMemoryCache();
            services.AddSession(opts=>
            {
                opts.IdleTimeout = TimeSpan.FromMinutes(10);
                opts.Cookie.Name = "session_cookie";
            });
        }

        //� ������ Startup ����� �������������� ������ ���� Configure{EnvironmentName}Services � Configure{EnvironmentName}
        //������� ����� ���������� � ��������������� ������
        //���� ����� ������ �� ����� �������, �� ����� ������������ ������� ConfigureServices � Configure
        /*public void ConfigureDevelopmentServices(IServiceCollection services) { }
        public void ConfigureDevelopment(IApplicationBuilder app)
        {
            app.Run(context=>context.Response.WriteAsync("<h1>This is Develoment!</h1>"));
        }*/

        //��������� ��������� ��������� ��������
        //Use this method to configure the HTTP request pipeline
        //  IApplicationBuilder - ������������ �������� ��� ������ Configure
        //  IWebHostEnvironment - �� ������������ ��������. ��������� �������� ���������� � �����
        //  ILoggerFactory      - �� ������������ ��������.
        //  � �������� ���������� ����� ���������� ����� ������, ������������������ � ������ ConfigureServices
        //����������� ���� ��� ��� �������� ������� ������ Startup
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IMessageSender sender, ServiceUsingExample senderService, ILogger<Startup> loger1, ILoggerFactory loggerFactory)
        {
            #region RouterMiddleware - ������� ������������� �� ����. ������
            //������ ��������� �������

            var routeHandler = new RouteHandler(async context =>
            {
                //��������� ������ ��������
                RouteData routeData = context.GetRouteData();

                string s = "";
                foreach(KeyValuePair<string, object> item in routeData.Values)
                {
                    s += $" {item.Key}={item.Value};";
                }

                //��������� ������ �������� �� �����
                object id = context.GetRouteValue("id");

                await context.Response.WriteAsync("Router middleware example " + s + $" {(id == null ? string.Empty : id)}");
            });

            var routeBuilder = new RouteBuilder(app, routeHandler);

            //������ ������������� midleware
            routeBuilder.MapMiddlewareGet("middleware/{action}", middl =>
            {
                middl.Run(async context => await context.Response.WriteAsync("middleware example"));
            });
            
            //������ ������������� ������������� ������
            routeBuilder.MapVerb("GET", "test/{action}/{id?}", async (request, response, route) => {
                await response.WriteAsync("MapVerbExample");
            });

            //������ ������������ ���������� ��� Post ������
            routeBuilder.MapPost("test/{action}", async context => {
                await context.Response.WriteAsync("POST: test/");
            });

            //������ ����������� �������� (���������� routeHandler)
            routeBuilder.MapRoute("default", @"{controller:regex(^H.*)=home}/{action:alpha:minlength(3)=index}/{id:regex(\d+)?}");
            //routeBuilder.MapRoute("default",
            //    "{controller}/{action}/{id?}",
            //    new { controller = "home", action = "index" },                                            //������ ���������� �����������
            //    new { httpMethod = new HttpMethodRouteConstraint("GET"), controller = "^H.*", id = @"\d+", action = new CompositeRouteConstraint(new IRouteConstraint[]{
            //        new AlphaRouteConstraint(),
            //        new MinLengthRouteConstraint(3)
            //})});
            //Microsoft.AspNetCore.Routing.Constraints.* - �������������� ����������

            app.UseRouter(routeBuilder.Build());

            app.Run(async context =>
            {
                await context.Response.WriteAsync("Route miss");
            });

            #endregion

            app.UsePerformanceTimer();

            //Logging example
            //ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            //{
            //    //����� ���������� �� �������
            //    //builder.AddConsole();
            //    //����� ���������� � ���� Output
            //    builder.AddDebug();
            //    //������������ � ��� ETW (Event Tracing for Windows)
            //    //builder.AddEventSourceLogger();
            //    //���������� � Windows Event Log
            //    //builder.AddEventLog();
            //});
            //ILogger<ExampleOptions> testLogger = loggerFactory.CreateLogger<ExampleOptions>();

            //loggerFactory.AddFile(Path.Combine(Directory.GetCurrentDirectory(), "log.txt"));
            loggerFactory.AddProvider(new ColorConsoleLoggerProvider(new ColorConsoleLoggerConfiguration
            {
                Color = ConsoleColor.DarkCyan,
                LogLevel = LogLevel.Warning
            }));
            
            ILogger fileLogger = loggerFactory.CreateLogger("FileLogger");

            app.Use((context, next) =>
            {
                string path = context.Request.Path;
                string id = context.Request.Query["id"];

                using (loger1.BeginScope("String format"))
                {
                    loger1.LogTrace(100, "Trace from loger1");
                    loger1.LogDebug("Debug from loger1 {path} id={id}", path, id);
                    loger1.LogInformation("Info from loger1");
                    loger1.LogWarning("Warning from loger1");
                    loger1.LogError(new Exception("Test exception for logger1"), "Error from loger1");
                    loger1.LogCritical(new EventId(123, "eventID_123"), "Critical from loger1");
                }
                //testLogger.LogInformation("Hello from test logger");
                fileLogger.LogInformation("[{time}] Example log", DateTime.Now);

                //performance example
                using (loger1.ScopeExample(123))
                {
                    loger1.WithoutParameters();
                    loger1.OneParameter(100);
                    loger1.TwoParameters("Foo-string", 777);
                    loger1.WithException(333, new Exception("Test exception"));
                }

                return next();
            });

            //���� ����������� � ����������
            if (env.IsDevelopment())
            {
                //�������� ����������� �������� � ��������� ������
                app.UseDeveloperExceptionPage();
            }
            else
            {
                //���������� ������, ������������ ��� production ���������
                //����� �������� ���� � ����������� ������
                //!���������� �� ���������� ��������, �.�. ���� � ������ �������� ����
                app.UseExceptionHandler("/errorHandler");

                //��������� ��������� HSTS. ������������� � ConfigureServices
                //app.UseHsts();
            }

            //���������� ������������� �� https. ��������� ����� ��������� � ConfigureServices
            app.UseHttpsRedirection();

            //����������� ���������� ��� Http ������ (���� 400-599)
            //����� �������� ��������� ���������, ������ {0} ����� ��������� ���
            app.UseStatusCodePages(/*"text/plain", "Code: {0}"*/);
            ////������ ���������� ���������� ������������� �� ������ �������� (302 / Found). ���� ����� �������� ��� ������ ����� {0}
            //app.UseStatusCodePagesWithRedirects("/env{0}");
            //��� ������� - ������ ������ �� ���� �� ����
            ////��� ����������� ���� � ����������� � ������ � �����������, ��� {0} ��������� ���
            //app.UseStatusCodePagesWithReExecute("/env", "?id={0}");

            //���������� ����� UseDefaultFiles, UseDirectoryBrowser � UseStaticFiles
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

            ////��������� ������������� ������������� ���������� �������� wwwroot
            ////����� ����� �������������� �� ����� ����� ����� ��������� DirectoryBrowser
            ////� �� ������ url DirectoryBrowser ����� "�����������"
            //app.UseDirectoryBrowser(new DirectoryBrowserOptions
            //{
            //    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"content")),
            //    RequestPath = "/admin/contentFolder"
            //});
            ////��� ��������� � ����� ���� ����������� �����: default.htm, default.html, index.htm, index.html
            ////����� ��-��������� ����� ��������������
            //app.UseDefaultFiles(new DefaultFilesOptions
            //{
            //    DefaultFileNames = new List<string> { "index.html" }
            //});
            //������ UseDefaultFiles �� ��������� �������, � ������ ����������� ������ path 
            //app.Run(context=>context.Response.WriteAsync(context.Request.Path));

            //middleware ��� ������������� ��������� ������
            //app.UseStaticFiles();

            //�������� � ������� ���� ��������� middleware
            //app.UseMiddleware<TokenMiddleware>();
            //���� ����� ����� ����������
            //app.UseToken("1234");

            //������
            app.UseSession();

            //�������� ����������� �������������, �������� - ������������� ������ UseEndpoints
            app.UseRouting();
            
            //! ���������� middleware ��������� ���� ��� � ����� � ������� ����� ���������� ����� ����������
            //�.�. �������� x �� ����� ������������ ����� ���������
            int x = 1;
            
            //��������� ���������
            app.UseEndpoints(endpoints =>
            {
                //��������� Get ������� �� ������ "/"
                endpoints.MapGet("/", async context =>
                {
                    x += 7;
                    //�������� ������ ����� � ��������
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

                //������������. ������ �������� ������/���������
                endpoints.MapGet("/getConfig", context =>
                {
                    string username = _exampleConfig["username"];
                    string[] configs = _exampleConfig.GetChildren().Select(x=>$"Path={x.Path}; {x.Key}={x.Value}").ToArray();
                    
                    //������� �������� ������������ � �������                //��-��������� ������������� ������ ��������� ����������
                    var bindedOptions = new ExampleOptions();                //���� ���������� BindNonPublicProperties = true, �� ����� ������������� ��� ����������, �� ����������� readonly
                    _exampleConfig.Bind(bindedOptions, options => options.BindNonPublicProperties = true);
                    var bindedOptionsAlt = _exampleConfig.Get<ExampleOptions>();
                    var bindedOptionsAlt2 = _exampleConfig.Get<ExampleOptions>(options => options.BindNonPublicProperties = true);

                    return context.Response.WriteAsync($"username: {username}\n\nAll configs:\n{string.Join("\n", configs)}");
                });
                endpoints.MapGet("/getConfigAll", context =>
                {
                    string[] configs = _appConfig.GetChildren().Select(x=>$"{x.Path}={x.Value}").ToArray();
                    
                    return context.Response.WriteAsync($"All configs:\n\n{string.Join("\n", configs)}");
                });
                endpoints.MapGet("/updateConfig", context =>
                {
                    string name = context.Request.Query["name"];
                    string value = context.Request.Query["value"];

                    bool isRecordExists = !string.IsNullOrEmpty(_exampleConfig[name]);

                    _exampleConfig[name] = value;

                    return context.Response.WriteAsync($"Record {(isRecordExists ? "updated" : "added")}  {name}={value}");
                });
                endpoints.MapGet("/deleteConfig", context =>
                {
                    string name = context.Request.Query["name"];

                    bool isRecordExists = !string.IsNullOrEmpty(_exampleConfig[name]);

                    if (isRecordExists)
                    {
                        _exampleConfig[name] = null;
                    }

                    return context.Response.WriteAsync($"Record {(isRecordExists ? "deleted" : "not exists")}");
                });

                //Cookies
                endpoints.MapGet("/getCookie", context =>
                {
                    int cookieCount = context.Request.Cookies.Count;
                    string cookieKeys = string.Join(", ", context.Request.Cookies.Keys);
                    string myCookie = context.Request.Cookies["myCookie"];
                    
                    return context.Response.WriteAsync($"Total cookies: {cookieCount}\nKeys: {cookieKeys}\nMyCookie: {myCookie}");
                });
                endpoints.MapGet("/setCookie", context =>
                {
                    string name = context.Request.Query["name"];
                    string value = context.Request.Query["value"];
                    
                    context.Response.Cookies.Append(name, value);

                    return context.Response.WriteAsync($"Set cookie: {name}={value}");
                });
                endpoints.MapGet("/setCookie2", context =>
                {
                    string name = context.Request.Query["name"];
                    string value = context.Request.Query["value"];
                    
                    var cookieOptions = new CookieOptions
                    {
                        Path = "/",
                        MaxAge = TimeSpan.FromDays(2),
                        Domain = "localhost",
                        Expires = DateTimeOffset.Now.AddDays(1),
                        HttpOnly = true,
                        IsEssential = true,
                        SameSite = SameSiteMode.Lax,
                        Secure = true
                    };

                    context.Response.Cookies.Append(name, value, cookieOptions);

                    return context.Response.WriteAsync($"Set cookie: {name}={value}");
                });
                
                //Session
                endpoints.MapGet("/getSession", context =>
                {
                    bool isSessionAvailable = context.Session.IsAvailable;
                    string sessionKeys = string.Join(", ", context.Session.Keys);
                    string sessionID = context.Session.Id;
                    string foo = context.Session.GetString("foo");
                    
                    return context.Response.WriteAsync($"SessionAvailable: {isSessionAvailable}\nKeys: {sessionKeys}\nID: {sessionID}\nfoo: {foo}");
                });
                endpoints.MapGet("/setSession", context =>
                {
                    string value = context.Request.Query["value"];
                    context.Session.SetString("foo", value);
                    
                    return context.Response.WriteAsync($"Session set");
                });

                //��������� ��� ������ � ������ ��������� ������:
                //Transient:
                //  � /send � /send2 �������� �� ����� ������� ����� ���������, �� � ������� ����� ��� ��������
                //  ��� ��-�� ����, ��� Configure ���������� ���� ��� ��� ���������� ���������� � ���������� ������� �������� �������� ���� � ����
                //  � /send3 � /send4 ������ ��� ����� �������� ��������, �.�. ������ ��� ������������� ����� ������
                //Scoped:
                //  � /send � /send2 �������� ����� ���� � ����, �.�. ���������� � ����� ������ ���� � ������ ��������
                //  ��� ���� ��-�� ����, ��� Configure ���������� ���� ���, �� � ���� ��� ������ ������� ���������� � ������ �������, �.�. senderService._sender == sender
                //  � /send3 ������ ��� ����� �������� ��������
                //  � /send4 �������� ������. � ��� ������� - ������ �������. ������ ����� ���� � ���, ��� ������ ������� ������������� � �������.
                //      � �.�. ��� ���������� ApplicationServices, ��������� � ������ �������, �� ����� ���������� ������� ������ ��������� ��������� ������ � ������ �� ������ ��������.
                //      �� ���� ����� ���� ���� ������ ���������� ������ ��������? ���� �� ��������� =(
                //Singleton:
                //  �� ���� ������� ��������� ���� � �� �� ��������
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
            
            //��� �������� ����������� middleware ������������ ������� RequestDelegate
            //�� ��������� ��������� �������� � ��������� �������� �������
            //RequestDelegate delegateExample = context => context.Response.WriteAsync($"Not Found! x={x}");
            //app.Run(delegateExample);

            //app.Use(async (context, next) => await next());
        }

        /// <summary> ���������� �� ������������� �������� </summary>
        public async Task ListAllServices(HttpContext context)
        {
            var sb = new StringBuilder();
            sb.Append("<h1>��� �������</h1>");
            sb.Append("<table>");
            sb.Append("<tr><th>���</th><th>Lifetime</th><th>����������</th></tr>");
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

    /* ���������� ���������� middleware:
     *
     * Authentication:                          ������������� ��������� ��������������
     * Cookie Policy:                           ����������� �������� ������������ �� �������� ��������� � ��� ���������� � �����
     * CORS:                                    ������������ ��������� ������������� ��������
     * Diagnostics:                             ������������� �������� ��������� �����, ���������� ��������� ����������, �������� ���������� ������������
     * Forwarded Headers:                       �������������� �������� �������
     * Health Check:                            ��������� ����������������� ���������� asp.net core
     * HTTP Method Override:                    ��������� ��������� POST-������� �������������� �����
     * HTTPS Redirection:                       �������������� ��� ������� HTTP �� HTTPS
     * HTTP Strict Transport Security (HSTS):   ��� ��������� ������������ ���������� ��������� ����������� ��������� ������
     * MVC:                                     ������������ ���������� ���������� MVC
     * Request Localization:                    ������������ ��������� �����������
     * Response Caching:                        ��������� ���������� ���������� ��������
     * Response Compression:                    ������������ ������ ������ �������
     * URL Rewrite:                             ������������� ���������������� URL Rewriting
     * Endpoint Routing:                        ������������� �������� �������������
     * Session:                                 ������������� ��������� ������
     * Static Files:                            ������������� ��������� ��������� ����������� ������
     * WebSockets:                              ��������� ��������� ��������� WebSockets
     */

    /* �������� ������������
     *
     * ����� ����������� ������ (�� ����������� ������������ ������ Startup)
     *      ��. ServiceUsingExample
     * ����� �������� ������ Configure ������ Startup
     * ����� �������� ������ Invoke ���������� middleware
     *      ��. TokenMiddleware
     * ����� �������� RequestServices ��������� ������� HttpContext � ����������� middleware
     *      ����������� - service locator, �� ������������� � �������������
     * ����� �������� ApplicationServices ������� IApplicationBuilder � ������ Startup
     *      ������������� ������� ����. ������. ��! ���� ������ �� ������� ��� ������������� Scoped ��������
     *
     * ������������� ������������ ��������� ������� � ������ ����������.
     * ����� ������������� �������� ������ ���������� � namespace Microsoft.Extensions.DependencyInjection
     *
     * ������ ����������: https://docs.microsoft.com/ru-ru/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-5.0
     */

    /* ��������� ���� ��������
     *
     * Transient: ������ ��� ��������� ����� ������ �������
     *      � ������� ������ ������� ����� ���� ��������� ��������� � �������,
     *      �������������� ��� ������ ��������� ����� ����������� ����� ������.
     *      �������� ������ �������� �������� ��� !����������� ��������!,
     *      ������� �� ������ ������ � ���������.
     * Scoped: ��� ������� ������� ��������� ���� ������ �������
     *      � ���� ������ � ������ ������ ������� ��� ��������� ����� ����������
     *      � ������ � ���� �� ������� �������.
     * Singleton: ��� �� ������ - ��� ������ ��������� � ������� ��������� ������
     *      � "�����" ���� ���������� �� ���������.
     *
     * ������ � Singleton �������� ������������ Scoped �����������.
     * ������ ��� scoped ������������� � ��������� �������, � �� ������ ��������
     * Singleton - ������� ��� ���, � ���������� ���� �� �����.
     */

    /* IHostEnvironment VS IWebHostEnvironment
     * �� ����������� ����� ������������ IHostEnvironment, �� ����������� ��� �������, ����� ����� ������ � WebRootPath ��� WebRootFileProvider
     */

    /* ������������ ����������
     * �������� ���������:
     *  - ��������� ���������� ������
     *  - ���������� ����� ���������
     *  - ������� .net � ������
     *  - ����� json, xml, ini
     *  - azure
     *  - ���������������� ��������� ������������
     *
     * ����� ������������ ����� ���������� ��� �������� � ������� ����� IOptions<TOptions>
     *  ��. TokenMiddleware
     * IOptions - singleton �.�. �� ��������� ���������� �������� ����� ������� ����������,
     * IOptionsSnapshot - scoped �.�. ����������� � ������ ��������
     * IOptionsMonitor - singleton, �� ������ �������� ����� ��������, ����� ����������� �� ���������
     * � ConfigureAppConfiguration � Program ����� �������� ���������� ������������
     */

    /* HttpContext.Items
     * ��������� ���� IDictionary<object, object>
     * ��������� ������ � �������� ������ �������. ����� ������ ����������� - ������ ���������
     */

    /* ������
     * ��� ����������� ����� ���������� �������:
     *      services.AddDistributedMemoryCache();
     *      services.AddSession();
     * � middleware:
     *      app.UseSession();
     */
}
