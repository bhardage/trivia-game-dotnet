using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TriviaGame.Models;

namespace TriviaGame.Services
{
    public interface IScoreService
    {
        Dictionary<SlackUser, long> GetAllScoresByUser(string channelId);
        bool CreateUserIfNotExists(string channelId, SlackUser user);
        bool DoesUserExist(string channelId, string userId);
        void IncrementScore(string channelId, string userId);
        void ResetScores(string channelId);
    }
}
