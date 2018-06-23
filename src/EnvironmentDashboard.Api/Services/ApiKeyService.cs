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
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Microsoft.Extensions.Options;
using EnvironmentDashboard.Api.Options;

namespace EnvironmentDashboard.Api.Services {
    public class ApiKeyService : IApiKeyService {
        private readonly IApiKeyStore _apiKeyStore;
        private readonly MongoDbOptions _mongoDbOptions;
        private readonly SmtpOptions _smtpOptions;

        public ApiKeyService(IApiKeyStore apiKeyStore, IOptions<MongoDbOptions> mongoDbOptionsAccessor, IOptions<SmtpOptions> smtpOptionsAccessor) {
            _apiKeyStore = apiKeyStore;
            _mongoDbOptions = mongoDbOptionsAccessor.Value;
            _smtpOptions = smtpOptionsAccessor.Value;
        }

        public async Task SendApiKey(string apiKeyId, string currentBaseUrl) {
            var apiKey = await _apiKeyStore.GetById(apiKeyId);

            if(apiKey == null)
                return;

			var message = new MimeMessage();
			message.From.Add(new MailboxAddress("Environment Dashboard", _smtpOptions.Sender));
			message.To.Add(new MailboxAddress(apiKey.Name, apiKey.Email));
			message.Subject = $"API key from the Environment Dashboard";

            var docUri = new Uri(new Uri(currentBaseUrl), "/swagger/");
            var apiToken = await GenerateApiToken(apiKey.Id);

			message.Body = new TextPart("plain") {
				Text = $@"Hi {apiKey.Email},

You've requested an api-key to the Environment Dashboard. Your key can be seen below.

{apiToken}

API documentation can be found at {docUri.ToString()}.

The information we have stored for this API key is the following:

Name: {apiKey.Name}
Email: {apiKey.Email}

-- Environment Dashboard"
			};

			using(var client = new SmtpClient()) {
				client.Connect(_smtpOptions.Host, _smtpOptions.Port, true);
                await client.AuthenticateAsync(_smtpOptions.Username, _smtpOptions.Password);

				client.Send(message);
				client.Disconnect(true);
			}
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