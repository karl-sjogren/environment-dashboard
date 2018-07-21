using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace EnvironmentDashboard.Api.Models {
    public /*abstract*/ class SensorValue {
        public string Id { get; set; }
        public string SensorId { get; set; }
        public DateTime Created { get; set; }
    }
}