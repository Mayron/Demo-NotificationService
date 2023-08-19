using System.Collections.Generic;

namespace NotificationService.Api.Data
{
    public interface IUserRepository
    {
        void Add(User user);

        int Count();

        void Clear();
    }

    public class UserRepository : IUserRepository
    {
        private readonly List<User> _users = new();

        public void Add(User user)
        {
            _users.Add(user);
        }

        public int Count()
        {
            return _users.Count;
        }

        public void Clear()
        {
            _users.Clear();
        }
    }
}