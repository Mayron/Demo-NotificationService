using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;
using NotificationService.Api;

namespace NotificationService.IntegrationTests
{
    /// <summary>
    /// To be used as a fixture for integration tests.
    /// </summary>
    [UsedImplicitly]
    public class AppInstance : WebApplicationFactory<StartUp>
    {
        public HttpClient CreateClient(Uri address)
        {
            HttpClient client = CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                BaseAddress = address,
                HandleCookies = true
            });

            client.DefaultRequestHeaders.Referrer = address;
            client.DefaultRequestHeaders.Host = address.Host;

            return client;
        }

        protected override IHostBuilder CreateHostBuilder() => Host
            .CreateDefaultBuilder()
            .UseEnvironment("IntegrationTesting")
            .ConfigureWebHostDefaults(cfg => cfg.UseStartup<StartUp>())
            .UseDefaultServiceProvider(cfg =>
            {
                cfg.ValidateOnBuild = true;
                cfg.ValidateScopes = true;
            });
    }
}