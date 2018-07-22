using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnvironmentDashboard.Api.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace EnvironmentDashboard.Api.Contracts {
    public interface ISensorStore {
        Task<Int32> Count();
        Task<Sensor> GetById(string id);
        Task<ICollection<Sensor>> GetById(string[] ids);
        Task<PaginatedResult<Sensor>> GetPaged(Int32 pageIndex, Int32 pageSize);
        Task<Sensor> Save(Sensor sensor);
        Task Delete(string id);
        Task<PaginatedResult<SensorValue>> GetValues(string id, Int32 pageIndex, Int32 pageSize);
        Task SaveValue(SensorValue value);
        Task DeleteValues(string sensorId);
    }
}