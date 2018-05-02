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
    public class SensorStore : MongoDBStore, ISensorStore {
        public SensorStore(IMongoClient client, IOptions<MongoDbOptions> optionsAccessor, ILogger<SensorStore> logger) : base(client, optionsAccessor, logger) { }

        public async Task<Int32> Count() {
            var collection = Database.GetCollection<Sensor>("sensors");
            return await collection.AsQueryable().CountAsync();
        }

        public async Task<Sensor> GetById(string id) {
            var collection = Database.GetCollection<Sensor>("sensors");
            return await collection.AsQueryable().Where(a => a.Id == id).FirstOrDefaultAsync();
        }

        public async Task<ICollection<Sensor>> GetById(string[] ids) {
            var collection = Database.GetCollection<Sensor>("sensors");
            return await collection.AsQueryable().Where(a => ids.Contains(a.Id)).ToListAsync();
        }

        public async Task<PaginatedResult<Sensor>> GetPaged(Int32 pageIndex, Int32 pageSize) {
            var collection = Database.GetCollection<Sensor>("sensors");
            var items = await collection.AsQueryable().Skip(pageIndex * pageSize).Take(pageSize).ToListAsync();
            var count = await collection.AsQueryable().CountAsync();
            return new PaginatedResult<Sensor>(items, pageIndex, pageSize, count);
        }

        public async Task<Sensor> Save(Sensor sensor) {
            var existingSensor = await GetById(sensor.Id);
            if(existingSensor != null) {
                sensor.Created = existingSensor.Created;
            }

            var collection = Database.GetCollection<Sensor>("sensors");
            if(sensor.Id == null) {
                Logger.LogInformation($"Creating new sensor {sensor.Name}.");
                sensor.Created = sensor.Modified = DateTime.Now;
                await collection.InsertOneAsync(sensor);
                return await GetById(sensor.Id);
            } else {
                Logger.LogInformation($"Updating existing sensor {sensor.Name}.");
                sensor.Modified = DateTime.Now;
                var result = await collection.ReplaceOneAsync(
                    filter: new BsonDocument("_id", sensor.Id),
                    replacement: sensor
                );
                return await GetById(sensor.Id);
            }
        }

        public async Task Delete(string id) {
            Logger.LogInformation($"Removing sensor with id {id}.");
            var collection = Database.GetCollection<Sensor>("sensors");
            await collection.DeleteOneAsync(new BsonDocument("_id", id));
        }
    }
}