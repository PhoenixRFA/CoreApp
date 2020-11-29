using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace CoreApp
{
    //Cодержит логику обработки входящих запросов
    //For more information on how to configure your application, visit https://docs.microsoft.com/ru-ru/aspnet/core/fundamentals/startup?view=aspnetcore-5.0
    //Является входной точкой в приложение ASP.NET Core
    public class Startup
    {
        //Настройка служб приложения - НЕ обязательный метод
        //Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
        }

        //Настройка конвейера обработки запросов
        //Use this method to configure the HTTP request pipeline
        //  IApplicationBuilder - обязательный параметр для метода Configure
        //  IWebHostEnvironment - НЕ обязательный параметр. Позволяет получить информацию о среде
        //  в качестве параметров можно передавать любой сервис, зарегистрированный в методе ConfigureServices
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //если проиложение в разработке
            if (env.IsDevelopment())
            {
                //выводить специальную страницу с описанием ошибки
                app.UseDeveloperExceptionPage();
            }

            //включаем возможность маршрутизации, например - использование метода UseEndpoints
            app.UseRouting();

            //установка маршрутов
            app.UseEndpoints(endpoints =>
            {
                //обработка Get запроса по адресу "/"
                endpoints.MapGet("/", async context =>
                {
                    //отправка ответа прямо в контекст
                    await context.Response.WriteAsync("Hello World!");
                });
            });
        }
    }
}
