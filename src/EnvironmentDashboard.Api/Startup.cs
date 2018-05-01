using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnvironmentDashboard.Api.Authentication;
using EnvironmentDashboard.Api.Contracts;
using EnvironmentDashboard.Api.Middlewares;
using EnvironmentDashboard.Api.Stores;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace EnvironmentDashboard.Api {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            services.AddMvc();

            services.AddResponseCompression(options => {
                options.Providers.Add<GzipCompressionProvider>();
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "image/svg+xml" });
            });
            
            var mongoClient = new MongoClient(Configuration["MONGODB_URI"]);
            services.AddSingleton<IMongoClient>(mongoClient);

            services.AddScoped<IApiKeyStore, ApiKeyStore>();
            services.AddScoped<IUserStore, UserStore>();

            // The initializers should be executed in order
            services.AddSingleton<IInitializer, AutoMapperInitializer>();
            services.AddSingleton<IInitializer, MongoDbInitializer>();
            services.AddSingleton<IInitializer, UserInitializer>();

            var serviceProvider = services.BuildServiceProvider();
            
            foreach(IInitializer initializer in serviceProvider.GetServices(typeof(IInitializer)))
                initializer.Initialize(services);

            services.AddAuthentication(options => {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            });

            services.AddAuthorization(options => {
                options.AddPolicy("AdminUser", policy => policy.RequireClaim("AdminUser"));
                options.AddPolicy("ApiUser", policy => policy.RequireClaim("ApiUser"));
            });

            services.AddScheme<JwtBearerOptions, JwtAuthenticationHandler>(JwtBearerDefaults.AuthenticationScheme, "Environment Dashboard", x => new JwtBearerOptions());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
            app.UseResponseCompression();

            if(env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();

                // Serves index.html from wwwroot
                app.UseDefaultFiles();
                app.Use(async (context, next) => {
                    await next();
                    
                    if(context.Request.Method != "GET")
                        return;
                    
                    if(context.Response.StatusCode != 404)
                        return;
                    
                    if(context.Request.Path.ToString().Contains("admin/"))
                        return;

                    // Rewrite to index.html and execute again
                    context.Request.Path = new PathString("/index.html");

                    await next();
                });
            }

            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseMvc();

            if(env.IsDevelopment()) {
                app.UseReverseProxy("http", "localhost", 4200);
            }
        }
    }
}
