using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using MongoDBApp.Models.db;

namespace MongoDBApp.Models
{
    public class UsersService
    {
        private IGridFSBucket gridFs;
        private IMongoCollection<User> Users;

        public UsersService()
        {
            string connectionString = "mongodb://127.0.0.1:27017/test";
            var connection = new MongoUrlBuilder(connectionString);
            var client = new MongoClient(connectionString);
            IMongoDatabase db = client.GetDatabase(connection.DatabaseName);
            gridFs = new GridFSBucket(db);

            Users = db.GetCollection<User>("users");
        }

        public async Task<IEnumerable<User>> GetUsers(string name)
        {
            var filterBuilder = new FilterDefinitionBuilder<User>();
            FilterDefinition<User> filter = filterBuilder.Empty;

            if (!string.IsNullOrEmpty(name))
            {
                filter &= filterBuilder.Eq("name", name);
            }

            return await Users.Find(filter).ToListAsync();
        }

        public async Task<User> GetUser(string id)
        {
            return await Users.Find(new BsonDocument("_id", new ObjectId(id))).FirstOrDefaultAsync();
        }

        public async Task Create(User user)
        {
            await Users.InsertOneAsync(user);
        }

        public async Task Update(User user)
        {
            await Users.ReplaceOneAsync(new BsonDocument("_id", new ObjectId(user.Id)), user);
        }

        public async Task Remove(string id)
        {
            await Users.DeleteOneAsync(new BsonDocument("_id", new ObjectId(id)));
        }
    }
}
