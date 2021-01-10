using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using System;
using WebAPIApp.Auth;

namespace WebAPIApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IUserStore, UsersStore>();
            services.AddSingleton<AuthService>();

            services.AddControllers();

            var cont = new OpenApiContact {Url = new Uri("https://localhost:44313/#contact"), Email = "asd@asd.asd", Name = "Developer"};
            var licence = new OpenApiLicense { Name = "APACHE", Url = new Uri("https://localhost:44313/#license")};
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebAPIApp", Version = "v1", Contact = cont, Description = "Test Web API project", TermsOfService = new Uri("https://localhost:44313/#terms"), License = licence});
            });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opts => {
                    //opts.RequireHttpsMetadata = false;
                    opts.TokenValidationParameters = new TokenValidationParameters {
                        ValidateIssuer = true,
                        ValidIssuer = AuthOptions.ISSUER,

                        ValidateAudience = true,
                        ValidAudience = AuthOptions.AUDIENCE,

                        ValidateLifetime = true,
                        
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = AuthOptions.GetKey()
                    };
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger(setup =>
                {
                    setup.RouteTemplate = "docs/{documentName}/swagger.json";
                });
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/docs/v1/swagger.json", "WebAPIApp v1");
                    c.RoutePrefix = "docs";
                });
            }

            app.UseHttpsRedirection();

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
