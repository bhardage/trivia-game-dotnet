using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TriviaGame.Models;

namespace TriviaGame.Services
{
    public interface ITriviaGameService
    {
        SlackResponseDoc start(SlackRequestDoc requestDoc, string topic);

        /**
         * This method is used when a person is supposed to be
         * selecting a quote but they don't want to
         */
        SlackResponseDoc stop(SlackRequestDoc requestDoc);

        /**
         * This method allows users to participate in playing the
         * game. Note that a game does not have to be started to join
         */
        SlackResponseDoc join(SlackRequestDoc requestDoc);

        SlackResponseDoc pass(SlackRequestDoc requestDoc, string target);

        SlackResponseDoc submitQuestion(SlackRequestDoc requestDoc, string question);
        SlackResponseDoc submitAnswer(SlackRequestDoc requestDoc, string answer);
        SlackResponseDoc markAnswerIncorrect(SlackRequestDoc requestDoc, string target);
        SlackResponseDoc markAnswerCorrect(SlackRequestDoc requestDoc, string target, string answer);

        SlackResponseDoc getStatus(SlackRequestDoc requestDoc);

        SlackResponseDoc getScores(SlackRequestDoc requestDoc);
        SlackResponseDoc resetScores(SlackRequestDoc requestDoc);
    }
}
