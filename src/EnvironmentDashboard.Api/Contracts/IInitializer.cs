using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace EnvironmentDashboard.Api.Contracts {
    public interface IInitializer {
        Task Initialize(IServiceCollection serviceCollection);
    }
}