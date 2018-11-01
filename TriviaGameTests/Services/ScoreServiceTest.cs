using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using TriviaGame.Exceptions;
using TriviaGame.Models;
using TriviaGame.Repositories;
using TriviaGame.Services;

namespace TriviaGameTests.Services
{
    [TestClass]
    public class ScoreServiceTest
    {
        public ScoreService cut;

        private Mock<IScoreInfoRepository> scoreInfoRepository = new Mock<IScoreInfoRepository>();

        [TestInitialize]
        public void Setup()
        {
            scoreInfoRepository.Reset();
            cut = new ScoreService(scoreInfoRepository.Object);
        }

        #region GetAllScoresByUser
        [TestMethod]
        public void TestGetAllScoresByUserWithNoScores()
        {
            string channelId = "C12345";

            scoreInfoRepository.Setup(x => x.FindByChannelId(It.IsAny<string>())).Returns(new List<ScoreInfo>());

            Dictionary<SlackUser, long> result = cut.GetAllScoresByUser(channelId);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);

            scoreInfoRepository.Verify(x => x.FindByChannelId(channelId));
        }

        [TestMethod]
        public void TestGetAllScoresByUserWithScores()
        {
            string channelId = "C12345";
            List<ScoreInfo> scores = new List<ScoreInfo>
            {
                new ScoreInfo
                {
                    ChannelId = channelId,
                    UserId = "U12345",
                    Username = "test1",
                    Score = 95L
                },
                new ScoreInfo
                {
                    ChannelId = channelId,
                    UserId = "U12346",
                    Username = "test2",
                    Score = 0L
                },
                new ScoreInfo
                {
                    ChannelId = channelId,
                    UserId = "U12347",
                    Username = "test3",
                    Score = 32L
                }
            };

            scoreInfoRepository.Setup(x => x.FindByChannelId(It.IsAny<string>())).Returns(scores);

            Dictionary<SlackUser, long> result = cut.GetAllScoresByUser(channelId);

            Assert.IsNotNull(result);
            Assert.AreEqual(3, result.Count);

            var scoresByUserSorted = from pair in result
                                     orderby pair.Key.UserId ascending
                                     select pair;

            int i = 0;

            foreach (KeyValuePair<SlackUser, long> pair in scoresByUserSorted)
            {
                Assert.AreEqual(scores[i].UserId, pair.Key.UserId);
                Assert.AreEqual(scores[i].Username, pair.Key.Username);
                Assert.AreEqual(scores[i].Score, pair.Value);
                i++;
            }

            scoreInfoRepository.Verify(x => x.FindByChannelId(channelId));
        }
        #endregion

