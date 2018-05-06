using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnvironmentDashboard.Api.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace EnvironmentDashboard.Api.Contracts {
    public interface IApiKeyService {
        Task<string> GenerateApiToken(string apiKeyId);
    }
}