using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TriviaGame.Models;

namespace TriviaGame.Repositories
{
    public interface IWorkflowRepository
    {
        Workflow FindByChannelId(string channelId);
        Workflow Save(Workflow workflow);
        void DeleteById(ObjectId id);
    }
}
