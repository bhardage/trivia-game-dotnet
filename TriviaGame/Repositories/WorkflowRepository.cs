using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq;
using TriviaGame.Models;

namespace TriviaGame.Repositories
{
    public class WorkflowRepository : IWorkflowRepository
    {
        private readonly ITriviaGameDbContext _triviaGameDbContext;

        public WorkflowRepository(ITriviaGameDbContext triviaGameDbContext)
        {
            _triviaGameDbContext = triviaGameDbContext;
        }

        public Workflow FindByChannelId(string channelId)
        {
            FilterDefinition<Workflow> filter = Builders<Workflow>.Filter
                .Eq(w => w.ChannelId, channelId);

            return getWorkflows()
                .Find(filter)
                .FirstOrDefault();
        }

        public Workflow Save(Workflow workflow)
        {
            if (workflow.Id == ObjectId.Empty)
            {
                getWorkflows()
                    .InsertOne(workflow);
            }
            else
            {
                getWorkflows()
                    .ReplaceOne(w => w.Id == workflow.Id, workflow);

                //TODO throw an exception if nothing was updated?
            }

            return workflow;
        }

        public void DeleteById(ObjectId id)
        {
            FilterDefinition<Workflow> filter = Builders<Workflow>.Filter
                .Eq(w => w.Id, id);

            getWorkflows()
                .DeleteOne(filter);
        }

        private IMongoCollection<Workflow> getWorkflows()
        {
            return _triviaGameDbContext.GetDb().GetCollection<Workflow>("workflow");
        }
    }
}
