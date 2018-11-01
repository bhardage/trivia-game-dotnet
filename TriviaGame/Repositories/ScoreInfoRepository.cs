using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using TriviaGame.Models;

namespace TriviaGame.Repositories
{
    public class ScoreInfoRepository : IScoreInfoRepository
    {
        private readonly ITriviaGameDbContext _triviaGameDbContext;

        public ScoreInfoRepository(ITriviaGameDbContext triviaGameDbContext)
        {
            _triviaGameDbContext = triviaGameDbContext;
        }

        public List<ScoreInfo> FindByChannelId(string channelId)
        {
            FilterDefinition<ScoreInfo> filter = Builders<ScoreInfo>.Filter
                .Eq(m => m.ChannelId, channelId);

            return getScores()
                .Find(filter)
                .ToList();
        }

        public ScoreInfo FindByChannelIdAndUserId(string channelId, string userId)
        {
            FilterDefinition<ScoreInfo> filter = Builders<ScoreInfo>.Filter.And(
                Builders<ScoreInfo>.Filter.Eq(m => m.ChannelId, channelId),
                Builders<ScoreInfo>.Filter.Eq(m => m.UserId, userId)
            );

            return getScores()
                .Find(filter)
                .First();
        }

        public ScoreInfo Save(ScoreInfo scoreInfo)
        {
            if (scoreInfo.Id == null)
            {
                getScores()
                    .InsertOne(scoreInfo);
            }
            else
            {
                getScores()
                    .ReplaceOne(s => s.Id == scoreInfo.Id, scoreInfo);

                //TODO throw an exception if nothing was updated?
            }

            return scoreInfo;
        }

        public void DeleteByChannelId(string channelId)
        {
            FilterDefinition<ScoreInfo> filter = Builders<ScoreInfo>.Filter
                .Eq(m => m.ChannelId, channelId);

            getScores()
                .DeleteMany(filter);
        }

        private IMongoCollection<ScoreInfo> getScores()
        {
            return _triviaGameDbContext.GetDb().GetCollection<ScoreInfo>("scoreInfo");
        }
    }
}
