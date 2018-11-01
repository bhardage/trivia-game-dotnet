using MongoDB.Bson.Serialization.Attributes;
using System;

namespace TriviaGame.Models
{
    public class Answer
    {
        [BsonElement("userId")]
        public string UserId { get; set; }

        [BsonElement("username")]
        public string Username { get; set; }

        [BsonElement("text")]
        public string Text { get; set; }

        [BsonDateTimeOptions]
        [BsonElement("createdDate")]
        public DateTime CreatedDate { get; set; }
    }
}
