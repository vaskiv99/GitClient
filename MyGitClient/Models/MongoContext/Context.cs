using MongoDB.Driver;
using MyGitClient.Models;
namespace MyGitClient.MongoContext
{
    public class MongoDbContext
    {
        private static readonly IMongoDatabase _database;

        public IMongoCollection<Merge> Merges =>
           _database.GetCollection<Merge>("Merges");

        public IMongoCollection<Repository> Repositories =>
           _database.GetCollection<Repository>("Repositories");

        static MongoDbContext()
        {
            var connectionString = "mongodb://localhost:27017";
            _database = new MongoClient(connectionString).GetDatabase("GitDb");
        }
    }
}
