using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Logging;
using NotificationService.Api.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NotificationService.Api.Notifications
{
    public record NewUserNotification(User NewUser, Guid AccountId) : INotification;

    [UsedImplicitly]
    public class NewUserNotificationHandler : INotificationHandler<NewUserNotification>
    {
        private readonly IUserRepository _users;
        private readonly ILogger<NewUserNotificationHandler> _logger;

        public NewUserNotificationHandler(IUserRepository users, ILogger<NewUserNotificationHandler> logger)
        {
            _users = users;
            _logger = logger;
        }

        public Task Handle(NewUserNotification notification, CancellationToken cancellationToken)
        {
            User newUser = notification.NewUser;
            _users.Add(newUser);

            _logger.LogInformation(
                "New user {UserName} created with ID {UserId}, and joined account {AccountId}",
                newUser.Name, newUser.Id, notification.AccountId);

            return Task.CompletedTask;
        }
    }
}