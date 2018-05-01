using System;
using System.Collections.Generic;
using System.Linq;
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
    public class ApiKeyStore : MongoDBStore, IApiKeyStore {
        public ApiKeyStore(IMongoClient client, IConfiguration configuration, ILogger<ApiKeyStore> logger) : base(client, configuration, logger) { }

        public async Task<Int32> Count() {
            var collection = Database.GetCollection<ApiKey>("api-keys");
            return await collection.AsQueryable().CountAsync();
        }

        public async Task<ApiKey> GetById(string id) {
            var collection = Database.GetCollection<ApiKey>("api-keys");
            return await collection.AsQueryable().Where(a => a.Id == id).FirstOrDefaultAsync();
        }

        public async Task<ICollection<ApiKey>> GetById(string[] ids) {
            var collection = Database.GetCollection<ApiKey>("api-keys");
            return await collection.AsQueryable().Where(a => ids.Contains(a.Id)).ToListAsync();
        }

        public async Task<PaginatedResult<ApiKey>> GetPaged(Int32 pageIndex, Int32 pageSize) {
            var collection = Database.GetCollection<ApiKey>("api-keys");
            var items = await collection.AsQueryable().Skip(pageIndex * pageSize).Take(pageSize).ToListAsync();
            var count = await collection.AsQueryable().CountAsync();
            return new PaginatedResult<ApiKey>(items, pageIndex, pageSize, count);
        }

        public async Task<ApiKey> Save(ApiKey apiKey) {
            var existingApiKey = await GetById(apiKey.Id);
            if(existingApiKey != null) {
                apiKey.Created = existingApiKey.Created;
                apiKey.LastAccess = existingApiKey.LastAccess;
                apiKey.RequestCounter = existingApiKey.RequestCounter;
            }

            var collection = Database.GetCollection<ApiKey>("api-keys");
            if(apiKey.Id == null) {
                Logger.LogInformation($"Creating new api-key for contact {apiKey.FirstName} {apiKey.LastName}.");
                apiKey.Created = apiKey.Modified = DateTime.Now;
                await collection.InsertOneAsync(apiKey);
                return await GetById(apiKey.Id);
            } else {
                Logger.LogInformation($"Updating existing api-key for contact {apiKey.FirstName} {apiKey.LastName}.");
                apiKey.Modified = DateTime.Now;
                var result = await collection.ReplaceOneAsync(
                    filter: new BsonDocument("_id", apiKey.Id),
                    replacement: apiKey
                );
                return await GetById(apiKey.Id);
            }
        }

        public async Task Delete(string id) {
            Logger.LogInformation($"Removing api-key with id {id}.");
            var collection = Database.GetCollection<ApiKey>("api-keys");
            await collection.DeleteOneAsync(new BsonDocument("_id", id));
        }

        public async Task IncrementReuests(string id) {
            var collection = Database.GetCollection<ApiKey>("api-keys");
            
            await collection.UpdateOneAsync(
                filter: new BsonDocument("_id", id),
                update: new BsonDocument {
                    {
                        "$set", new BsonDocument {
                            { "LastAccess",  DateTime.Now }
                        }
                    },
                    { 
                        "$inc", new BsonDocument {
                            { "RequestCounter",  1 }
                        }
                    }
                }
            );
        }
    }
}