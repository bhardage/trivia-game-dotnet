using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TriviaGame.Models;

namespace TriviaGame.Services
{
    public class SlackSlashCommandService : ISlackSlashCommandService
    {
        public SlackResponseDoc processSlashCommand(SlackRequestDoc requestDoc)
        {
            const string command = "/game";

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
    }
}
