using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Logging;

namespace EnvironmentDashboard.Api {
    public class Program {
        public static void Main(string[] args) {
            var builder = new ConfigurationBuilder()
                .AddEnvironmentVariables(prefix: "ASPNETCORE_");

            if(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")?.ToLowerInvariant() == "development") {
                builder.AddUserSecrets<Startup>();
            }

            var config = builder.Build();

            var host = new WebHostBuilder()
                .UseConfiguration(config)
                .UseKestrel(options => {
                    options.Listen(IPAddress.Any, 5000);
                })
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureLogging((hostingContext, logging) => {
                    logging.AddConsole();
                })
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}