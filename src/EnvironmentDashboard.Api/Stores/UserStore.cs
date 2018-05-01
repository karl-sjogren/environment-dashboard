using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using EnvironmentDashboard.Api.Contracts;
using EnvironmentDashboard.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace EnvironmentDashboard.Api.Stores {
    public class UserStore : MongoDBStore, IUserStore {
        public UserStore(IMongoClient client, IConfiguration configuration, ILogger<UserStore> logger) : base(client, configuration, logger) { }

        public async Task<Int32> Count() {
            var collection = Database.GetCollection<User>("users");
            return await collection.AsQueryable().CountAsync();
        }

        public async Task<User> GetById(string id) {
            var collection = Database.GetCollection<User>("users");
            return await collection.AsQueryable().Where(a => a.Id == id).FirstOrDefaultAsync();
        }

        public async Task<ICollection<User>> GetById(string[] ids) {
            var collection = Database.GetCollection<User>("users");
            return await collection.AsQueryable().Where(a => ids.Contains(a.Id)).ToListAsync();
        }

        public async Task<PaginatedResult<User>> GetPaged(Int32 pageIndex, Int32 pageSize) {
            var collection = Database.GetCollection<User>("users");
            var items = await collection.AsQueryable().Skip(pageIndex * pageSize).Take(pageSize).ToListAsync();
            var count = await collection.AsQueryable().CountAsync();
            return new PaginatedResult<User>(items, pageIndex, pageSize, count);
        }

        public async Task<User> Save(User user) {
            var existingUser = await GetById(user.Id);
            if(existingUser != null) {
                user.Created = existingUser.Created;
                user.LastLogin = existingUser.LastLogin;
                user.PasswordHash = existingUser.PasswordHash;
            }

            user.Email = user.Email?.ToLowerInvariant(); // Always store lowercase email address

            var collection = Database.GetCollection<User>("users");
            if(user.Id == null) {
                user.Created = user.Modified = DateTime.Now;
                await collection.InsertOneAsync(user);
                return await GetById(user.Id);
            } else {
                user.Modified = DateTime.Now;
                var result = await collection.ReplaceOneAsync(
                    filter: new BsonDocument("_id", user.Id),
                    replacement: user
                );
                return await GetById(user.Id);
            }
        }

        public async Task Delete(string id) {
            var collection = Database.GetCollection<User>("users");
            await collection.DeleteOneAsync(new BsonDocument("_id", id));
        }

        public async Task<User> Authenticate(string username, string password) {
            var collection = Database.GetCollection<User>("users");
            
            username = username?.ToLowerInvariant(); // Always use lowercase email address

            var user = await collection.AsQueryable().Where(a => a.Email == username).FirstOrDefaultAsync();
            if(user == null)
                return null;

            var newHash = CalculatePasswordHash(password, user.Id);
            if(user.PasswordHash != newHash)
                return null;

            user.LastLogin = DateTime.Now;

            return await Save(user);
        }

        public async Task<User> SetPassword(string id, string password) {
            var collection = Database.GetCollection<User>("users");
            
            var user = await GetById(id);
            if(user == null)
                return null;
            
            Logger.LogInformation($"Updating password for user {user.Email}.");

            user.PasswordHash = CalculatePasswordHash(password, user.Id);
            
            var result = await collection.ReplaceOneAsync(
                filter: new BsonDocument("_id", user.Id),
                replacement: user
            );

            return await GetById(user.Id);
        }

        private static string CalculatePasswordHash(string password, string salt) {
            byte[] saltBytes = Encoding.ASCII.GetBytes(salt);
            byte[] derived;
            using(var pbkdf2 = new Rfc2898DeriveBytes(
                Encoding.UTF8.GetBytes(password),
                saltBytes,
                1000,
                HashAlgorithmName.SHA256)) {
                derived = pbkdf2.GetBytes(32);
            }

            return Convert.ToBase64String(derived);
        }
    }
}