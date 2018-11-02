using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using TriviaGame.Exceptions;
using TriviaGame.Models;
using TriviaGame.Services;

namespace TriviaGameTests.Services
{
    [TestClass]
    public class TriviaGameServiceTest
    {
        public TriviaGameService cut;

        private Mock<IScoreService> scoreService = new Mock<IScoreService>();

        private Mock<IWorkflowService> workflowService = new Mock<IWorkflowService>();

        private Mock<IDelayedSlackService> delayedSlackService = new Mock<IDelayedSlackService>();

        [TestInitialize]
        public void Setup()
        {
            scoreService.Reset();
            workflowService.Reset();
            delayedSlackService.Reset();
            cut = new TriviaGameService(scoreService.Object, workflowService.Object, delayedSlackService.Object);
        }

        #region Start
        [TestMethod]
        public void TestStartWithGameNotStarted()
        {
            string channelId = "channel";
            string userId = "U12345";
            string command = "/command";
            string topic = "some topic";

            SlackRequestDoc requestDoc = new SlackRequestDoc
            {
                ChannelId = channelId,
                UserId = userId,
                Command = command
            };

            workflowService.Setup(x => x.OnGameStarted(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Throws(new GameNotStartedException());

            SlackResponseDoc responseDoc = cut.Start(requestDoc, topic);

            Assert.IsNotNull(responseDoc);
            Assert.AreEqual(SlackResponseType.EPHEMERAL, responseDoc.ResponseType);
            Assert.AreEqual("A game has not yet been started. If you'd like to start a game, try `" + command + " start`", responseDoc.Text);

            workflowService.Verify(x => x.OnGameStarted(channelId, userId, topic));
        }

        [TestMethod]
        public void TestStartWithWorkflowException()
        {
            string channelId = "channel";
            string userId = "U12345";
            string command = "/command";
            string topic = "some topic";
            string exceptionMessage = "some message";

            SlackRequestDoc requestDoc = new SlackRequestDoc
            {
                ChannelId = channelId,
                UserId = userId,
                Command = command
            };

            workflowService.Setup(x => x.OnGameStarted(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Throws(new WorkflowException(exceptionMessage));

            SlackResponseDoc responseDoc = cut.Start(requestDoc, topic);

            Assert.IsNotNull(responseDoc);
            Assert.AreEqual(SlackResponseType.EPHEMERAL, responseDoc.ResponseType);
            Assert.AreEqual(exceptionMessage, responseDoc.Text);

            workflowService.Verify(x => x.OnGameStarted(channelId, userId, topic));
        }

        [TestMethod]
        public void TestSuccessfulStart()
        {
            string channelId = "channel";
            string userId = "U12345";
            string command = "/command";
            string topic = "some topic";

            SlackRequestDoc requestDoc = new SlackRequestDoc
            {
                ChannelId = channelId,
                UserId = userId,
                Command = command
            };

            SlackResponseDoc responseDoc = cut.Start(requestDoc, topic);

            Assert.IsNotNull(responseDoc);
            Assert.AreEqual(SlackResponseType.IN_CHANNEL, responseDoc.ResponseType);
            Assert.AreEqual("OK, <@" + userId + ">, please ask a question.", responseDoc.Text);

            workflowService.Verify(x => x.OnGameStarted(channelId, userId, topic));
        }
        #endregion

        #region Stop
        [TestMethod]
        public void TestStopWithGameNotStarted()
        {
            string channelId = "channel";
            string userId = "U12345";
            string command = "/command";

            SlackRequestDoc requestDoc = new SlackRequestDoc
            {
                ChannelId = channelId,
                UserId = userId,
                Command = command
            };

            workflowService.Setup(x => x.OnGameStopped(It.IsAny<string>(), It.IsAny<string>())).Throws(new GameNotStartedException());

            SlackResponseDoc responseDoc = cut.Stop(requestDoc);

            Assert.IsNotNull(responseDoc);
            Assert.AreEqual(SlackResponseType.EPHEMERAL, responseDoc.ResponseType);
            Assert.AreEqual("A game has not yet been started. If you'd like to start a game, try `" + command + " start`", responseDoc.Text);

            workflowService.Verify(x => x.OnGameStopped(channelId, userId));
        }

        [TestMethod]
        public void TestStopWithWorkflowException()
        {
            string channelId = "channel";
            string userId = "U12345";
            string command = "/command";
            string exceptionMessage = "some message";

            SlackRequestDoc requestDoc = new SlackRequestDoc
            {
                ChannelId = channelId,
                UserId = userId,
                Command = command
            };

            workflowService.Setup(x => x.OnGameStopped(It.IsAny<string>(), It.IsAny<string>())).Throws(new WorkflowException(exceptionMessage));

            SlackResponseDoc responseDoc = cut.Stop(requestDoc);

            Assert.IsNotNull(responseDoc);
            Assert.AreEqual(SlackResponseType.EPHEMERAL, responseDoc.ResponseType);
            Assert.AreEqual(exceptionMessage, responseDoc.Text);

            workflowService.Verify(x => x.OnGameStopped(channelId, userId));
        }

        [TestMethod]
        public void TestSuccessfulStop()
        {
            string channelId = "channel";
            string userId = "U12345";
            string command = "/command";

            SlackRequestDoc requestDoc = new SlackRequestDoc
            {
                ChannelId = channelId,
                UserId = userId,
                Command = command
            };

            SlackResponseDoc responseDoc = cut.Stop(requestDoc);

            Assert.IsNotNull(responseDoc);
            Assert.AreEqual(SlackResponseType.IN_CHANNEL, responseDoc.ResponseType);
            Assert.AreEqual("The game has been stopped but scores have not been cleared. If you'd like to start a new game, try `" + command + " start`.", responseDoc.Text);

            workflowService.Verify(x => x.OnGameStopped(channelId, userId));
        }
        #endregion

        #region Join
        [TestMethod]
        public void TestJoinWithExistingUser()
        {
            string channelId = "channel";
            string userId = "U12345";
            string username = "test1";

            SlackRequestDoc requestDoc = new SlackRequestDoc
            {
                ChannelId = channelId,
                UserId = userId,
                Username = username
            };

            scoreService.Setup(x => x.CreateUserIfNotExists(It.IsAny<string>(), It.IsAny<SlackUser>())).Returns(false);

            SlackResponseDoc responseDoc = cut.Join(requestDoc);

            Assert.IsNotNull(responseDoc);
            Assert.AreEqual(SlackResponseType.EPHEMERAL, responseDoc.ResponseType);
            Assert.AreEqual("You're already in the game.", responseDoc.Text);

            scoreService.Verify(x => x.CreateUserIfNotExists(channelId, new SlackUser(userId, username)));
            delayedSlackService.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void TestJoinWithNewUser()
        {
            string channelId = "channel";
            string userId = "U12345";
            string username = "test1";
            string responseUrl = "some url";

            SlackRequestDoc requestDoc = new SlackRequestDoc
            {
                ChannelId = channelId,
                UserId = userId,
                Username = username,
                ResponseUrl = responseUrl
            };

            string capturedResponseUrl = null;
            SlackResponseDoc capturedResponseDoc = null;
            delayedSlackService.Setup(x => x.sendResponse(It.IsAny<string>(), It.IsAny<SlackResponseDoc>()))
                .Callback<string, SlackResponseDoc>((ru, rd) =>
                {
                    capturedResponseUrl = ru;
                    capturedResponseDoc = rd;
                });

            scoreService.Setup(x => x.CreateUserIfNotExists(It.IsAny<string>(), It.IsAny<SlackUser>())).Returns(true);

            SlackResponseDoc responseDoc = cut.Join(requestDoc);

            Assert.IsNotNull(responseDoc);
            Assert.AreEqual(SlackResponseType.EPHEMERAL, responseDoc.ResponseType);
            Assert.AreEqual("Joining game.", responseDoc.Text);

            scoreService.Verify(x => x.CreateUserIfNotExists(channelId, new SlackUser(userId, username)));
            delayedSlackService.Verify(x => x.sendResponse(It.IsAny<string>(), It.IsAny<SlackResponseDoc>()));

            Assert.AreEqual(responseUrl, capturedResponseUrl);
            Assert.IsNotNull(capturedResponseDoc);
            Assert.AreEqual(SlackResponseType.IN_CHANNEL, capturedResponseDoc.ResponseType);
            Assert.AreEqual("<@" + userId + "> has joined the game!", capturedResponseDoc.Text);
        }
        /*
        public SlackResponseDoc Join(SlackRequestDoc requestDoc)
        {
            SlackUser user = new SlackUser(requestDoc.UserId, requestDoc.Username);
            bool userCreated = _scoreService.CreateUserIfNotExists(requestDoc.ChannelId, user);

            SlackResponseDoc responseDoc = new SlackResponseDoc
            {
                ResponseType = SlackResponseType.EPHEMERAL
            };

            if (userCreated)
            {
                responseDoc.Text = "Joining game.";

                SlackResponseDoc delayedResponseDoc = new SlackResponseDoc
                {
                    ResponseType = SlackResponseType.IN_CHANNEL,
                    Text = String.Format("", user.UserId)
                };

                _delayedSlackService.sendResponse(requestDoc.ResponseUrl, delayedResponseDoc);
            }
            else
            {
                responseDoc.Text = "You're already in the game.";
            }

            return responseDoc;
        }
         */
        #endregion

        #region GetStatus
        [TestMethod]
        public void TestGetStatusWithNullGameState()
        {
            string channelId = "channel";
            SlackRequestDoc requestDoc = new SlackRequestDoc
            {
                ChannelId = channelId,
                Command = "/command"
            };

            workflowService.Setup(x => x.GetCurrentGameState(It.IsAny<string>())).Returns((GameState)null);

            SlackResponseDoc responseDoc = cut.GetStatus(requestDoc);

            Assert.IsNotNull(responseDoc);
            Assert.AreEqual(SlackResponseType.EPHEMERAL, responseDoc.ResponseType);
            Assert.AreEqual("A game has not yet been started. If you'd like to start a game, try `/command start`", responseDoc.Text);
        }

        [TestMethod]
        public void TestGetStatusWithNullHostInGameState()
        {
            string channelId = "channel";
            SlackRequestDoc requestDoc = new SlackRequestDoc
            {
                ChannelId = channelId,
                Command = "/command"
            };

            GameState gameState = new GameState
            {

            };

            workflowService.Setup(x => x.GetCurrentGameState(It.IsAny<string>())).Returns(gameState);

            SlackResponseDoc responseDoc = cut.GetStatus(requestDoc);

            Assert.IsNotNull(responseDoc);
            Assert.AreEqual(SlackResponseType.EPHEMERAL, responseDoc.ResponseType);
            Assert.AreEqual("A game has not yet been started. If you'd like to start a game, try `/command start`", responseDoc.Text);
        }

        [TestMethod]
        public void TestGetStatusWithNoTopicInGameState()
        {
            string channelId = "channel";
            string userId = "U12345";

            SlackRequestDoc requestDoc = new SlackRequestDoc
            {
                ChannelId = channelId,
                Command = "/command",
                UserId = userId
            };

            GameState gameState = new GameState
            {
                ControllingUserId = userId
            };

            workflowService.Setup(x => x.GetCurrentGameState(It.IsAny<string>())).Returns(gameState);

            SlackResponseDoc responseDoc = cut.GetStatus(requestDoc);

            Assert.IsNotNull(responseDoc);
            Assert.AreEqual(SlackResponseType.EPHEMERAL, responseDoc.ResponseType);
            Assert.AreEqual("*Topic:* None\n*Turn:* Yours\n*Question:* Waiting...", responseDoc.Text);
        }

        [TestMethod]
        public void TestGetStatusWithSameHostAndNoQuestionInGameState()
        {
            string channelId = "channel";
            string userId = "U12345";
            string topic = "some topic";

            SlackRequestDoc requestDoc = new SlackRequestDoc
            {
                ChannelId = channelId,
                Command = "/command",
                UserId = userId
            };

            GameState gameState = new GameState
            {
                ControllingUserId = userId,
                Topic = topic
            };

            workflowService.Setup(x => x.GetCurrentGameState(It.IsAny<string>())).Returns(gameState);

            SlackResponseDoc responseDoc = cut.GetStatus(requestDoc);

            Assert.IsNotNull(responseDoc);
            Assert.AreEqual(SlackResponseType.EPHEMERAL, responseDoc.ResponseType);
            Assert.AreEqual("*Topic:* some topic\n*Turn:* Yours\n*Question:* Waiting...", responseDoc.Text);
        }

        [TestMethod]
        public void TestGetStatusWithDifferentHostAndNoQuestionInGameState()
        {
            string channelId = "channel";
            string userId = "U12345";
            string controllingUserId = "U6789";
            string topic = "some topic";

            SlackRequestDoc requestDoc = new SlackRequestDoc
            {
                ChannelId = channelId,
                Command = "/command",
                UserId = userId
            };

            GameState gameState = new GameState
            {
                ControllingUserId = controllingUserId,
                Topic = topic
            };

            workflowService.Setup(x => x.GetCurrentGameState(It.IsAny<string>())).Returns(gameState);

            SlackResponseDoc responseDoc = cut.GetStatus(requestDoc);

            Assert.IsNotNull(responseDoc);
            Assert.AreEqual(SlackResponseType.EPHEMERAL, responseDoc.ResponseType);
            Assert.AreEqual("*Topic:* some topic\n*Turn:* <@U6789>\n*Question:* Waiting...", responseDoc.Text);
        }

        [TestMethod]
        public void TestGetStatusWithSameHostAndQuestionInGameState()
        {
            string channelId = "channel";
            string userId = "U12345";
            string topic = "some topic";
            string question = "some question?";

            SlackRequestDoc requestDoc = new SlackRequestDoc
            {
                ChannelId = channelId,
                Command = "/command",
                UserId = userId
            };

            GameState gameState = new GameState
            {
                ControllingUserId = userId,
                Topic = topic,
                Question = question
            };

            workflowService.Setup(x => x.GetCurrentGameState(It.IsAny<string>())).Returns(gameState);

            SlackResponseDoc responseDoc = cut.GetStatus(requestDoc);

            Assert.IsNotNull(responseDoc);
            Assert.AreEqual(SlackResponseType.EPHEMERAL, responseDoc.ResponseType);
            Assert.AreEqual("*Topic:* some topic\n*Turn:* Yours\n*Question:*\n\nsome question?\n\n*Answers:* Waiting...", responseDoc.Text);
        }

        [TestMethod]
        public void TestGetStatusWithDifferentHostAndQuestionInGameState()
        {
            string channelId = "channel";
            string userId = "U12345";
            string controllingUserId = "U6789";
            string topic = "some topic";
            string question = "some question?";

            SlackRequestDoc requestDoc = new SlackRequestDoc
            {
                ChannelId = channelId,
                Command = "/command",
                UserId = userId
            };

            GameState gameState = new GameState
            {
                ControllingUserId = controllingUserId,
                Topic = topic,
                Question = question
            };

            workflowService.Setup(x => x.GetCurrentGameState(It.IsAny<string>())).Returns(gameState);

            SlackResponseDoc responseDoc = cut.GetStatus(requestDoc);

            Assert.IsNotNull(responseDoc);
            Assert.AreEqual(SlackResponseType.EPHEMERAL, responseDoc.ResponseType);
            Assert.AreEqual("*Topic:* some topic\n*Turn:* <@U6789>\n*Question:*\n\nsome question?\n\n*Answers:* Waiting...", responseDoc.Text);
        }

        [TestMethod]
        public void TestGetStatusWithQuestionAndAnswersInGameState()
        {
            string channelId = "channel";
            string userId = "U12345";
            string topic = "some topic";
            string question = "some question?";
            List<GameState.Answer> answers = new List<GameState.Answer>
            {
                    new GameState.Answer
                    {
                        UserId = "U1111",
                        Username = "jimbob",
                        Text = "answer 1",
                        CreatedDate = DateTime.Parse("10/9/2018 16:30:33")
                    },
                    new GameState.Answer
                    {
                        UserId = "U2222",
                        Username = "joe",
                        Text = "answer 2",
                        CreatedDate = DateTime.Parse("10/9/2018 16:32:21")
                    },
                    new GameState.Answer
                    {
                        UserId = "U3333",
                        Username = "muchlongerusername",
                        Text = "answer 3",
                        CreatedDate = DateTime.Parse("10/9/2018 16:34:25")
                    }
            };

            SlackRequestDoc requestDoc = new SlackRequestDoc
            {
                ChannelId = channelId,
                Command = "/command",
                UserId = userId
            };

            GameState gameState = new GameState
            {
                ControllingUserId = userId,
                Topic = topic,
                Question = question,
                Answers = answers
            };

            workflowService.Setup(x => x.GetCurrentGameState(It.IsAny<string>())).Returns(gameState);

            SlackResponseDoc responseDoc = cut.GetStatus(requestDoc);

            Assert.IsNotNull(responseDoc);
            Assert.AreEqual(SlackResponseType.EPHEMERAL, responseDoc.ResponseType);
            Assert.AreEqual(
                "*Topic:* some topic\n*Turn:* Yours\n*Question:*\n\nsome question?\n\n*Answers:*\n\n```10/09/2018 11:30:33 AM   @jimbob                answer 1\n10/09/2018 11:32:21 AM   @joe                   answer 2\n10/09/2018 11:34:25 AM   @muchlongerusername    answer 3```",
                responseDoc.Text
            );
        }
        #endregion

        #region GetScores
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
        #endregion

        #region ResetScores
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
        #endregion
    }
}
