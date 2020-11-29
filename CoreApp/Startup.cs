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

            //��������� ���������
            app.UseEndpoints(endpoints =>
            {
                //��������� Get ������� �� ������ "/"
                endpoints.MapGet("/", async context =>
                {
                    //�������� ������ ����� � ��������
                    await context.Response.WriteAsync("Hello World!");
                });
            });
        }
    }
}
