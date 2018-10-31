using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TriviaGame.Models;

namespace TriviaGame.Services
{
    public class TriviaGameService : ITriviaGameService
    {
        private const string NO_CORRECT_ANSWER_TARGET = "none";
        private const string SCORES_FORMAT = "```Scores:\n\n{0}```";

        private readonly IScoreService _scoreService;

        public TriviaGameService(IScoreService scoreService)
        {
            _scoreService = scoreService;
        }

        public SlackResponseDoc Start(SlackRequestDoc requestDoc, string topic)
        {
            throw new NotImplementedException();
        }

        public SlackResponseDoc Stop(SlackRequestDoc requestDoc)
        {
            throw new NotImplementedException();
        }

        public SlackResponseDoc Join(SlackRequestDoc requestDoc)
        {
            throw new NotImplementedException();
        }

        public SlackResponseDoc Pass(SlackRequestDoc requestDoc, string target)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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

        private string generateScoreText(SlackRequestDoc requestDoc)
        {
            Dictionary<SlackUser, long> scoresByUser = _scoreService.GetAllScoresByUser(requestDoc.ChannelId);

            string scoreText;

            if (scoresByUser.Count == 0)
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
