using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using Moq;
using System;
using System.Collections.Generic;
using TriviaGame.Exceptions;
using TriviaGame.Models;
using TriviaGame.Repositories;
using TriviaGame.Services;

namespace TriviaGameTests.Services
{
    [TestClass]
    public class WorkflowServiceTest
    {
        public WorkflowService cut;

        private Mock<IWorkflowRepository> workflowRepository = new Mock<IWorkflowRepository>();

        [TestInitialize]
        public void Setup()
        {
            workflowRepository.Reset();
            cut = new WorkflowService(workflowRepository.Object);
        }

        #region OnGameStarted
        [TestMethod]
        public void TestOnGameStartedWithNullChannelId()
        {
            Exception exception = null;

            try
            {
                cut.OnGameStarted(null, "12345", null);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNull(exception);

            workflowRepository.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void TestOnGameStartedWithNullUserId()
        {
            Exception exception = null;

            try
            {
                cut.OnGameStarted("12345", null, null);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNull(exception);

            workflowRepository.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void TestOnGameStartedWithGameAlreadyStartedAndCurrentUserAsHost()
        {
            string channelId = "C12345";
            string userId = "U6789";

            Workflow workflow = new Workflow
            {
                Id = new ObjectId(),
                ChannelId = channelId,
                ControllingUserId = userId,
                Stage = WorkflowStage.QUESTION_ASKED
            };

            workflowRepository.Setup(x => x.FindByChannelId(It.IsAny<string>())).Returns(workflow);

            Exception exception = null;

            try
            {
                cut.OnGameStarted(channelId, userId, null);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(WorkflowException));
            Assert.AreEqual("You are already hosting!", exception.Message);

            workflowRepository.Verify(x => x.FindByChannelId(channelId));
            workflowRepository.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void TestOnGameStartedWithGameAlreadyStartedAndDifferentHost()
        {
            string channelId = "C12345";
            string userId = "U6789";
            string controllingUserId = "U1346";

            Workflow workflow = new Workflow
            {
                Id = new ObjectId(),
                ChannelId = channelId,
                ControllingUserId = controllingUserId,
                Stage = WorkflowStage.QUESTION_ASKED
            };

            workflowRepository.Setup(x => x.FindByChannelId(It.IsAny<string>())).Returns(workflow);

            Exception exception = null;

            try
            {
                cut.OnGameStarted(channelId, userId, null);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(WorkflowException));
            Assert.AreEqual("<@" + controllingUserId + "> is currently hosting.", exception.Message);

            workflowRepository.Verify(x => x.FindByChannelId(channelId));
            workflowRepository.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void TestOnGameStartedWithGameNotStarted()
        {
            string channelId = "C12345";
            string userId = "U6789";
            string topic = "some topic";

            Workflow capturedWorkflow = null;
            workflowRepository.Setup(x => x.FindByChannelId(It.IsAny<string>())).Returns((Workflow)null);
            workflowRepository.Setup(x => x.Save(It.IsAny<Workflow>())).Callback<Workflow>(w => capturedWorkflow = w);

            Exception exception = null;

            try
            {
                cut.OnGameStarted(channelId, userId, topic);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNull(exception);

            workflowRepository.Verify(x => x.FindByChannelId(channelId));
            workflowRepository.Verify(x => x.Save(It.IsAny<Workflow>()));

            Assert.IsNotNull(capturedWorkflow);
            Assert.AreEqual(channelId, capturedWorkflow.ChannelId);
            Assert.AreEqual(userId, capturedWorkflow.ControllingUserId);
            Assert.AreEqual(topic, capturedWorkflow.Topic);
            Assert.AreEqual(WorkflowStage.STARTED, capturedWorkflow.Stage);
        }
        #endregion

        #region OnGameStopped
        [TestMethod]
        public void TestOnGameStoppedWithNullChannelId()
        {
            Exception exception = null;

            try
            {
                cut.OnGameStopped(null, "12345");
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNull(exception);

            workflowRepository.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void TestOnGameStoppedWithNullUserId()
        {
            Exception exception = null;

            try
            {
                cut.OnGameStopped("12345", null);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNull(exception);

            workflowRepository.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void TestOnGameStoppedWithNoExistingWorkflow()
        {
            string channelId = "C12345";
            string userId = "U6789";

            workflowRepository.Setup(x => x.FindByChannelId(It.IsAny<string>())).Returns((Workflow)null);

            Exception exception = null;

            try
            {
                cut.OnGameStopped(channelId, userId);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(GameNotStartedException));

            workflowRepository.Verify(x => x.FindByChannelId(channelId));
            workflowRepository.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void TestOnGameStoppedWithExistingWorkflowAndDifferentHost()
        {
            string channelId = "C12345";
            string userId = "U6789";
            string controllingUserId = "U1346";

            Workflow workflow = new Workflow
            {
                Id = new ObjectId(),
                ChannelId = channelId,
                ControllingUserId = controllingUserId,
                Stage = WorkflowStage.QUESTION_ASKED
            };

            workflowRepository.Setup(x => x.FindByChannelId(It.IsAny<string>())).Returns(workflow);

            Exception exception = null;

            try
            {
                cut.OnGameStopped(channelId, userId);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(WorkflowException));
            Assert.AreEqual("<@" + controllingUserId + "> is currently hosting.", exception.Message);

            workflowRepository.Verify(x => x.FindByChannelId(channelId));
            workflowRepository.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void TestOnGameStoppedWithExistingWorkflowAndSameHost()
        {
            ObjectId id = new ObjectId();
            string channelId = "C12345";
            string userId = "U6789";

            Workflow workflow = new Workflow
            {
                Id = id,
                ChannelId = channelId,
                ControllingUserId = userId,
                Stage = WorkflowStage.QUESTION_ASKED
            };

            workflowRepository.Setup(x => x.FindByChannelId(It.IsAny<string>())).Returns(workflow);

            Exception exception = null;

            try
            {
                cut.OnGameStopped(channelId, userId);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNull(exception);

            workflowRepository.Verify(x => x.FindByChannelId(channelId));
            workflowRepository.Verify(x => x.DeleteById(id));
        }
        #endregion

        #region OnQuestionSubmitted
        [TestMethod]
        public void TestOnQuestionSubmittedWithNullChannelId()
        {
            Exception exception = null;

            try
            {
                cut.OnQuestionSubmitted(null, "12345", "test question");
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNull(exception);

            workflowRepository.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void TestOnQuestionSubmittedWithNullUserId()
        {
            Exception exception = null;

            try
            {
                cut.OnQuestionSubmitted("12345", null, "test question");
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNull(exception);

            workflowRepository.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void TestOnQuestionSubmittedWithNoExistingWorkflow()
        {
            string channelId = "C12345";
            string userId = "U6789";
            string question = "test question";

            workflowRepository.Setup(x => x.FindByChannelId(It.IsAny<string>())).Returns((Workflow)null);

            Exception exception = null;

            try
            {
                cut.OnQuestionSubmitted(channelId, userId, question);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(GameNotStartedException));

            workflowRepository.Verify(x => x.FindByChannelId(channelId));
            workflowRepository.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void TestOnQuestionSubmittedWithDifferentHostAndQuestionAsked()
        {
            string channelId = "C12345";
            string userId = "U6789";
            string controllingUserId = "U1346";
            string question = "test question";

            Workflow workflow = new Workflow
            {
                Id = new ObjectId(),
                ChannelId = channelId,
                ControllingUserId = controllingUserId,
                Stage = WorkflowStage.QUESTION_ASKED
            };

            workflowRepository.Setup(x => x.FindByChannelId(It.IsAny<string>())).Returns(workflow);

            Exception exception = null;

            try
            {
                cut.OnQuestionSubmitted(channelId, userId, question);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(WorkflowException));
            Assert.AreEqual("<@" + controllingUserId + "> has already asked a question.", exception.Message);

            workflowRepository.Verify(x => x.FindByChannelId(channelId));
            workflowRepository.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void TestOnQuestionSubmittedWithDifferentHostAndNoQuestionAsked()
        {
            string channelId = "C12345";
            string userId = "U6789";
            string controllingUserId = "U1346";
            string question = "test question";

            Workflow workflow = new Workflow
            {
                Id = new ObjectId(),
                ChannelId = channelId,
                ControllingUserId = controllingUserId,
                Stage = WorkflowStage.STARTED
            };

            workflowRepository.Setup(x => x.FindByChannelId(It.IsAny<string>())).Returns(workflow);

            Exception exception = null;

            try
            {
                cut.OnQuestionSubmitted(channelId, userId, question);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(WorkflowException));
            Assert.AreEqual("It's <@" + controllingUserId + ">'s turn to ask a question.", exception.Message);

            workflowRepository.Verify(x => x.FindByChannelId(channelId));
            workflowRepository.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void TestOnQuestionSubmittedWithSameHostAndQuestionAsked()
        {
            string channelId = "C12345";
            string userId = "U6789";
            string question = "test question";

            Workflow workflow = new Workflow
            {
                Id = new ObjectId(),
                ChannelId = channelId,
                ControllingUserId = userId,
                Stage = WorkflowStage.QUESTION_ASKED
            };

            workflowRepository.Setup(x => x.FindByChannelId(It.IsAny<string>())).Returns(workflow);

            Exception exception = null;

            try
            {
                cut.OnQuestionSubmitted(channelId, userId, question);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(WorkflowException));
            Assert.AreEqual("You have already asked a question.", exception.Message);

            workflowRepository.Verify(x => x.FindByChannelId(channelId));
            workflowRepository.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void TestOnQuestionSubmittedWithSameHostAndNoQuestionAsked()
        {
            ObjectId id = new ObjectId();
            string channelId = "C12345";
            string userId = "U6789";
            string question = "test question";

            Workflow workflow = new Workflow
            {
                Id = new ObjectId(),
                ChannelId = channelId,
                ControllingUserId = userId,
                Stage = WorkflowStage.STARTED
            };

            Workflow capturedWorkflow = null;
            workflowRepository.Setup(x => x.FindByChannelId(It.IsAny<string>())).Returns(workflow);
            workflowRepository.Setup(x => x.Save(It.IsAny<Workflow>())).Callback<Workflow>(w => capturedWorkflow = w);

            Exception exception = null;

            try
            {
                cut.OnQuestionSubmitted(channelId, userId, question);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNull(exception);

            workflowRepository.Verify(x => x.FindByChannelId(channelId));

            Assert.IsNotNull(capturedWorkflow);
            Assert.AreEqual(id, capturedWorkflow.Id);
            Assert.AreEqual(channelId, capturedWorkflow.ChannelId);
            Assert.AreEqual(userId, capturedWorkflow.ControllingUserId);
            Assert.AreEqual(question, capturedWorkflow.Question);
            Assert.AreEqual(WorkflowStage.QUESTION_ASKED, capturedWorkflow.Stage);
        }
        #endregion

        #region OnAnswerSubmitted
        [TestMethod]
        public void TestOnAnswerSubmittedWithNullChannelId()
        {
            Exception exception = null;

            try
            {
                cut.OnAnswerSubmitted(null, "12345", null, null, DateTime.Now);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNull(exception);

            workflowRepository.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void TestOnAnswerSubmittedWithNullUserId()
        {
            Exception exception = null;

            try
            {
                cut.OnAnswerSubmitted("12345", null, null, null, DateTime.Now);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNull(exception);

            workflowRepository.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void TestOnAnswerSubmittedWithNoExistingWorkflow()
        {
            string channelId = "C12345";
            string userId = "U6789";

            workflowRepository.Setup(x => x.FindByChannelId(It.IsAny<string>())).Returns((Workflow)null);

            Exception exception = null;

            try
            {
                cut.OnAnswerSubmitted(channelId, userId, null, null, DateTime.Now);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(GameNotStartedException));

            workflowRepository.Verify(x => x.FindByChannelId(channelId));
            workflowRepository.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void TestOnAnswerSubmittedWithSameHostAndNoQuestionAsked()
        {
            string channelId = "C12345";
            string userId = "U6789";

            Workflow workflow = new Workflow
            {
                Id = new ObjectId(),
                ChannelId = channelId,
                ControllingUserId = userId,
                Stage = WorkflowStage.STARTED
            };

            workflowRepository.Setup(x => x.FindByChannelId(It.IsAny<string>())).Returns(workflow);

            Exception exception = null;

            try
            {
                cut.OnAnswerSubmitted(channelId, userId, null, null, DateTime.Now);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(WorkflowException));
            Assert.AreEqual("You can't answer your own question!", exception.Message);

            workflowRepository.Verify(x => x.FindByChannelId(channelId));
            workflowRepository.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void TestOnAnswerSubmittedWithSameHostAndQuestionAsked()
        {
            string channelId = "C12345";
            string userId = "U6789";

            Workflow workflow = new Workflow
            {
                Id = new ObjectId(),
                ChannelId = channelId,
                ControllingUserId = userId,
                Stage = WorkflowStage.QUESTION_ASKED
            };

            workflowRepository.Setup(x => x.FindByChannelId(It.IsAny<string>())).Returns(workflow);

            Exception exception = null;

            try
            {
                cut.OnAnswerSubmitted(channelId, userId, null, null, DateTime.Now);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(WorkflowException));
            Assert.AreEqual("You can't answer your own question!", exception.Message);

            workflowRepository.Verify(x => x.FindByChannelId(channelId));
            workflowRepository.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void TestOnAnswerSubmittedWithDifferentHostAndNoQuestionAsked()
        {
            string channelId = "C12345";
            string userId = "U6789";
            string controllingUserId = "U1346";

            Workflow workflow = new Workflow
            {
                Id = new ObjectId(),
                ChannelId = channelId,
                ControllingUserId = controllingUserId,
                Stage = WorkflowStage.STARTED
            };

            workflowRepository.Setup(x => x.FindByChannelId(It.IsAny<string>())).Returns(workflow);

            Exception exception = null;

            try
            {
                cut.OnAnswerSubmitted(channelId, userId, null, null, DateTime.Now);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(WorkflowException));
            Assert.AreEqual("A question has not yet been submitted. Please wait for <@" + controllingUserId + "> to ask a question.", exception.Message);

            workflowRepository.Verify(x => x.FindByChannelId(channelId));
            workflowRepository.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void TestOnAnswerSubmittedWithDifferentHostAndQuestionAsked()
        {
            ObjectId id = new ObjectId();
            string channelId = "C12345";
            string userId = "U6789";
            string username = "myusername";
            string answerText = "answer test";
            DateTime answerTime = DateTime.Now;
            string controllingUserId = "U1346";

            Workflow workflow = new Workflow
            {
                Id = id,
                ChannelId = channelId,
                ControllingUserId = controllingUserId,
                Stage = WorkflowStage.QUESTION_ASKED
            };

            Workflow capturedWorkflow = null;
            workflowRepository.Setup(x => x.FindByChannelId(It.IsAny<string>())).Returns(workflow);
            workflowRepository.Setup(x => x.Save(It.IsAny<Workflow>())).Callback<Workflow>(w => capturedWorkflow = w);

            Exception exception = null;

            try
            {
                cut.OnAnswerSubmitted(channelId, userId, username, answerText, answerTime);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNull(exception);

            workflowRepository.Verify(x => x.FindByChannelId(channelId));
            workflowRepository.Verify(x => x.Save(It.IsAny<Workflow>()));

            Assert.IsNotNull(capturedWorkflow);
            Assert.AreEqual(id, capturedWorkflow.Id);
            Assert.AreEqual(channelId, capturedWorkflow.ChannelId);
            Assert.AreEqual(controllingUserId, capturedWorkflow.ControllingUserId);
            Assert.AreEqual(WorkflowStage.QUESTION_ASKED, capturedWorkflow.Stage);

            List<Answer> answers = capturedWorkflow.Answers;
            Assert.IsNotNull(answers);
            Assert.AreEqual(1, answers.Count);
            Assert.AreEqual(userId, answers[0].UserId);
            Assert.AreEqual(username, answers[0].Username);
            Assert.AreEqual(answerText, answers[0].Text);
            Assert.AreEqual(answerTime, answers[0].CreatedDate);
        }
        #endregion

        #region OnIncorrectAnswerSelected
        [TestMethod]
        public void TestOnIncorrectAnswerSelectedWithNullChannelId()
        {
            Exception exception = null;

            try
            {
                cut.OnIncorrectAnswerSelected(null, "12345", "12346");
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNull(exception);

            workflowRepository.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void TestOnIncorrectAnswerSelectedWithNullUserId()
        {
            Exception exception = null;

            try
            {
                cut.OnIncorrectAnswerSelected("12345", null, "12346");
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNull(exception);

            workflowRepository.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void TestOnIncorrectAnswerSelectedWithNullIncorrectUserId()
        {
            Exception exception = null;

            try
            {
                cut.OnIncorrectAnswerSelected("12345", "12345", null);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNull(exception);

            workflowRepository.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void TestOnIncorrectAnswerSelectedWithNoExistingWorkflow()
        {
            string channelId = "C12345";
            string userId = "U6789";
            string incorrectUserId = "U78910";

            workflowRepository.Setup(x => x.FindByChannelId(It.IsAny<string>())).Returns((Workflow)null);

            Exception exception = null;

            try
            {
                cut.OnIncorrectAnswerSelected(channelId, userId, incorrectUserId);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(GameNotStartedException));

            workflowRepository.Verify(x => x.FindByChannelId(channelId));
        }

        [TestMethod]
        public void TestOnIncorrectAnswerSelectedWithDifferentHost()
        {
            string channelId = "C12345";
            string userId = "U6789";
            string incorrectUserId = "U78910";
            string controllingUserId = "U1346";

            Workflow workflow = new Workflow
            {
                Id = new ObjectId(),
                ChannelId = channelId,
                ControllingUserId = controllingUserId,
                Stage = WorkflowStage.STARTED
            };

            workflowRepository.Setup(x => x.FindByChannelId(It.IsAny<string>())).Returns(workflow);

            Exception exception = null;

            try
            {
                cut.OnIncorrectAnswerSelected(channelId, userId, incorrectUserId);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(WorkflowException));
            Assert.AreEqual("It's <@" + controllingUserId + ">'s turn; only he/she can identify an incorrect answer.", exception.Message);

            workflowRepository.Verify(x => x.FindByChannelId(channelId));
        }

        [TestMethod]
        public void TestOnIncorrectAnswerSelectedWithSameHostAndNoQuestionAsked()
        {
            string channelId = "C12345";
            string userId = "U6789";
            string incorrectUserId = "U78910";

            Workflow workflow = new Workflow
            {
                Id = new ObjectId(),
                ChannelId = channelId,
                ControllingUserId = userId,
                Stage = WorkflowStage.STARTED
            };

            workflowRepository.Setup(x => x.FindByChannelId(It.IsAny<string>())).Returns(workflow);

            Exception exception = null;

            try
            {
                cut.OnIncorrectAnswerSelected(channelId, userId, incorrectUserId);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(WorkflowException));
            Assert.AreEqual("A question has not yet been submitted. Please ask a question before identifying an incorrect answer.", exception.Message);

            workflowRepository.Verify(x => x.FindByChannelId(channelId));
        }

        [TestMethod]
        public void TestOnIncorrectAnswerSelectedWithSameHostAndQuestionAskedAndNoAnswersFromIncorrectUser()
        {
            string channelId = "C12345";
            string userId = "U6789";
            string incorrectUserId = "U78910";

            Workflow workflow = new Workflow
            {
                Id = new ObjectId(),
                ChannelId = channelId,
                ControllingUserId = userId,
                Stage = WorkflowStage.QUESTION_ASKED,
                Answers = new List<Answer>()
            };

            workflowRepository.Setup(x => x.FindByChannelId(It.IsAny<string>())).Returns(workflow);

            Exception exception = null;

            try
            {
                cut.OnIncorrectAnswerSelected(channelId, userId, incorrectUserId);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(WorkflowException));
            Assert.AreEqual("User <@" + incorrectUserId + "> either doesn't exist or has not answered this question yet.", exception.Message);

            workflowRepository.Verify(x => x.FindByChannelId(channelId));
        }

        [TestMethod]
        public void TestOnIncorrectAnswerSelectedWithSameHostAndQuestionAskedAndAnswerFromIncorrectUser()
        {
            string channelId = "C12345";
            string userId = "U6789";
            string incorrectUserId = "U78910";

            Workflow workflow = new Workflow
            {
                Id = new ObjectId(),
                ChannelId = channelId,
                ControllingUserId = userId,
                Stage = WorkflowStage.QUESTION_ASKED,
                Answers = new List<Answer> { new Answer { UserId = incorrectUserId } }
            };

            workflowRepository.Setup(x => x.FindByChannelId(It.IsAny<string>())).Returns(workflow);

            Exception exception = null;

            try
            {
                cut.OnIncorrectAnswerSelected(channelId, userId, incorrectUserId);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNull(exception);

            workflowRepository.Verify(x => x.FindByChannelId(channelId));
        }
        #endregion

        #region OnCorrectAnswerSelected
        [TestMethod]
        public void TestOnCorrectAnswerSelectedWithNullChannelId()
        {
            Exception exception = null;

            try
            {
                cut.OnCorrectAnswerSelected(null, "12345");
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNull(exception);

            workflowRepository.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void TestOnCorrectAnswerSelectedWithNullUserId()
        {
            Exception exception = null;

            try
            {
                cut.OnCorrectAnswerSelected("12345", null);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNull(exception);

            workflowRepository.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void TestOnCorrectAnswerSelectedWithNoExistingWorkflow()
        {
            string channelId = "C12345";
            string userId = "U6789";

            workflowRepository.Setup(x => x.FindByChannelId(It.IsAny<string>())).Returns((Workflow)null);

            Exception exception = null;

            try
            {
                cut.OnCorrectAnswerSelected(channelId, userId);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(GameNotStartedException));

            workflowRepository.Verify(x => x.FindByChannelId(channelId));
        }

        [TestMethod]
        public void TestOnCorrectAnswerSelectedWithDifferentHostAndNoQuestionAsked()
        {
            string channelId = "C12345";
            string userId = "U6789";
            string controllingUserId = "U1346";

            Workflow workflow = new Workflow
            {
                Id = new ObjectId(),
                ChannelId = channelId,
                ControllingUserId = controllingUserId,
                Stage = WorkflowStage.STARTED
            };

            workflowRepository.Setup(x => x.FindByChannelId(It.IsAny<string>())).Returns(workflow);

            Exception exception = null;

            try
            {
                cut.OnCorrectAnswerSelected(channelId, userId);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(WorkflowException));
            Assert.AreEqual("It's <@" + controllingUserId + ">'s turn; only he/she can mark an answer correct.", exception.Message);

            workflowRepository.Verify(x => x.FindByChannelId(channelId));
        }

        [TestMethod]
        public void TestOnCorrectAnswerSelectedWithDifferentHostAndQuestionAsked()
        {
            string channelId = "C12345";
            string userId = "U6789";
            string controllingUserId = "U1346";

            Workflow workflow = new Workflow
            {
                Id = new ObjectId(),
                ChannelId = channelId,
                ControllingUserId = controllingUserId,
                Stage = WorkflowStage.QUESTION_ASKED
            };

            workflowRepository.Setup(x => x.FindByChannelId(It.IsAny<string>())).Returns(workflow);

            Exception exception = null;

            try
            {
                cut.OnCorrectAnswerSelected(channelId, userId);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(WorkflowException));
            Assert.AreEqual("It's <@" + controllingUserId + ">'s turn; only he/she can mark an answer correct.", exception.Message);

            workflowRepository.Verify(x => x.FindByChannelId(channelId));
        }

        [TestMethod]
        public void TestOnCorrectAnswerSelectedWithSameHostAndNoQuestionAsked()
        {
            string channelId = "C12345";
            string userId = "U6789";

            Workflow workflow = new Workflow
            {
                Id = new ObjectId(),
                ChannelId = channelId,
                ControllingUserId = userId,
                Stage = WorkflowStage.STARTED
            };

            workflowRepository.Setup(x => x.FindByChannelId(It.IsAny<string>())).Returns(workflow);

            Exception exception = null;

            try
            {
                cut.OnCorrectAnswerSelected(channelId, userId);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(WorkflowException));
            Assert.AreEqual("A question has not yet been submitted. Please ask a question before marking an answer correct.", exception.Message);

            workflowRepository.Verify(x => x.FindByChannelId(channelId));
        }

        [TestMethod]
        public void TestOnCorrectAnswerSelectedWithSameHostAndQuestionAsked()
        {
            string channelId = "C12345";
            string userId = "U6789";

            Workflow workflow = new Workflow
            {
                Id = new ObjectId(),
                ChannelId = channelId,
                ControllingUserId = userId,
                Stage = WorkflowStage.QUESTION_ASKED
            };

            workflowRepository.Setup(x => x.FindByChannelId(It.IsAny<string>())).Returns(workflow);

            Exception exception = null;

            try
            {
                cut.OnCorrectAnswerSelected(channelId, userId);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNull(exception);

            workflowRepository.Verify(x => x.FindByChannelId(channelId));
        }
        #endregion

        #region OnTurnChanged
        [TestMethod]
        public void TestOnTurnChangedWithNullChannelId()
        {
            Exception exception = null;

            try
            {
                cut.OnTurnChanged(null, "12345", "6789");
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNull(exception);

            workflowRepository.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void TestOnTurnChangedWithNullUserId()
        {
            Exception exception = null;

            try
            {
                cut.OnTurnChanged("12345", null, "6789");
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNull(exception);

            workflowRepository.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void TestOnTurnChangedWithNullControllingUserId()
        {
            Exception exception = null;

            try
            {
                cut.OnTurnChanged("12345", "6789", null);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNull(exception);

            workflowRepository.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void TestOnTurnChangedWithNoExistingWorkflow()
        {
            string channelId = "C12345";
            string userId = "U6789";
            string newControllingUserId = "U1532";

            workflowRepository.Setup(x => x.FindByChannelId(It.IsAny<string>())).Returns((Workflow)null);

            Exception exception = null;

            try
            {
                cut.OnTurnChanged(channelId, userId, newControllingUserId);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(GameNotStartedException));

            workflowRepository.Verify(x => x.FindByChannelId(channelId));
            workflowRepository.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void TestOnTurnChangedWithDifferentHost()
        {
            string channelId = "C12345";
            string userId = "U6789";
            string newControllingUserId = "U1532";
            string controllingUserId = "U1346";

            Workflow workflow = new Workflow
            {
                Id = new ObjectId(),
                ChannelId = channelId,
                ControllingUserId = controllingUserId,
                Stage = WorkflowStage.QUESTION_ASKED
            };

            workflowRepository.Setup(x => x.FindByChannelId(It.IsAny<string>())).Returns(workflow);

            Exception exception = null;

            try
            {
                cut.OnTurnChanged(channelId, userId, newControllingUserId);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(WorkflowException));
            Assert.AreEqual("It's <@" + controllingUserId + ">'s turn; only he/she can cede his/her turn.", exception.Message);

            workflowRepository.Verify(x => x.FindByChannelId(channelId));
            workflowRepository.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void TestOnTurnChangedWithSameHost()
        {
            ObjectId id = new ObjectId();
            string channelId = "C12345";
            string userId = "U6789";
            string newControllingUserId = "U1532";

            Workflow workflow = new Workflow
            {
                Id = id,
                ChannelId = channelId,
                ControllingUserId = userId,
                Stage = WorkflowStage.QUESTION_ASKED
            };

            Workflow capturedWorkflow = null;
            workflowRepository.Setup(x => x.FindByChannelId(It.IsAny<string>())).Returns(workflow);
            workflowRepository.Setup(x => x.Save(It.IsAny<Workflow>())).Callback<Workflow>(w => capturedWorkflow = w);

            Exception exception = null;

            try
            {
                cut.OnTurnChanged(channelId, userId, newControllingUserId);
            }
            catch (Exception e)
            {
                exception = e;
            }

            Assert.IsNull(exception);

            workflowRepository.Verify(x => x.FindByChannelId(channelId));

            Assert.IsNotNull(capturedWorkflow);
            Assert.AreEqual(id, capturedWorkflow.Id);
            Assert.AreEqual(channelId, capturedWorkflow.ChannelId);
            Assert.AreEqual(newControllingUserId, capturedWorkflow.ControllingUserId);
            Assert.AreEqual(WorkflowStage.STARTED, capturedWorkflow.Stage);
        }
        #endregion

        #region GetCurrentGameState
        [TestMethod]
        public void TestGetCurrentGameStateWithNullChannelId()
        {
            GameState result = cut.GetCurrentGameState(null);

            Assert.IsNull(result);

            workflowRepository.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void TestGetCurrentGameStateWithNoExistingWorkflow()
        {
            string channelId = "C12345";

            workflowRepository.Setup(x => x.FindByChannelId(It.IsAny<string>())).Returns((Workflow)null);

            GameState result = cut.GetCurrentGameState(channelId);

            Assert.IsNotNull(result);
            Assert.IsNull(result.ControllingUserId);
            Assert.IsNull(result.Topic);
            Assert.IsNull(result.Question);
            Assert.IsNull(result.Answers);

            workflowRepository.Verify(x => x.FindByChannelId(channelId));
        }

        [TestMethod]
        public void TestGetCurrentGameStateWithNoQuestionAsked()
        {
            string channelId = "C12345";
            string controllingUserId = "U12345";
            string topic = "some topic";

            Workflow workflow = new Workflow
            {
                ControllingUserId = controllingUserId,
                Topic = topic,
                Stage = WorkflowStage.STARTED
            };

            workflowRepository.Setup(x => x.FindByChannelId(It.IsAny<string>())).Returns(workflow);

            GameState result = cut.GetCurrentGameState(channelId);

            Assert.IsNotNull(result);
            Assert.AreEqual(controllingUserId, result.ControllingUserId);
            Assert.AreEqual(topic, result.Topic);
            Assert.IsNull(result.Question);
            Assert.IsNull(result.Answers);

            workflowRepository.Verify(x => x.FindByChannelId(channelId));
        }

        [TestMethod]
        public void TestGetCurrentGameStateWithQuestionAskedAndNoAnswers()
        {
            string channelId = "C12345";
            string controllingUserId = "U12345";
            string topic = "some topic";
            string question = "some question";

            Workflow workflow = new Workflow
            {
                ControllingUserId = controllingUserId,
                Topic = topic,
                Question = question,
                Stage = WorkflowStage.QUESTION_ASKED
            };

            workflowRepository.Setup(x => x.FindByChannelId(It.IsAny<string>())).Returns(workflow);

            GameState result = cut.GetCurrentGameState(channelId);

            Assert.IsNotNull(result);
            Assert.AreEqual(controllingUserId, result.ControllingUserId);
            Assert.AreEqual(topic, result.Topic);
            Assert.AreEqual(question, result.Question);
            Assert.IsNotNull(result.Answers);
            Assert.AreEqual(0, result.Answers.Count);

            workflowRepository.Verify(x => x.FindByChannelId(channelId));
        }

        [TestMethod]
        public void TestGetCurrentGameStateWithQuestionAskedAndAnswers()
        {
            string channelId = "C12345";
            string controllingUserId = "U12345";
            string topic = "some topic";
            string question = "some question";

            Workflow workflow = new Workflow
            {
                ControllingUserId = controllingUserId,
                Topic = topic,
                Question = question,
                Answers = new List<Answer>
                {
                    new Answer
                    {
                        UserId = "U12346",
                        Username = "test1",
                        Text = "answer 1",
                        CreatedDate = DateTime.Now
                    },
                    new Answer
                    {
                        UserId = "U12347",
                        Username = "test2",
                        Text = "answer 2",
                        CreatedDate = DateTime.Now
                    }
                },
                Stage = WorkflowStage.QUESTION_ASKED
            };

            workflowRepository.Setup(x => x.FindByChannelId(It.IsAny<string>())).Returns(workflow);

            GameState result = cut.GetCurrentGameState(channelId);

            Assert.IsNotNull(result);
            Assert.AreEqual(controllingUserId, result.ControllingUserId);
            Assert.AreEqual(topic, result.Topic);
            Assert.AreEqual(question, result.Question);

            Assert.IsNotNull(result.Answers);
            Assert.AreEqual(2, result.Answers.Count);

            Assert.AreEqual(workflow.Answers[0].UserId, result.Answers[0].UserId);
            Assert.AreEqual(workflow.Answers[0].Username, result.Answers[0].Username);
            Assert.AreEqual(workflow.Answers[0].Text, result.Answers[0].Text);
            Assert.AreEqual(workflow.Answers[0].CreatedDate, result.Answers[0].CreatedDate);

            Assert.AreEqual(workflow.Answers[1].UserId, result.Answers[1].UserId);
            Assert.AreEqual(workflow.Answers[1].Username, result.Answers[1].Username);
            Assert.AreEqual(workflow.Answers[1].Text, result.Answers[1].Text);
            Assert.AreEqual(workflow.Answers[1].CreatedDate, result.Answers[1].CreatedDate);

            workflowRepository.Verify(x => x.FindByChannelId(channelId));
        }
        #endregion
    }
}
