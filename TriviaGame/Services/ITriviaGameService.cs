using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TriviaGame.Models;

namespace TriviaGame.Services
{
    public interface ITriviaGameService
    {
        SlackResponseDoc Start(SlackRequestDoc requestDoc, string topic);

        /**
         * This method is used when a person is supposed to be
         * selecting a quote but they don't want to
         */
        SlackResponseDoc Stop(SlackRequestDoc requestDoc);

        /**
         * This method allows users to participate in playing the
         * game. Note that a game does not have to be started to join
         */
        SlackResponseDoc Join(SlackRequestDoc requestDoc);

        SlackResponseDoc Pass(SlackRequestDoc requestDoc, string target);

        SlackResponseDoc SubmitQuestion(SlackRequestDoc requestDoc, string question);
        SlackResponseDoc SubmitAnswer(SlackRequestDoc requestDoc, string answer);
        SlackResponseDoc MarkAnswerIncorrect(SlackRequestDoc requestDoc, string target);
        SlackResponseDoc MarkAnswerCorrect(SlackRequestDoc requestDoc, string target, string answer);

        SlackResponseDoc GetStatus(SlackRequestDoc requestDoc);

        SlackResponseDoc GetScores(SlackRequestDoc requestDoc);
        SlackResponseDoc ResetScores(SlackRequestDoc requestDoc);
    }
}
