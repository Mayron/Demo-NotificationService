using System;

namespace NotificationService.Api.Data
{
    public class AuditLog
    {
        public Guid Id { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool Internal { get; set; }
        public Guid? AccountId { get; set; }
        public Guid? UserId { get; set; }
    }
}