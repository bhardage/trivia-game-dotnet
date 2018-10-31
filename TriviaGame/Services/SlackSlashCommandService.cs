using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TriviaGame.Models;

namespace TriviaGame.Services
{
    public class SlackSlashCommandService : ISlackSlashCommandService
    {
        private readonly ITriviaGameService _triviaGameService;

        public SlackSlashCommandService(ITriviaGameService triviaGameService)
        {
            _triviaGameService = triviaGameService;
        }

        public SlackResponseDoc ProcessSlashCommand(SlackRequestDoc requestDoc)
        {
            //First thing, capture the timestamp
            requestDoc.RequestTime = DateTime.Now;

            string commandText = requestDoc.Text == null ? "" : requestDoc.Text.Trim();
            string[] commandParts = Regex.Split(commandText, "\\s+");

            string operatorText = null;

            if (commandParts.Length >= 1)
            {
                operatorText = commandParts[0];
                commandText = commandText.Substring(operatorText.Length).Trim();
            }

            switch (operatorText)
            {
                case "start":
                    return _triviaGameService.Start(requestDoc, String.IsNullOrEmpty(commandText) ? null : commandText);
                case "stop":
                    return _triviaGameService.Stop(requestDoc);
                case "join":
                    return _triviaGameService.Join(requestDoc);
                case "pass":
                    if (commandParts.Length < 2)
                    {
                        return getPassFormat(requestDoc.Command);
                    }

                    return _triviaGameService.Pass(requestDoc, commandText);
                case "question":
                    if (commandParts.Length < 2)
                    {
                        return getSubmitQuestionFormat(requestDoc.Command);
                    }

                    return _triviaGameService.SubmitQuestion(requestDoc, commandText);
                case "answer":
                    if (commandParts.Length < 2)
                    {
                        return getSubmitAnswerFormat(requestDoc.Command);
                    }

                    return _triviaGameService.SubmitAnswer(requestDoc, commandText);
                case "incorrect":
                    if (commandParts.Length < 2)
                    {
                        return getMarkAnswerIncorrectFormat(requestDoc.Command);
                    }

                    return _triviaGameService.MarkAnswerIncorrect(requestDoc, commandText);
                case "correct":
                    if (commandParts.Length < 2)
                    {
                        return getMarkAnswerCorrectFormat(requestDoc.Command);
                    }

                    return _triviaGameService.MarkAnswerCorrect(
                            requestDoc,
                            commandParts[1],
                            commandParts.Length > 2 ? commandText.Substring(commandParts[1].Length).Trim() : null
                    );
                case "status":
                    return _triviaGameService.GetStatus(requestDoc);
                case "scores":
                    return _triviaGameService.GetScores(requestDoc);
                case "reset":
                    return _triviaGameService.ResetScores(requestDoc);
            }

            return getUsageFormat(requestDoc.Command);
        }

        #region Usage formats
        private SlackResponseDoc getPassFormat(string command)
        {
            return new SlackResponseDoc
            {
                ResponseType = SlackResponseType.EPHEMERAL,
                Text = "To pass your turn, use `" + command + " pass <USERNAME>`.\n\nFor example, `" + command + " pass @jsmith`"
            };
        }

        private SlackResponseDoc getSubmitQuestionFormat(string command)
        {
            return new SlackResponseDoc
            {
                ResponseType = SlackResponseType.EPHEMERAL,
                Text = "To submit a question, use `" + command + " question <QUESTION_TEXT>`.\n\nFor example, `" + command + " question In what year did WWII officially begin?`"
            };
        }

        private SlackResponseDoc getSubmitAnswerFormat(string command)
        {
            return new SlackResponseDoc
            {
                ResponseType = SlackResponseType.EPHEMERAL,
                Text = "To submit an answer, use `" + command + " answer <ANSWER_TEXT>`.\n\nFor example, `" + command + " answer Blue skies`"
            };
        }

        private SlackResponseDoc getMarkAnswerIncorrectFormat(string command)
        {
            return new SlackResponseDoc
            {
                ResponseType = SlackResponseType.EPHEMERAL,
                Text = "To identify an answer as incorrect, use `" + command + " incorrect <USERNAME>`.\n" +
                    //+ "Optional: To include the incorrect answer to which you're referring, use `" + command + " incorrect <USERNAME> <INCORRECT_ANSWER>`.\n\n" +
                    "\nFor example, `" + command + " incorrect @jsmith`"
            };
        }

        private SlackResponseDoc getMarkAnswerCorrectFormat(string command)
        {
            return new SlackResponseDoc
            {
                ResponseType = SlackResponseType.EPHEMERAL,
                Text = "To mark an answer correct, use `" + command + " correct <USERNAME>`.\n" +
                    "Optional: To include the correct answer, use `" + command + " correct <USERNAME> <CORRECT_ANSWER>`.\n\n" +
                    "For example, `" + command + " correct @jsmith Chris Farley`"
            };
        }

        private SlackResponseDoc getUsageFormat(string command)
        {
            return new SlackResponseDoc
            {
                ResponseType = SlackResponseType.EPHEMERAL,
                Text = "`" + command + "` usage:",
                Attachments = new List<SlackAttachment>
                {
                    new SlackAttachment("To start a new game as the host, use `" + command + " start`"),
                    new SlackAttachment("To join a game, use `" + command + " join`"),
                    new SlackAttachment("To ask a question, use `" + command + " question <QUESTION>`. This requires you to be the host."),
                    new SlackAttachment("To answer a question, use `" + command + " answer <ANSWER>`. (Note that answering a question will automatically join the game.)"),
                    new SlackAttachment(
                            "To identify a correct answer, use `" + command + " correct <USERNAME> <ANSWER>`." +
                                    " If no correct answers were given, use `" + command + " correct none <CORRECT_ANSWER>`. This requires you to be the host."
                    ),
                    new SlackAttachment("To pass your turn to someone else, use `" + command + " pass <USERNAME>`"),
                    new SlackAttachment("To view whose turn it is, the current question, and all answers provided so far, use `" + command + " status`"),
                    new SlackAttachment("To view the current scores, use `" + command + " scores`."),
                    new SlackAttachment("To reset all scores, use `" + command + " reset`."),
                    new SlackAttachment("To stop the current game, use `" + command + " stop`. This requires you to be the host.")
                }
            };
        }
        #endregion
    }
}
