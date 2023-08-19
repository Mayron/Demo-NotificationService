using System;

namespace NotificationService.Api.Data
{
    public class User
    {
        public Guid Id { get; }
        public string Name { get; }

        public User(Guid userId, string userName)
        {
            Id = userId;
            Name = userName;
        }
    }
}