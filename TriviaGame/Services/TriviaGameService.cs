using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TriviaGame.Exceptions;
using TriviaGame.Models;
using TriviaGame.Utils;

namespace TriviaGame.Services
{
    public class TriviaGameService : ITriviaGameService
    {
        private const string GAME_NOT_STARTED_FORMAT = "A game has not yet been started. If you'd like to start a game, try `{0} start`";

        private const string BASE_STATUS_FORMAT = "*Topic:* {0}\n*Turn:* {1}\n*Question:*{2}";
        private const string ANSWERS_FORMAT = "\n\n*Answers:*{0}";
        private const string SINGLE_ANSWER_FORMAT = "{0,22}   {1}   {2}";

        private const string NO_CORRECT_ANSWER_TARGET = "none";
        private const string SCORES_FORMAT = "```Scores:\n\n{0}```";

        private const string DATE_FORMAT = "MM/dd/yyyy hh:mm:ss tt";
        private const string LOCAL_TIMEZONE_NAME = "Central Standard Time";

        private readonly IScoreService _scoreService;
        private readonly IWorkflowService _workflowService;

        private readonly IDelayedSlackService _delayedSlackService;

        public TriviaGameService(
            IScoreService scoreService,
            IWorkflowService workflowService,
            IDelayedSlackService delayedSlackService
        )
        {
            _scoreService = scoreService;
            _workflowService = workflowService;
            _delayedSlackService = delayedSlackService;
        }

        public SlackResponseDoc Start(SlackRequestDoc requestDoc, string topic)
        {
            string channelId = requestDoc.ChannelId;
            string userId = requestDoc.UserId;

            try
            {
                _workflowService.OnGameStarted(channelId, userId, topic);
            }
            catch (GameNotStartedException)
            {
                return SlackResponseDoc.Failure(String.Format(GAME_NOT_STARTED_FORMAT, requestDoc.Command));
            }
            catch (WorkflowException e)
            {
                return SlackResponseDoc.Failure(e.Message);
            }

            return new SlackResponseDoc
            {
                ResponseType = SlackResponseType.IN_CHANNEL,
                Text = String.Format("OK, <@{0}>, please ask a question.", userId)
            };
        }

        public SlackResponseDoc Stop(SlackRequestDoc requestDoc)
        {
            try
            {
                _workflowService.OnGameStopped(requestDoc.ChannelId, requestDoc.UserId);
            }
            catch (GameNotStartedException)
            {
                return SlackResponseDoc.Failure(String.Format(GAME_NOT_STARTED_FORMAT, requestDoc.Command));
            }
            catch (WorkflowException e)
            {
                return SlackResponseDoc.Failure(e.Message);
            }

            return new SlackResponseDoc
            {
                ResponseType = SlackResponseType.IN_CHANNEL,
                Text = String.Format(
                    "The game has been stopped but scores have not been cleared. If you'd like to start a new game, try `{0} start`.",
                    requestDoc.Command,
                    requestDoc.UserId
                )
            };
        }

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
                    Text = String.Format("<@{0}> has joined the game!", user.UserId)
                };

