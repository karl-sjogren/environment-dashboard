
using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using EnvironmentDashboard.Api.Contracts;
using EnvironmentDashboard.Api.Models;
using EnvironmentDashboard.Api.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;

namespace EnvironmentDashboard.Api.Stores {
    public class MongoDbInitializer : IInitializer {
        private readonly IMongoClient _client;
        protected readonly IMongoDatabase _database;

        public MongoDbInitializer(IMongoClient client, IOptions<MongoDbOptions> optionsAccessor) {
            _client = client;

            var databaseName = MongoUrl.Create(optionsAccessor.Value.MongoDbUri).DatabaseName;
            _database = client.GetDatabase(databaseName);
        }

        public async Task Initialize(IServiceCollection serviceCollection) {
            BsonClassMap.RegisterClassMap<ApiKey>(cm => {
                cm.AutoMap();
                cm.MapIdMember(c => c.Id).SetIdGenerator(StringObjectIdGenerator.Instance);
            });

            BsonClassMap.RegisterClassMap<Sensor>(cm => {
                cm.AutoMap();
                cm.MapIdMember(c => c.Id).SetIdGenerator(StringObjectIdGenerator.Instance);
            });

            BsonClassMap.RegisterClassMap<SensorValue>(cm => {
                cm.AutoMap();
                cm.SetIsRootClass(true);
                cm.MapIdMember(c => c.Id).SetIdGenerator(StringObjectIdGenerator.Instance);
            });

            BsonClassMap.RegisterClassMap<SensorTemperatureValue>(cm => {
                cm.AutoMap();
            });

            BsonClassMap.RegisterClassMap<Camera>(cm => {
                cm.AutoMap();
                cm.MapIdMember(c => c.Id).SetIdGenerator(StringObjectIdGenerator.Instance);
            });

            BsonClassMap.RegisterClassMap<User>(cm => {
                cm.AutoMap();
                cm.MapIdMember(c => c.Id).SetIdGenerator(StringObjectIdGenerator.Instance);
            });

            {
                var collection = _database.GetCollection<SensorValue>("sensor-values");
                var indicies = await (await collection.Indexes.ListAsync()).ToListAsync();
                if(!indicies.Any(i => i["name"] == "SensorId_Created")) {
                    var keysBuilder = new IndexKeysDefinitionBuilder<SensorValue>();

                    var keys = keysBuilder
                                .Descending(b => b.SensorId)
                                .Descending(b => b.Created);
                    
                    var options = new CreateIndexOptions {
                        Name = "SensorId_Created"
                    };

                    await collection.Indexes.CreateOneAsync(keys, options);
                }
            }
        }
    }
}
