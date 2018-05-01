using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EnvironmentDashboard.Api.Models {
    public class User {
        public string Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public DateTime? LastLogin { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Email { get; set; }
        
        [JsonIgnore] // This should not be seralized and sent outside
        public string PasswordHash { get; set; }
    }
}