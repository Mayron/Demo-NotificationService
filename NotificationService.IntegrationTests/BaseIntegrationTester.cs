using Xunit;

namespace NotificationService.IntegrationTests
{
    [CollectionDefinition(nameof(AppInstanceFixtureCollection), DisableParallelization = true)]
    public class AppInstanceFixtureCollection : ICollectionFixture<AppInstance>
    {
    }

    [Collection(nameof(AppInstanceFixtureCollection))]
    public abstract class BaseIntegrationTester
    {
        protected HttpClient Client { get; }
        protected IServiceProvider Services { get; }
        private const string BaseUri = "https://integration-testing.com";

        protected BaseIntegrationTester(AppInstance instance)
        {
            Client = instance.CreateClient(new Uri(BaseUri));
            Services = instance.Services;
        }
    }
}