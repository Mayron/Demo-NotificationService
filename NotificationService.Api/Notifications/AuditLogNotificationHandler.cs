using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Logging;
using NotificationService.Api.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NotificationService.Api.Notifications
{
    /// <summary>
    /// Published when an action occurred that should be stored in the audit log.
    /// </summary>
    public record AuditLogNotification(string Message, bool IsInternal) : INotification;

    public record UserAuditLogNotification(string Message, Guid UserId) : AuditLogNotification(Message, false);

    public record AccountAuditLogNotification(string Message, Guid AccountId) : AuditLogNotification(Message, false);

    [UsedImplicitly]
    public class AuditLogNotificationHandler<T> : INotificationHandler<T> where T : AuditLogNotification
    {
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly ILogger<AuditLogNotificationHandler<T>> _logger;

        public AuditLogNotificationHandler(
            IAuditLogRepository auditLogRepository,
            ILogger<AuditLogNotificationHandler<T>> logger)
        {
            _auditLogRepository = auditLogRepository;
            _logger = logger;
        }

        public async Task Handle(T notification, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(notification.Message))
            {
                _logger.LogDebug("Skipping audit log because message body is an empty string.");
                return;
            }

            AuditLog auditLog = await CreateAuditLogAsync(notification);

            await SendAuditLogAsync(notification, auditLog);

            // TODO: More Shared logic for handling AuditLogNotification

            _auditLogRepository.Add(auditLog);
        }

        private Task SendAuditLogAsync(T notification, AuditLog auditLog)
        {
            if (notification is AccountAuditLogNotification accountLog)
            {
                auditLog.AccountId = accountLog.AccountId;

                _logger.LogDebug("Sending Account (ID: {AccountId}) Audit: {Message}",
                    accountLog.AccountId, accountLog.Message);
                // TODO - Handle sending logic for AccountAuditLogNotification

                return Task.CompletedTask;
            }

            if (notification is UserAuditLogNotification userLog)
            {
                auditLog.UserId = userLog.UserId;

                _logger.LogDebug("Sending User (ID: {UserId}) Audit: {Message}",
                    userLog.UserId, userLog.Message);
                // TODO - Handle sending logic for UserAuditLogNotification

                return Task.CompletedTask;
            }

            var auditType = notification.IsInternal ? "Internal" : "Public";
            _logger.LogDebug("Sending {AuditType} Audit: {Message}", auditType, notification.Message);

            // TODO - Handle sending logic for AuditLogNotification

            return Task.CompletedTask;
        }

        private static Task<AuditLog> CreateAuditLogAsync(AuditLogNotification notification)
        {
            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid(),
                TimeStamp = DateTime.UtcNow,
                Internal = notification.IsInternal,
                Message = notification.Message,
            };

            // TODO: A large bunch of code for handling an AuditLogNotification
            // TODO: (ignoring the actual instance's implementation type)

            return Task.FromResult(auditLog);
        }
    }
}