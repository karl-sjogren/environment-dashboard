using System;
using System.Threading.Tasks;
using AutoMapper;
using EnvironmentDashboard.Api.Contracts;
using EnvironmentDashboard.Api.Models;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization.IdGenerators;

namespace EnvironmentDashboard.Api.Stores {
    public class UserInitializer : IInitializer {
        private readonly IUserStore _userStore;

        public UserInitializer(IUserStore userStore) {
            _userStore = userStore;
        }

        public async Task Initialize(IServiceCollection serviceCollection) {
            var userCount = await _userStore.Count();
            if(userCount > 0)
                return;

            Console.WriteLine("Creating default user with configured password.");

            var user = new User {
                FirstName = "Randy",
                LastName = "Randleman",
                Email = "randy.randleman@example.com",
                Created = DateTime.Now,
                Modified = DateTime.Now
            };

            user = await _userStore.Save(user);
            var newUser = await _userStore.SetPassword(user.Id, "configured password");

            if(newUser == null)
                throw new Exception("Failed to create default user.");
        }
    }
}