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
    public class SlackSlashCommandServiceTest
    {
        public SlackSlashCommandService cut;

        private Mock<ITriviaGameService> triviaGameService;

        [TestInitialize]
        public void Setup()
        {
            triviaGameService = new Mock<ITriviaGameService>();
            cut = new SlackSlashCommandService(triviaGameService.Object);
        }

        [TestMethod]
        public void TestStartCommandWithNoTopic()
        {
            SlackRequestDoc requestDoc = new SlackRequestDoc
            {
                Text = "  start  "
            };
            SlackResponseDoc responseDoc = new SlackResponseDoc();

            triviaGameService.Setup(x => x.Start(It.IsAny<SlackRequestDoc>(), It.IsAny<string>())).Returns(responseDoc);

            SlackResponseDoc result = cut.ProcessSlashCommand(requestDoc);

            Assert.AreSame(responseDoc, result);

            triviaGameService.Verify(x => x.Start(requestDoc, null));
        }

        [TestMethod]
        public void TestStartCommandWithTopic()
        {
            SlackRequestDoc requestDoc = new SlackRequestDoc
            {
                Text = "  start  movie quotes"
            };
            SlackResponseDoc responseDoc = new SlackResponseDoc();

            triviaGameService.Setup(x => x.Start(It.IsAny<SlackRequestDoc>(), It.IsAny<string>())).Returns(responseDoc);

            SlackResponseDoc result = cut.ProcessSlashCommand(requestDoc);

            Assert.AreSame(responseDoc, result);

            triviaGameService.Verify(x => x.Start(requestDoc, "movie quotes"));
        }

        [TestMethod]
        public void TestStopCommand()
        {
            SlackRequestDoc requestDoc = new SlackRequestDoc
            {
                Text = "  stop  "
            };
            SlackResponseDoc responseDoc = new SlackResponseDoc();

            triviaGameService.Setup(x => x.Stop(It.IsAny<SlackRequestDoc>())).Returns(responseDoc);

            SlackResponseDoc result = cut.ProcessSlashCommand(requestDoc);

            Assert.AreSame(responseDoc, result);

            triviaGameService.Verify(x => x.Stop(requestDoc));
        }

        [TestMethod]
        public void TestQuestionCommandWithTooFewArguments()
        {
            SlackRequestDoc requestDoc = new SlackRequestDoc
            {
                Command = "/command",
                Text = "question"
            };

            SlackResponseDoc result = cut.ProcessSlashCommand(requestDoc);

            Assert.IsNotNull(result);
            Assert.AreEqual(SlackResponseType.EPHEMERAL, result.ResponseType);
            Assert.AreEqual("To submit a question, use `/command question <QUESTION_TEXT>`.\n\nFor example, `/command question In what year did WWII officially begin?`", result.Text);
            Assert.IsNull(result.Attachments);

            triviaGameService.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void TestQuestionCommand()
        {
            SlackRequestDoc requestDoc = new SlackRequestDoc
            {
                Text = "  question   What    does ATM stand for?   "
            };
            SlackResponseDoc responseDoc = new SlackResponseDoc();

            triviaGameService.Setup(x => x.SubmitQuestion(It.IsAny<SlackRequestDoc>(), It.IsAny<string>())).Returns(responseDoc);

            SlackResponseDoc result = cut.ProcessSlashCommand(requestDoc);

            Assert.AreSame(responseDoc, result);

            triviaGameService.Verify(x => x.SubmitQuestion(requestDoc, "What    does ATM stand for?"));
        }

        [TestMethod]
        public void TestAnswerCommandWithTooFewArguments()
        {
            SlackRequestDoc requestDoc = new SlackRequestDoc
            {
                Command = "/command",
                Text = "answer"
            };
        
            SlackResponseDoc result = cut.ProcessSlashCommand(requestDoc);

            Assert.IsNotNull(result);
            Assert.AreEqual(SlackResponseType.EPHEMERAL, result.ResponseType);
            Assert.AreEqual("To submit an answer, use `/command answer <ANSWER_TEXT>`.\n\nFor example, `/command answer Blue skies`", result.Text);
            Assert.IsNull(result.Attachments);

            triviaGameService.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void TestAnswerCommand()
        {
            SlackRequestDoc requestDoc = new SlackRequestDoc
            {
                Text = "  answer   I    do not    know   "
            };
            SlackResponseDoc responseDoc = new SlackResponseDoc();

            triviaGameService.Setup(x => x.SubmitAnswer(It.IsAny<SlackRequestDoc>(), It.IsAny<string>())).Returns(responseDoc);

            SlackResponseDoc result = cut.ProcessSlashCommand(requestDoc);

            Assert.AreSame(responseDoc, result);

            triviaGameService.Verify(x => x.SubmitAnswer(requestDoc, "I    do not    know"));
        }

        [TestMethod]
        public void TestMarkCorrectCommandWithTooFewArguments()
        {
            SlackRequestDoc requestDoc = new SlackRequestDoc
            {
                Command = "/command",
                Text = "correct"
            };
        
            SlackResponseDoc result = cut.ProcessSlashCommand(requestDoc);

            Assert.IsNotNull(result);
            Assert.AreEqual(SlackResponseType.EPHEMERAL, result.ResponseType);
            Assert.AreEqual(
                "To mark an answer correct, use `/command correct <USERNAME>`.\n" +
                "Optional: To include the correct answer, use `/command correct <USERNAME> <CORRECT_ANSWER>`.\n\n" +
                "For example, `/command correct @jsmith Chris Farley`",
                result.Text
            );
            Assert.IsNull(result.Attachments);

            triviaGameService.VerifyNoOtherCalls();
        }

        [TestMethod]
        public void TestMarkCorrectCommandWithNoAnswer()
        {
            SlackRequestDoc requestDoc = new SlackRequestDoc
            {
                Text = "correct <@12345>"
            };
            SlackResponseDoc responseDoc = new SlackResponseDoc();

            triviaGameService.Setup(x => x.MarkAnswerCorrect(It.IsAny<SlackRequestDoc>(), It.IsAny<string>(), It.IsAny<string>())).Returns(responseDoc);

            SlackResponseDoc result = cut.ProcessSlashCommand(requestDoc);

            Assert.AreSame(responseDoc, result);

            triviaGameService.Verify(x => x.MarkAnswerCorrect(requestDoc, "<@12345>", null));
        }

        [TestMethod]
        public void TestMarkCorrectCommandWithAnswer()
        {
            SlackRequestDoc requestDoc = new SlackRequestDoc
            {
                Text = "correct <@12345>   I    do not    know"
            };
            SlackResponseDoc responseDoc = new SlackResponseDoc();

            triviaGameService.Setup(x => x.MarkAnswerCorrect(It.IsAny<SlackRequestDoc>(), It.IsAny<string>(), It.IsAny<string>())).Returns(responseDoc);

            SlackResponseDoc result = cut.ProcessSlashCommand(requestDoc);

            Assert.AreSame(responseDoc, result);

            triviaGameService.Verify(x => x.MarkAnswerCorrect(requestDoc, "<@12345>", "I    do not    know"));
        }

        [TestMethod]
        public void TestGetScoresCommand()
        {
            SlackRequestDoc requestDoc = new SlackRequestDoc
            {
                Text = "  scores  "
            };
            SlackResponseDoc responseDoc = new SlackResponseDoc();

            triviaGameService.Setup(x => x.GetScores(It.IsAny<SlackRequestDoc>())).Returns(responseDoc);

            SlackResponseDoc result = cut.ProcessSlashCommand(requestDoc);

            Assert.AreSame(responseDoc, result);

            triviaGameService.Verify(x => x.GetScores(requestDoc));
        }

        [TestMethod]
        public void TestResetScoresCommand()
        {
            SlackRequestDoc requestDoc = new SlackRequestDoc
            {
                Text = "  reset  "
            };
            SlackResponseDoc responseDoc = new SlackResponseDoc();

            triviaGameService.Setup(x => x.ResetScores(It.IsAny<SlackRequestDoc>())).Returns(responseDoc);

            SlackResponseDoc result = cut.ProcessSlashCommand(requestDoc);

            Assert.AreSame(responseDoc, result);

            triviaGameService.Verify(x => x.ResetScores(requestDoc));
        }
    }
}
