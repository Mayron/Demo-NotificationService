using Microsoft.Extensions.DependencyInjection;
using NotificationService.Api.Data;
using System.Net;
using Xunit;

namespace NotificationService.IntegrationTests
{
    public class UsersApiIntegrationTests : BaseIntegrationTester
    {
        public UsersApiIntegrationTests(AppInstance instance) : base(instance)
        {
        }

        [Fact(DisplayName =
            "Create action adds new user with provided user name " +
            "to user repository and returns accepted status code.")]
        public async Task TestCreate_NewUserAdded()
        {
            // Arrange
            var usersRepository = Services.GetRequiredService<IUserRepository>();
            usersRepository.Clear();
            Assert.Equal(0, usersRepository.Count());

            var data = new List<KeyValuePair<string, string>>
            {
                new("userName", "Mike")
            };

            var formContent = new FormUrlEncodedContent(data);

            // Act
            HttpResponseMessage response = await Client.PostAsync("/users/Create", formContent);

            // Assert
            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

            Assert.Equal(1, usersRepository.Count()); // BUG: This is 2!
        }

        [Fact(DisplayName =
            "Create action adds 1 account audit log, 1 user audit log, and 1 base internal " +
            "audit log to AuditLog repository and returns accepted status code.")]
        public async Task TestCreate_AuditsCollected()
        {
            // Arrange
            var auditLogRepository = Services.GetRequiredService<IAuditLogRepository>();
            auditLogRepository.Clear();
            Assert.Equal(0, auditLogRepository.Count());

            var data = new List<KeyValuePair<string, string>>
            {
                new("userName", "Mike")
            };

            var formContent = new FormUrlEncodedContent(data);

            // Act
            HttpResponseMessage response = await Client.PostAsync("/users/Create", formContent);

            // Assert
            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);

            Assert.Equal(3, auditLogRepository.Count());

            var auditLogs = auditLogRepository.GetAll();

            Assert.Equal(1, auditLogs.Count(a => a.AccountId.HasValue));
            Assert.Equal(1, auditLogs.Count(a => a.UserId.HasValue));
            Assert.Equal(1, auditLogs.Count(a => !a.AccountId.HasValue && !a.UserId.HasValue && a.Internal));
        }
    }
}