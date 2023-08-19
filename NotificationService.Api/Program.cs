using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace NotificationService.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IHostBuilder hostBuilder = Host
                .CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(cfg => cfg.UseStartup<StartUp>())
                .UseDefaultServiceProvider(cfg =>
                {
                    cfg.ValidateOnBuild = true;
                    cfg.ValidateScopes = true;
                });

            hostBuilder.Build().Run();
        }
    }
}