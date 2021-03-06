﻿using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;

namespace CoreApp.Middleware
{
    public class PerformanceMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Stopwatch _stopwatch;

        public PerformanceMiddleware(RequestDelegate next)
        {
            _next = next;
            _stopwatch = new Stopwatch();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            _stopwatch.Reset();
            _stopwatch.Start();

            //Пример из SO https://stackoverflow.com/questions/37395227/add-response-headers-to-asp-net-core-middleware
            context.Response.OnStarting(state =>
            {
                var httpContext = (HttpContext)state;

                _stopwatch.Stop();

                httpContext.Response.Headers.Add("X-RESPONSE-PERFORMANCE-TICKS", _stopwatch.ElapsedTicks.ToString());
                httpContext.Response.Headers.Add("X-RESPONSE-PERFORMANCE-MILIS", _stopwatch.ElapsedMilliseconds.ToString());

                return Task.CompletedTask;
            }, context);

            //Нельзя менять загловки ответа, после того, как ответ начал отправляться
            //по этому используется спец. метод, который вызывает делегат непосредственно перез началом отправки ответа
            //!В таком виде при выбрасывании исключения выкидывало сюда, а не в место где исключение произошло
            //context.Response.OnStarting(() =>
            //{
            //    _stopwatch.Stop();

            //    context.Response.Headers.Add("X-RESPONSE-PERFORMANCE-TICKS", _stopwatch.ElapsedTicks.ToString());
            //    context.Response.Headers.Add("X-RESPONSE-PERFORMANCE-MILIS", _stopwatch.ElapsedMilliseconds.ToString());

            //    return Task.CompletedTask;
            //});

            //Позволяет узнать - начата ли отправка ответа
            //context.Response.HasStarted

            await _next.Invoke(context);
        }
    }

    public static class PerformanceMiddlewareExtension
    {
        public static IApplicationBuilder UsePerformanceTimer(this IApplicationBuilder builder) =>
            builder.UseMiddleware<PerformanceMiddleware>();
    }
}
