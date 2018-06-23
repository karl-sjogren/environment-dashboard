using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace EnvironmentDashboard.Api.Models {
    public class ApiKey {
        public string Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public DateTime? LastAccess { get; set; }

        public long RequestCounter { get; set; }

        public string Name { get; set; }
        public string Email { get; set; }
    }
}