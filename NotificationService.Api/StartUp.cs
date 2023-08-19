using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Api.Data;

namespace NotificationService.Api
{
    public class StartUp
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<StartUp>());

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