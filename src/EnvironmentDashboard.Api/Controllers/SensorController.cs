using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnvironmentDashboard.Api.Contracts;
using EnvironmentDashboard.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnvironmentDashboard.Api.Controllers {
    [Authorize(Policy = "AdminUser")]
    [Route("admin/api/sensors/")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class SensorController : Controller {
        private readonly ISensorStore _sensorStore;

        public SensorController(ISensorStore sensorStore) {
            _sensorStore = sensorStore;
        }

        [HttpGet]
        public async Task<IActionResult> ListSensors() {
            return Json(await _sensorStore.GetPaged(0, 100));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSensor([FromRoute] string id) {
            var user = await _sensorStore.GetById(id);

            if(user == null)
                return NotFound();

            return Json(user);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSensor([FromBody] Sensor sensor) {
            var result = await _sensorStore.Save(sensor);

            return Json(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSensor([FromRoute] string id, [FromBody] Sensor sensor) {
            if(id != sensor?.Id)
                return BadRequest();

            var result = await _sensorStore.Save(sensor);

            return Json(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSensor([FromRoute] string id) {
            await _sensorStore.DeleteValues(id);
            await _sensorStore.Delete(id);
            return NoContent();
        }
    }
}
