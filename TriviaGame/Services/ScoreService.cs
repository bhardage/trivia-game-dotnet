using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TriviaGame.Exceptions;
using TriviaGame.Models;
using TriviaGame.Repositories;

namespace TriviaGame.Services
{
    public class ScoreService : IScoreService
    {
        private readonly IScoreInfoRepository _scoreInfoRepository;

        public ScoreService(IScoreInfoRepository scoreInfoRepository)
        {
            _scoreInfoRepository = scoreInfoRepository;
        }

        public Dictionary<SlackUser, long> GetAllScoresByUser(string channelId)
        {
            List<ScoreInfo> scores = _scoreInfoRepository.FindByChannelId(channelId);

            return scores.ToDictionary(
                e => new SlackUser(e.UserId, e.Username),
                e => e.Score
            );
        }

        public bool CreateUserIfNotExists(string channelId, SlackUser user)
        {
            ScoreInfo scoreInfo = _scoreInfoRepository.FindByChannelIdAndUserId(channelId, user.UserId);

            if (scoreInfo == null)
            {
                scoreInfo = new ScoreInfo
                {
                    ChannelId = channelId,
                    UserId = user.UserId,
                    Username = user.Username,
                    Score = 0L
                };
                _scoreInfoRepository.Save(scoreInfo);

                return true;
            }

            return false;
        }

        public bool DoesUserExist(string channelId, string userId)
        {
            ScoreInfo scoreInfo = _scoreInfoRepository.FindByChannelIdAndUserId(channelId, userId);
            return scoreInfo != null;
        }

        public void IncrementScore(string channelId, string userId)
        {
            ScoreInfo scoreInfo = _scoreInfoRepository.FindByChannelIdAndUserId(channelId, userId);

            if (scoreInfo == null)
            {
                throw new ScoreException();
            }

            scoreInfo.Score += 1;
            _scoreInfoRepository.Save(scoreInfo);
        }

        public void ResetScores(string channelId)
        {
            _scoreInfoRepository.DeleteByChannelId(channelId);
        }
    }
}
