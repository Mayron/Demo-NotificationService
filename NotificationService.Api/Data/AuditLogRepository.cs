using System.Collections.Generic;

namespace NotificationService.Api.Data
{
    public interface IAuditLogRepository
    {
        void Add(AuditLog auditLog);

        int Count();

        List<AuditLog> GetAll();

        void Clear();
    }

    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly List<AuditLog> _auditLogs = new();

        public void Add(AuditLog auditLog)
        {
            _auditLogs.Add(auditLog);
        }

        public int Count()
        {
            return _auditLogs.Count;
        }

        public List<AuditLog> GetAll()
        {
            return new List<AuditLog>(_auditLogs);
        }

        public void Clear()
        {
            _auditLogs.Clear();
        }
    }
}