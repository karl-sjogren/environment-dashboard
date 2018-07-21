using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace EnvironmentDashboard.Api.Models {
    public class SensorTemperatureValue : SensorValue {
        public double Temperature { get; set; }
        public double Humidity { get; set; }
    }
}