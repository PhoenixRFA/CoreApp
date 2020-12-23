using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CoreApp.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CoreApp
{
    public class RoutingStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHealthChecks();
        }


        public void Configure(IApplicationBuilder app, LinkGenerator linkGenerator, ILogger<RoutingStartup> logger)
        {
            app.UseStopwatch("Timer1");


            app.Use(next => context =>
            {
                //Endpoint всегда будет null
                Debug.WriteLine("Endpoint before UseRouting: " + context.GetEndpoint()?.DisplayName);
                return next(context);
            });

            //включаем возможность маршрутизации, например - использование метода UseEndpoints
            app.UseRouting();

            app.UseStopwatch("Timer2");
            
            app.Use(next => context =>
            {
                //Endpoint всегда НЕ будет null, если один из маршрутов подойдет
                Debug.WriteLine("Endpoint after UseRouting: " + context.GetEndpoint()?.DisplayName);
                return next(context);
            });


            //Endpoints
            app.Use(async (context, next) =>
            {
                Endpoint endpoint = context.GetEndpoint();

                if (endpoint is null)
                {
                    Debug.WriteLine($"Path: {context.Request.Path}  Endpoint: null");

                    await context.Response.WriteAsync("Endpoint is not defined!");

                    return;
                }
                
                Debug.WriteLine($"Endpoint name: {endpoint.DisplayName}");

                //RouteEndpoint представляет дополнительные данные по маршруту (RoutePattern и Order)
                if (endpoint is RouteEndpoint routeEndpoint)
                {
                    Debug.WriteLine($"Route pattern: {routeEndpoint.RoutePattern.RawText} ({routeEndpoint.Order})");
                }

                foreach (object metadata in endpoint.Metadata)
                {
                    Debug.WriteLine($"Metadata: {metadata}");
                }

                //Проверка метаданных в middleware
                var auditPolicy = endpoint.Metadata.GetMetadata<AuditPolicyAttribute>();
                if (auditPolicy?.NeedAudit == true)
                {
                    Debug.WriteLine($"Accessing sensitive data {DateTime.Now:g}");
                }
                
                await next();
            });


            app.UseStopwatch("Timer3");


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health");

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Root home");
                });

                endpoints.MapGet("/host", async context =>
                {
                    await context.Response.WriteAsync("Localhost");
                }).RequireHost("*.localhost:44334");//привязка к хосту

                //установка метаданных на endpoint
                endpoints.MapGet("/sensitive", async context =>
                {
                    await context.Response.WriteAsync("some sensitive data");
                }).WithMetadata(new AuditPolicyAttribute(true));

                endpoints.MapGet("/{controller}/{action}", async context =>
                {
                    string controller = context.Request.RouteValues["controller"].ToString();
                    string action = context.Request.RouteValues["action"].ToString();
                    
                    await context.Response.WriteAsync($"Controller: {controller}  Action: {action}");
                });
            });
        }
    }

    //Пример реализации своих метаданных (на основе аттрибута, т.к. их удобнее использовать)
    public class AuditPolicyAttribute : Attribute
    {
        public bool NeedAudit { get; set; }

        public AuditPolicyAttribute(bool needsAudit)
        {
            NeedAudit = needsAudit;
        }
    }
}
