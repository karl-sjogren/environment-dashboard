using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using EnvironmentDashboard.Api.Contracts;
using EnvironmentDashboard.Api.Models;
using AutoMapper;
using Jose;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Microsoft.Extensions.Options;
using EnvironmentDashboard.Api.Options;

namespace EnvironmentDashboard.Api.Services {
    public class ApiKeyService : IApiKeyService {
        private readonly IApiKeyStore _apiKeyStore;
        private readonly MongoDbOptions _mongoDbOptions;

        public ApiKeyService(IApiKeyStore apiKeyStore, IOptions<MongoDbOptions> optionsAccessor) {
            _apiKeyStore = apiKeyStore;
            _mongoDbOptions = optionsAccessor.Value;
        }

        public async Task<string> GenerateApiToken(string apiKeyId) {
            var apiKey = await _apiKeyStore.GetById(apiKeyId);

            if(apiKey == null)
                return null;

            var payload = new Dictionary<string, object>() {
                { "sub", apiKey.Id },
                { "iss", "AuthorDatabase" },
                { "claim", "ApiUser" }
            };

            var sha = SHA256Managed.Create();
            var privateKey = sha.ComputeHash(Encoding.UTF8.GetBytes(_mongoDbOptions.MongoDbUri));
            var token = Jose.JWT.Encode(payload, privateKey, JweAlgorithm.A256KW, JweEncryption.A256CBC_HS512);

            return token;
        }
    }
}