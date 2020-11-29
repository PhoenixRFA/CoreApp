using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CoreApp
{
    //C������� ������ ��������� �������� ��������
    //For more information on how to configure your application, visit https://docs.microsoft.com/ru-ru/aspnet/core/fundamentals/startup?view=aspnetcore-5.0
    //�������� ������� ������ � ���������� ASP.NET Core
    public class Startup
    {
        //��������� ����� ���������� - �� ������������ �����
        //Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
        }

        //��������� ��������� ��������� ��������
        //Use this method to configure the HTTP request pipeline
        //  IApplicationBuilder - ������������ �������� ��� ������ Configure
        //  IWebHostEnvironment - �� ������������ ��������. ��������� �������� ���������� � �����
        //  � �������� ���������� ����� ���������� ����� ������, ������������������ � ������ ConfigureServices
        //����������� ���� ��� ��� �������� ������� ������ Startup
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //���� ����������� � ����������
            if (env.IsDevelopment())
            {
                //�������� ����������� �������� � ��������� ������
                app.UseDeveloperExceptionPage();
            }

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
            RequestDelegate delegateExample = context => context.Response.WriteAsync($"Not Found! x={x}");

            app.Run(delegateExample);
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
}
