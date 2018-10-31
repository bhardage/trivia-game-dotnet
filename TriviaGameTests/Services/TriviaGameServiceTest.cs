using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using TriviaGame.Models;
using TriviaGame.Services;

namespace TriviaGameTests.Services
{
    [TestClass]
    public class TriviaGameServiceTest
    {
        public TriviaGameService cut;

        private Mock<IScoreService> scoreService;

        [TestInitialize]
        public void Setup()
        {
            scoreService = new Mock<IScoreService>();
            cut = new TriviaGameService(scoreService.Object);
        }

        [TestMethod]
        public void TestGetScoresWithNoUsers()
        {
            string channelId = "channel";
            SlackRequestDoc requestDoc = new SlackRequestDoc
            {
                ChannelId = channelId
            };

            scoreService.Setup(x => x.GetAllScoresByUser(It.IsAny<string>())).Returns(new Dictionary<SlackUser, long>());

            SlackResponseDoc result = cut.GetScores(requestDoc);

            /*
             * ```Scores:
             *
             * No scores yet...```
             */
            Assert.AreEqual("```Scores:\n\nNo scores yet...```", result.Text);

            scoreService.Verify(x => x.GetAllScoresByUser(channelId));
        }

        [TestMethod]
        public void TestGetScoresFormatsAndSortsCorrectly()
        {
            Dictionary<SlackUser, long> scoresByUser = new Dictionary<SlackUser, long>
            {
                { new SlackUser("1234", "test4"), 1L },
                { new SlackUser("1235", "longertest2"), 103L },
                { new SlackUser("1236", "unmanageablylongertest3"), 12L },
                { new SlackUser("1237", "test1"), 1L }
            };
            string channelId = "channel";
            SlackRequestDoc requestDoc = new SlackRequestDoc
            {
                ChannelId = channelId
            };

            scoreService.Setup(x => x.GetAllScoresByUser(It.IsAny<string>())).Returns(scoresByUser);

            SlackResponseDoc result = cut.GetScores(requestDoc);

            /*
             * ```Scores:
             *
             * @longertest2:             103
             * @unmanageablylongertest3:  12
             * @test1:                     1
             * @test4:                     1```
             */
            Assert.AreEqual(
                "```Scores:\n\n@longertest2:             103\n@unmanageablylongertest3:  12\n@test1:                     1\n@test4:                     1```",
                result.Text
            );

            scoreService.Verify(x => x.GetAllScoresByUser(channelId));
        }

        [TestMethod]
        public void TestResetScores()
        {
            string channelId = "channel";
            SlackRequestDoc requestDoc = new SlackRequestDoc
            {
                ChannelId = channelId
            };

            scoreService.Setup(x => x.GetAllScoresByUser(It.IsAny<string>())).Returns(new Dictionary<SlackUser, long>());

            SlackResponseDoc result = cut.ResetScores(requestDoc);

            Assert.IsNotNull(result);
            Assert.AreEqual(SlackResponseType.IN_CHANNEL, result.ResponseType);
            Assert.AreEqual("Scores have been reset!", result.Text);

            Assert.IsNotNull(result.Attachments);
            Assert.IsTrue(result.Attachments.Count == 1);
            Assert.AreEqual("```Scores:\n\nNo scores yet...```", result.Attachments[0].Text);
            Assert.IsTrue(result.Attachments[0].MarkdownIn.Count == 1);
            Assert.AreEqual("text", result.Attachments[0].MarkdownIn[0]);

            scoreService.Verify(x => x.ResetScores(channelId));
            scoreService.Verify(x => x.GetAllScoresByUser(channelId));
        }
    }
}
