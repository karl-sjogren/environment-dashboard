using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnvironmentDashboard.Api.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace EnvironmentDashboard.Api.Stores {
    public abstract class MongoDBStore {
        protected readonly IMongoClient Client;
        protected readonly IMongoDatabase Database;
        protected readonly ILogger Logger;
        protected readonly MongoDbOptions Options;

        protected MongoDBStore(IMongoClient client, IOptions<MongoDbOptions> optionsAccessor, ILogger logger) {
            Client = client;
            Options = optionsAccessor.Value;
            Logger = logger;

            var databaseName = MongoUrl.Create(Options.MongoDbUri).DatabaseName;
            Database = client.GetDatabase(databaseName);
        }
    }
}