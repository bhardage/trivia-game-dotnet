using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TriviaGame.Models
{
    public class SlackUser
    {
        private readonly string userId;
        private readonly String username;

        public SlackUser(string userId, string username)
        {
            this.userId = userId;
            this.username = username;
        }

        public string UserId { get { return userId; } }
        public string Username { get { return username; } }

        public override bool Equals(object obj)
        {
            var user = obj as SlackUser;
            return user != null &&
                   userId == user.userId &&
                   username == user.username;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(userId, username);
        }
    }
}
