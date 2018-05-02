using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace EnvironmentDashboard.Api.Models {
    public class Sensor {
        public string Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public string Name { get; set; }
        public string Type  { get; set; }
    }
}