                _delayedSlackService.sendResponse(requestDoc.ResponseUrl, delayedResponseDoc);
            }
            else
            {
                responseDoc.Text = "You're already in the game.";
            }

            return responseDoc;
        }

        public SlackResponseDoc Pass(SlackRequestDoc requestDoc, string target)
        {
            string userId = SlackUtils.NormalizeId(target);

            try
            {
                bool userExists = _scoreService.DoesUserExist(requestDoc.ChannelId, userId);

                if (!userExists)
                {
                    SlackResponseDoc responseDoc = SlackResponseDoc.Failure("User " + target + " does not exist. Please choose a valid user.");
                    responseDoc.Attachments = new List<SlackAttachment> { new SlackAttachment("Usage: `" + requestDoc.Command + " pass @jsmith`") };
                    return responseDoc;
                }

                _workflowService.OnTurnChanged(requestDoc.ChannelId, requestDoc.UserId, userId);
            }
            catch (GameNotStartedException)
            {
                return SlackResponseDoc.Failure(String.Format(GAME_NOT_STARTED_FORMAT, requestDoc.Command));
            }
            catch (WorkflowException e)
            {
                return SlackResponseDoc.Failure(e.Message);
            }

            SlackResponseDoc delayedResponseDoc = new SlackResponseDoc
            {
                ResponseType = SlackResponseType.IN_CHANNEL,
                Text = String.Format("<@{0}> has decided to pass his/her turn to <@{1}>.\n\nOK, <@{1}>, it's your turn to ask a question!", requestDoc.UserId, userId)
            };
            _delayedSlackService.sendResponse(requestDoc.ResponseUrl, delayedResponseDoc);

            return new SlackResponseDoc
            {
                ResponseType = SlackResponseType.EPHEMERAL,
                Text = "Turn passed to <@" + userId + ">."
            };
        }

        public SlackResponseDoc SubmitQuestion(SlackRequestDoc requestDoc, string question)
        {
            throw new NotImplementedException();
        }

        public SlackResponseDoc SubmitAnswer(SlackRequestDoc requestDoc, string answer)
        {
            throw new NotImplementedException();
        }

        public SlackResponseDoc MarkAnswerIncorrect(SlackRequestDoc requestDoc, string target)
        {
            throw new NotImplementedException();
        }

        public SlackResponseDoc MarkAnswerCorrect(SlackRequestDoc requestDoc, string target, string answer)
        {
            throw new NotImplementedException();
        }

        public SlackResponseDoc GetStatus(SlackRequestDoc requestDoc)
        {
            GameState gameState = _workflowService.GetCurrentGameState(requestDoc.ChannelId);

            return new SlackResponseDoc
            {
                ResponseType = SlackResponseType.EPHEMERAL,
                Text = generateStatusText(requestDoc, gameState)
            };
        }

        public SlackResponseDoc GetScores(SlackRequestDoc requestDoc)
        {
            return new SlackResponseDoc
            {
                ResponseType = SlackResponseType.EPHEMERAL,
                Text = generateScoreText(requestDoc)
            };
        }

        public SlackResponseDoc ResetScores(SlackRequestDoc requestDoc)
        {
            _scoreService.ResetScores(requestDoc.ChannelId);

            return new SlackResponseDoc
            {
                ResponseType = SlackResponseType.IN_CHANNEL,
                Text = "Scores have been reset!",
                Attachments = new List<SlackAttachment> { new SlackAttachment(generateScoreText(requestDoc)) }
            };
        }

        private string generateStatusText(SlackRequestDoc requestDoc, GameState gameState)
        {
            if (gameState == null || gameState.ControllingUserId == null)
            {
                return String.Format(GAME_NOT_STARTED_FORMAT, requestDoc.Command);
            }

            bool isControllingUser = gameState.ControllingUserId == requestDoc.UserId;

            string topic = gameState.Topic == null ? "None" : gameState.Topic;
            string turn = isControllingUser ? "Yours" : "<@" + gameState.ControllingUserId + ">";
            string question = gameState.Question == null ? " Waiting..." : ("\n\n" + gameState.Question);

            string statusText = String.Format(BASE_STATUS_FORMAT, topic, turn, question);

            if (gameState.Question != null)
            {
                string answerText;

                if (gameState.Answers == null || !gameState.Answers.Any())
                {
                    answerText = " Waiting...";
                }
                else
                {
                    int maxUsernameLength = 1 + gameState.Answers
                        .Select(a => a.Username.Length)
                        .Max();

                    answerText = "\n\n```" + gameState.Answers
                        .OrderBy(answer => answer.CreatedDate)
                        .Select(answer =>
                            String.Format(
                                SINGLE_ANSWER_FORMAT,
                                TimeZoneInfo.ConvertTimeFromUtc(answer.CreatedDate, TimeZoneInfo.FindSystemTimeZoneById(LOCAL_TIMEZONE_NAME)).ToString(DATE_FORMAT),
                                String.Format("@{0,-" + maxUsernameLength + "}", answer.Username),
                                answer.Text
                            )
                        )
                        .Aggregate((a, b) => a + "\n" + b) + "```";
                }

                statusText += String.Format(ANSWERS_FORMAT, answerText);
            }

            return statusText;
        }

        private string generateScoreText(SlackRequestDoc requestDoc)
        {
            Dictionary<SlackUser, long> scoresByUser = _scoreService.GetAllScoresByUser(requestDoc.ChannelId);

            string scoreText;

            if (!scoresByUser.Any())
            {
                scoreText = "No scores yet...";
            }
            else
            {
                int maxUsernameLength = 1 + scoresByUser.Keys
                    .Select(e => e.Username.Length)
                    .Max();

                //sort by score desc, username
                var scoresByUserSorted = from pair in scoresByUser
                                         orderby pair.Value descending, pair.Key.Username ascending
                                         select pair;

                scoreText = scoresByUserSorted
                    .Select(e => String.Format("@{0,-" + maxUsernameLength + "} {1,3}", e.Key.Username + ":", e.Value))
                    .Aggregate((a, b) => a + "\n" + b);
            }

            return String.Format(SCORES_FORMAT, scoreText);
        }
    }
}
