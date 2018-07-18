using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnvironmentDashboard.Api.Contracts;
using EnvironmentDashboard.Api.Models;
using EnvironmentDashboard.Api.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace EnvironmentDashboard.Api.Stores {
    public class CameraStore : MongoDBStore, ICameraStore {
        public CameraStore(IMongoClient client, IOptions<MongoDbOptions> optionsAccessor, ILogger<CameraStore> logger) : base(client, optionsAccessor, logger) { }

        public async Task<Int32> Count() {
            var collection = Database.GetCollection<Camera>("cameras");
            return await collection.AsQueryable().CountAsync();
        }

        public async Task<Camera> GetById(string id) {
            var collection = Database.GetCollection<Camera>("cameras");
            return await collection.AsQueryable().Where(a => a.Id == id).FirstOrDefaultAsync();
        }

        public async Task<ICollection<Camera>> GetById(string[] ids) {
            var collection = Database.GetCollection<Camera>("cameras");
            return await collection.AsQueryable().Where(a => ids.Contains(a.Id)).ToListAsync();
        }

        public async Task<PaginatedResult<Camera>> GetPaged(Int32 pageIndex, Int32 pageSize) {
            var collection = Database.GetCollection<Camera>("cameras");
            var items = await collection.AsQueryable().Skip(pageIndex * pageSize).Take(pageSize).ToListAsync();
            var count = await collection.AsQueryable().CountAsync();
            return new PaginatedResult<Camera>(items, pageIndex, pageSize, count);
        }

        public async Task<Camera> Save(Camera camera) {
            var existingCamera = await GetById(camera.Id);
            if(existingCamera != null) {
                camera.Created = existingCamera.Created;
            }

            var collection = Database.GetCollection<Camera>("cameras");
            if(camera.Id == null) {
                Logger.LogInformation($"Creating new camera {camera.Name}.");
                camera.Created = camera.Modified = DateTime.Now;
                await collection.InsertOneAsync(camera);
                return await GetById(camera.Id);
            } else {
                Logger.LogInformation($"Updating existing camera {camera.Name}.");
                camera.Modified = DateTime.Now;
                var result = await collection.ReplaceOneAsync(
                    filter: new BsonDocument("_id", camera.Id),
                    replacement: camera
                );
                return await GetById(camera.Id);
            }
        }

        public async Task Delete(string id) {
            Logger.LogInformation($"Removing camera with id {id}.");
            var collection = Database.GetCollection<Camera>("cameras");
            await collection.DeleteOneAsync(new BsonDocument("_id", id));
        }
    }
}