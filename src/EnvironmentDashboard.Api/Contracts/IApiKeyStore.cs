using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnvironmentDashboard.Api.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace EnvironmentDashboard.Api.Contracts {
    public interface IApiKeyStore {
        Task<Int32> Count();
        Task<ApiKey> GetById(string id);
        Task<ICollection<ApiKey>> GetById(string[] ids);
        Task<PaginatedResult<ApiKey>> GetPaged(Int32 pageIndex, Int32 pageSize);
        Task<ApiKey> Save(ApiKey apiKey);
        Task Delete(string id);

        Task IncrementReuests(string id);
    }
}