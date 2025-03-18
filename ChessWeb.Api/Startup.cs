using ChessWeb.Api.Hubs;
using ChessWeb.Api.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace ChessWeb.Api
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration Configuration)
        {
            this.Configuration = Configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder
                            .WithOrigins(Configuration.GetSection("AllowedHosts").Get<string[]>())
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .WithExposedHeaders("Location");
                    });
            });

            services.AddSingleton<GameRoomsService>();
            services.AddSingleton<ConnectionToRoomService>();
            services.AddControllers();
            services.AddSignalR().AddMessagePackProtocol();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Chess API v1", Version = "v1" });
                options.AddSignalRSwaggerGen();
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCors();
            app.UseRouting();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Chess API v1");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<GameHub>("/gamehub");
            });
        }
    }
}
