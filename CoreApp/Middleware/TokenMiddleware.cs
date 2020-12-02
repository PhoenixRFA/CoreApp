using System;
using System.Threading.Tasks;
using CoreApp.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace CoreApp.Middleware
{
    //Example of simple middleware
    public class TokenMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _token;

        //Класс middleware должен иметь конструктор, который принимает параметр типа RequestDelegate
        //Через этот параметр передается ссылка на следующий делегат запроса (который стоит в конвейере обработки запроса)
        //Через конструктор также можно передать параметры в middleware (string token)
        public TokenMiddleware(RequestDelegate next, string token)
        {
            if (string.IsNullOrEmpty(token)) throw new ArgumentNullException(nameof(token));

            _next = next;
            _token = token;
        }

        //В классе должен быть определен метод, который должен называться Invoke, либо InvokeAsync
        //Этот метод должен возвращать Task и принимать HttpContext (контекст запроса)
        //Данный метод собственно и будет обрабатывать запрос
        public async Task InvokeAsync(HttpContext context, IMessageSender sender)
        {
            StringValues token = context.Request.Query["token"];

            if (token != _token)
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync($"Token is invalid\n{sender.Send()}");
                return;
            }

            await _next.Invoke(context);
        }
    }

    public static class TokenMiddlewareExtension
    {
        public static IApplicationBuilder UseToken(this IApplicationBuilder builder, string token) => builder.UseMiddleware<TokenMiddleware>(token);
    }
}
