using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TriviaGame.Repositories
{
    public class TriviaGameDbContext : ITriviaGameDbContext
    {
        private const string MONGODB_CONNECTION_STRING_ENVIRONMENT_VARIABLE = "MONGODB_CONNECTION_STRING";
        private const string MONGODB_DATABASE_ENVIRONMENT_VARIABLE = "MONGODB_DATABASE";

        private readonly IMongoDatabase _db;

        public TriviaGameDbContext()
        {
            var client = new MongoClient(Environment.GetEnvironmentVariable(MONGODB_CONNECTION_STRING_ENVIRONMENT_VARIABLE));
            _db = client.GetDatabase(Environment.GetEnvironmentVariable(MONGODB_DATABASE_ENVIRONMENT_VARIABLE));
        }

        public IMongoDatabase GetDb()
        {
            return _db;
        }
    }
}
