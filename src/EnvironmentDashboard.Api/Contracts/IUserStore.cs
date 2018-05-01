using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnvironmentDashboard.Api.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace EnvironmentDashboard.Api.Contracts {
    public interface IUserStore {
        Task<Int32> Count();
        Task<User> GetById(string id);
        Task<ICollection<User>> GetById(string[] ids);
        Task<PaginatedResult<User>> GetPaged(Int32 pageIndex, Int32 pageSize);
        Task<User> Save(User user);
        Task Delete(string id);

        Task<User> Authenticate(string username, string password);
        Task<User> SetPassword(string id, string password);
    }
}