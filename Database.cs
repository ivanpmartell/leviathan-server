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
            CreateUserIndexesAsync();
        }

        private async void CreateUserIndexesAsync()
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

        public async Task<User> UserLogin(string user, string pass)
        {
            var collection = leviathan.GetCollection<User>("users");
            FilterDefinition<User> usernameFilter = Builders<User>.Filter.Eq(user => user.Username, user);
            FilterDefinition<User> passwordFilter = Builders<User>.Filter.Eq(user => user.Password, pass);
            var filter = Builders<User>.Filter.And(usernameFilter, passwordFilter);
            var doc = await collection.Find(filter).FirstOrDefaultAsync();
            return doc;
        }

        public async Task CreateUser(string user, string pass, string email)
        {
            var collection = leviathan.GetCollection<User>("users");
            await collection.InsertOneAsync(
                new User { 
                    Username = user,
                    Password = pass,
                    Email = email,
                    Token = ""});
        }
    }
}