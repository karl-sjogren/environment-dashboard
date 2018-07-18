using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnvironmentDashboard.Api.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace EnvironmentDashboard.Api.Contracts {
    public interface ICameraStore {
        Task<Int32> Count();
        Task<Camera> GetById(string id);
        Task<ICollection<Camera>> GetById(string[] ids);
        Task<PaginatedResult<Camera>> GetPaged(Int32 pageIndex, Int32 pageSize);
        Task<Camera> Save(Camera camera);
        Task Delete(string id);
    }
}