using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace TriviaGame.Models
{
    [BsonIgnoreExtraElements]
    public class Workflow
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("channelId")]
        public string ChannelId { get; set; }

        [BsonElement("controllingUserId")]
        public string ControllingUserId { get; set; }

        [BsonElement("topic")]
        public string Topic { get; set; }

        [BsonElement("question")]
        public string Question { get; set; }

        [BsonElement("answers")]
        public List<Answer> Answers { get; set; } = new List<Answer>();

        [BsonElement("stage")]
        public WorkflowStage Stage { get; set; }
    }
}
