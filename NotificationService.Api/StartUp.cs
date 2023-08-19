using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Api.Data;
using System.Reflection;

namespace NotificationService.Api
{
    public class StartUp
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            Assembly thisAssembly = typeof(StartUp).Assembly;
            services.AddMediatR(thisAssembly);

            services.AddSingleton<IUserRepository, UserRepository>();
            services.AddSingleton<IAuditLogRepository, AuditLogRepository>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}