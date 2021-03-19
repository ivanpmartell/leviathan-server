using System;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace leviathan_server
{
    class Database
    {
        public IMongoDatabase leviathan { get; }
        public Database()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            leviathan = client.GetDatabase("leviathan");
            CreateUserIndexAsync();
        }

        private async Task CreateUserIndexAsync()
        {
            var collection = leviathan.GetCollection<User>("users");
            var options = new CreateIndexOptions() { Unique = true };
            var emailField = new StringFieldDefinition<User>("Email");
            var usernameField = new StringFieldDefinition<User>("Username");
            var usernameIndexDefinition = new IndexKeysDefinitionBuilder<User>().Ascending(usernameField);
            var emailIndexDefinition = new IndexKeysDefinitionBuilder<User>().Ascending(emailField);

            var usernameIndexModel = new CreateIndexModel<User>(usernameIndexDefinition,options);
            var emailIndexModel = new CreateIndexModel<User>(emailIndexDefinition,options);
            await leviathan.GetCollection<User>("users").Indexes.CreateOneAsync(usernameIndexModel);
            await leviathan.GetCollection<User>("users").Indexes.CreateOneAsync(emailIndexModel);
        }
    }
}