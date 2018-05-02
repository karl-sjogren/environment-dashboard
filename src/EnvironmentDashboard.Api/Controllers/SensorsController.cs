using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnvironmentDashboard.Api.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnvironmentDashboard.Api.Controllers {
    [Authorize(Policy = "AdminUser")]
    [Route("admin/api/sensors/")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class SensorsController : Controller {
        private readonly ISensorStore _sensorStore;

        public SensorsController(ISensorStore sensorStore) {
            _sensorStore = sensorStore;
        }

        [HttpGet]
        public IEnumerable<string> Get() {
            return new string[] { "value1", "value2" };
        }
    }
}
