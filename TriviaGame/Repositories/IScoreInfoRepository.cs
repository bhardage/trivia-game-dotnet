using System.Collections.Generic;
using TriviaGame.Models;

namespace TriviaGame.Repositories
{
    public interface IScoreInfoRepository
    {
        List<ScoreInfo> FindByChannelId(string channelId);
        ScoreInfo FindByChannelIdAndUserId(string channelId, string userId);
        ScoreInfo Save(ScoreInfo scoreInfo);
        void DeleteByChannelId(string channelId);
    }
}
