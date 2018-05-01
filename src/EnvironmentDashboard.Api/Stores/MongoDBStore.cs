using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace EnvironmentDashboard.Api.Stores {
    public abstract class MongoDBStore {
        protected readonly IMongoClient Client;
        protected readonly IMongoDatabase Database;
        protected readonly ILogger Logger;

        protected MongoDBStore(IMongoClient client, IConfiguration configuration, ILogger logger) {
            Client = client;
            Logger = logger;

            var databaseName = MongoUrl.Create(configuration["MONGODB_URI"]).DatabaseName;
            Database = client.GetDatabase(databaseName);
        }
    }
}