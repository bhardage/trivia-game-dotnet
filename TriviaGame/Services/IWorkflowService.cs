using System;
using TriviaGame.Models;

namespace TriviaGame.Services
{
    public interface IWorkflowService
    {
        void OnGameStarted(string channelId, string userId, string topic);
        void OnGameStopped(string channelId, string userId);
        void OnQuestionSubmitted(string channelId, string userId, string question);
        void OnAnswerSubmitted(
                string channelId,
                string userId,
                string username,
                string answerText,
                DateTime createdDate
        );
        void OnIncorrectAnswerSelected(string channelId, string userId, string incorrectUserId);
        void OnCorrectAnswerSelected(string channelId, string userId);
        void OnTurnChanged(string channelId, string userId, string newControllingUserId);
        GameState GetCurrentGameState(string channelId);
    }
}
