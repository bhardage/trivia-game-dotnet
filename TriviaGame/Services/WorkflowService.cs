using System;
using System.Collections.Generic;
using System.Linq;
using TriviaGame.Exceptions;
using TriviaGame.Models;
using TriviaGame.Repositories;

namespace TriviaGame.Services
{
    public class WorkflowService : IWorkflowService
    {
        private readonly IWorkflowRepository _workflowRepository;

        public WorkflowService(IWorkflowRepository workflowRepository)
        {
            _workflowRepository = workflowRepository;
        }

        public void OnGameStarted(string channelId, string userId, string topic)
        {
            if (channelId == null || userId == null)
            {
                return;
            }

            Workflow workflow = _workflowRepository.FindByChannelId(channelId);

            if (workflow != null)
            {
                String message = userId == workflow.ControllingUserId ?
                        "You are already hosting!" :
                        "<@" + workflow.ControllingUserId + "> is currently hosting.";

                throw new WorkflowException(message);
            }

            workflow = new Workflow
            {
                ChannelId = channelId,
                ControllingUserId = userId,
                Topic = topic,
                Question = null,
                Stage = WorkflowStage.STARTED
            };
            _workflowRepository.Save(workflow);
        }

        public void OnGameStopped(string channelId, string userId)
        {
            if (channelId == null || userId == null)
            {
                return;
            }

            Workflow workflow = _workflowRepository.FindByChannelId(channelId);

            if (workflow == null)
            {
                throw new GameNotStartedException();
            }
            else if (userId != workflow.ControllingUserId)
            {
                throw new WorkflowException("<@" + workflow.ControllingUserId + "> is currently hosting.");
            }

            _workflowRepository.DeleteById(workflow.Id);
        }

        public void OnQuestionSubmitted(string channelId, string userId, string question)
        {
            if (channelId == null || userId == null)
            {
                return;
            }

            Workflow workflow = _workflowRepository.FindByChannelId(channelId);

            if (workflow == null)
            {
                throw new GameNotStartedException();
            }
            else
            {
                bool isControllingUser = userId == workflow.ControllingUserId;

                if (workflow.Stage == WorkflowStage.QUESTION_ASKED)
                {
                    throw new WorkflowException((isControllingUser ? "You have" : "<@" + workflow.ControllingUserId + "> has") + " already asked a question.");
                }
                else if (!isControllingUser)
                {
                    throw new WorkflowException("It's <@" + workflow.ControllingUserId + ">'s turn to ask a question.");
                }
            }

            workflow.Question = question;
            workflow.Stage = WorkflowStage.QUESTION_ASKED;
            _workflowRepository.Save(workflow);
        }

        public void OnAnswerSubmitted(
            string channelId,
            string userId,
            string username,
            string answerText,
            DateTime createdDate
        )
        {
            if (channelId == null || userId == null)
            {
                return;
            }

            Workflow workflow = _workflowRepository.FindByChannelId(channelId);

            if (workflow == null)
            {
                throw new GameNotStartedException();
            }
            else if (userId == workflow.ControllingUserId)
            {
                throw new WorkflowException("You can't answer your own question!");
            }
            else if (workflow.Stage != WorkflowStage.QUESTION_ASKED)
            {
                throw new WorkflowException("A question has not yet been submitted. Please wait for <@" + workflow.ControllingUserId + "> to ask a question.");
            }

            Answer answer = new Answer
            {
                UserId = userId,
                Username = username,
                Text = answerText,
                CreatedDate = createdDate
            };
            workflow.Answers.Add(answer);
            _workflowRepository.Save(workflow);
        }

        public void OnIncorrectAnswerSelected(string channelId, string userId, string incorrectUserId)
        {
            if (channelId == null || userId == null || incorrectUserId == null)
            {
                return;
            }

            Workflow workflow = _workflowRepository.FindByChannelId(channelId);

            if (workflow == null)
            {
                throw new GameNotStartedException();
            }
            else if (userId != workflow.ControllingUserId)
            {
                throw new WorkflowException("It's <@" + workflow.ControllingUserId + ">'s turn; only he/she can identify an incorrect answer.");
            }
            else if (workflow.Stage != WorkflowStage.QUESTION_ASKED)
            {
                throw new WorkflowException("A question has not yet been submitted. Please ask a question before identifying an incorrect answer.");
            }
            else if (!workflow.Answers.Any(answer => answer.UserId == incorrectUserId))
            {
                throw new WorkflowException("User <@" + incorrectUserId + "> either doesn't exist or has not answered this question yet.");
            }
        }

        public void OnCorrectAnswerSelected(string channelId, string userId)
        {
            if (channelId == null || userId == null)
            {
                return;
            }

            Workflow workflow = _workflowRepository.FindByChannelId(channelId);

            if (workflow == null)
            {
                throw new GameNotStartedException();
            }
            else if (userId != workflow.ControllingUserId)
            {
                throw new WorkflowException("It's <@" + workflow.ControllingUserId + ">'s turn; only he/she can mark an answer correct.");
            }
            else if (workflow.Stage != WorkflowStage.QUESTION_ASKED)
            {
                throw new WorkflowException("A question has not yet been submitted. Please ask a question before marking an answer correct.");
            }
        }

        public void OnTurnChanged(string channelId, string userId, string newControllingUserId)
        {
            if (channelId == null || userId == null || newControllingUserId == null)
            {
                return;
            }

            Workflow workflow = _workflowRepository.FindByChannelId(channelId);

            if (workflow == null)
            {
                throw new GameNotStartedException();
            }
            else if (userId != workflow.ControllingUserId)
            {
                throw new WorkflowException("It's <@" + workflow.ControllingUserId + ">'s turn; only he/she can cede his/her turn.");
            }

            workflow.ControllingUserId = newControllingUserId;
            workflow.Question = null;
            workflow.Answers = new List<Answer>();
            workflow.Stage = WorkflowStage.STARTED;
            _workflowRepository.Save(workflow);
        }

        public GameState GetCurrentGameState(string channelId)
        {
            if (channelId == null)
            {
                return null;
            }

            GameState gameState = new GameState();
            Workflow workflow = _workflowRepository.FindByChannelId(channelId);

            if (workflow == null)
            {
                return gameState;
            }

            gameState.ControllingUserId = workflow.ControllingUserId;
            gameState.Topic = workflow.Topic;

            if (workflow.Stage == WorkflowStage.QUESTION_ASKED)
            {
                gameState.Question = workflow.Question;
                gameState.Answers = workflow.Answers
                        .Select(answer =>
                            new GameState.Answer
                            {
                                UserId = answer.UserId,
                                Username = answer.Username,
                                Text = answer.Text,
                                CreatedDate = answer.CreatedDate
                            }
                        )
                        .ToList();
            }

            return gameState;
        }
    }
}
