using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TriviaGame.Models
{
    [BsonIgnoreExtraElements]
    public class ScoreInfo
    {
        [BsonId]
        public ObjectId Id { get; set; }

        [BsonElement("channelId")]
        public string ChannelId { get; set; }

        [BsonElement("userId")]
        public string UserId { get; set; }

        [BsonElement("username")]
        public string Username { get; set; }

        [BsonElement("score")]
        public long Score { get; set; }
    }
}
