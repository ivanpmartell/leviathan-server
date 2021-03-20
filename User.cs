using System;
using MongoDB.Bson;
using MongoDB.Driver;

namespace leviathan_server
{
    public class User
    {
        public ObjectId Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
    }
}