using System.Threading.Tasks;
using AutoMapper;
using EnvironmentDashboard.Api.Contracts;
using EnvironmentDashboard.Api.Models;
using Microsoft.Extensions.DependencyInjection;

namespace EnvironmentDashboard.Api.Stores {
    public class AutoMapperInitializer : IInitializer {
        public Task Initialize(IServiceCollection serviceCollection) {
            var config = new MapperConfiguration(cfg => {
                // Nothing yet
            });

            var mapper = new Mapper(config);
            serviceCollection.AddSingleton<IMapper>(mapper);

            return Task.FromResult(0);
        }
    }
}