        #region CreateUserIfNotExists
        [TestMethod]
        public void TestCreateUserIfNotExistsWithExistingUser()
        {
            string channelId = "C12345";
            string userId = "U12345";
            SlackUser user = new SlackUser(userId, "test1");

            scoreInfoRepository.Setup(x => x.FindByChannelIdAndUserId(It.IsAny<string>(), It.IsAny<string>())).Returns(new ScoreInfo());

            bool result = cut.CreateUserIfNotExists(channelId, user);

            Assert.IsFalse(result);

            scoreInfoRepository.Verify(x => x.FindByChannelIdAndUserId(channelId, userId));
            scoreInfoRepository.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void TestCreateUserIfNotExistsWithNoExistingUser()
        {
            string channelId = "C12345";
            string userId = "U12345";
            string username = "test1";
            SlackUser user = new SlackUser(userId, username);

            ScoreInfo capturedScoreInfo = null;
            scoreInfoRepository.Setup(x => x.FindByChannelIdAndUserId(It.IsAny<string>(), It.IsAny<string>())).Returns((ScoreInfo)null);
            scoreInfoRepository.Setup(x => x.Save(It.IsAny<ScoreInfo>())).Callback<ScoreInfo>(si => capturedScoreInfo = si);

            bool result = cut.CreateUserIfNotExists(channelId, user);

            Assert.IsTrue(result);

            scoreInfoRepository.Verify(x => x.FindByChannelIdAndUserId(channelId, userId));
            scoreInfoRepository.Verify(x => x.Save(It.IsAny<ScoreInfo>()));

            Assert.IsNotNull(capturedScoreInfo);
            Assert.AreEqual(channelId, capturedScoreInfo.ChannelId);
            Assert.AreEqual(userId, capturedScoreInfo.UserId);
            Assert.AreEqual(username, capturedScoreInfo.Username);
            Assert.AreEqual(0L, capturedScoreInfo.Score);
        }
        #endregion

        #region DoesUserExist
        [TestMethod]
        public void TestDoesUserExistWithExistingUser()
        {
            string channelId = "C12345";
            string userId = "U12345";

            scoreInfoRepository.Setup(x => x.FindByChannelIdAndUserId(It.IsAny<string>(), It.IsAny<string>())).Returns(new ScoreInfo());

            bool result = cut.DoesUserExist(channelId, userId);

            Assert.IsTrue(result);

            scoreInfoRepository.Verify(x => x.FindByChannelIdAndUserId(channelId, userId));
        }

        [TestMethod]
        public void TestDoesUserExistWithNoExistingUser()
        {
            string channelId = "C12345";
            string userId = "U12345";

            scoreInfoRepository.Setup(x => x.FindByChannelIdAndUserId(It.IsAny<string>(), It.IsAny<string>())).Returns((ScoreInfo)null);

            bool result = cut.DoesUserExist(channelId, userId);

            Assert.IsFalse(result);

            scoreInfoRepository.Verify(x => x.FindByChannelIdAndUserId(channelId, userId));
        }
        #endregion

        #region IncrementScore
        [TestMethod]
        public void TestIncrementScoreForInvalidUser()
        {
            string channelId = "C12345";
            string userId = "U12345";

            scoreInfoRepository.Setup(x => x.FindByChannelIdAndUserId(It.IsAny<string>(), It.IsAny<string>())).Returns((ScoreInfo)null);

            Exception exception = null;

            try
            {
                cut.IncrementScore(channelId, userId);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(ScoreException));

            scoreInfoRepository.Verify(x => x.FindByChannelIdAndUserId(channelId, userId));
        }

        [TestMethod]
        public void TestIncrementScoreForValidUser()
        {
            string channelId = "C12345";
            string userId = "U12345";
            string username = "test1";

            ScoreInfo scoreInfo = new ScoreInfo
            {
                ChannelId = channelId,
                UserId = userId,
                Username = username,
                Score = 31
            };

            ScoreInfo capturedScoreInfo = null;
            scoreInfoRepository.Setup(x => x.FindByChannelIdAndUserId(It.IsAny<string>(), It.IsAny<string>())).Returns(scoreInfo);
            scoreInfoRepository.Setup(x => x.Save(It.IsAny<ScoreInfo>())).Callback<ScoreInfo>(si => capturedScoreInfo = si);

            Exception exception = null;

            try
            {
                cut.IncrementScore(channelId, userId);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNull(exception);

            scoreInfoRepository.Verify(x => x.FindByChannelIdAndUserId(channelId, userId));
            scoreInfoRepository.Verify(x => x.Save(It.IsAny<ScoreInfo>()));

            Assert.IsNotNull(capturedScoreInfo);
            Assert.AreEqual(channelId, capturedScoreInfo.ChannelId);
            Assert.AreEqual(userId, capturedScoreInfo.UserId);
            Assert.AreEqual(username, capturedScoreInfo.Username);
            Assert.AreEqual(32, capturedScoreInfo.Score);
        }
        #endregion

        #region ResetScores
        [TestMethod]
        public void TestResetScores()
        {
            string channelId = "C12345";

            cut.ResetScores(channelId);

            scoreInfoRepository.Verify(x => x.DeleteByChannelId(channelId));
        }
        #endregion
    }
}
