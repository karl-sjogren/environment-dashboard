using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnvironmentDashboard.Api.ActionFilters;
using EnvironmentDashboard.Api.Contracts;
using EnvironmentDashboard.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnvironmentDashboard.Api.Controllers {
    [Route("admin/api/sensors/")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class SensorController : Controller {
        private readonly ISensorStore _sensorStore;

        public SensorController(ISensorStore sensorStore) {
            _sensorStore = sensorStore;
        }

        [Authorize(Policy = "AdminUser")]
        [HttpGet]
        public async Task<IActionResult> ListSensors(Int32 pageIndex = 0, Int32 pageSize = 100) {
            return Json(await _sensorStore.GetPaged(pageIndex, pageSize));
        }

        [Authorize(Policy = "AdminUser")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSensor([FromRoute] string id) {
            var sensor = await _sensorStore.GetById(id);

            if(sensor == null)
                return NotFound();

            return Json(sensor);
        }

        [Authorize(Policy = "AdminUser")]
        [HttpPost]
        public async Task<IActionResult> CreateSensor([FromBody] Sensor sensor) {
            var result = await _sensorStore.Save(sensor);

            return Json(result);
        }

        [Authorize(Policy = "AdminUser")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSensor([FromRoute] string id, [FromBody] Sensor sensor) {
            if(id != sensor?.Id)
                return BadRequest();

            var result = await _sensorStore.Save(sensor);

            return Json(result);
        }

        [Authorize(Policy = "AdminUser")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSensor([FromRoute] string id) {
            await _sensorStore.DeleteValues(id);
            await _sensorStore.Delete(id);
            return NoContent();
        }

        [Authorize(Policy = "AdminUser")]
        [HttpGet("{id}/values")]
        public async Task<IActionResult> ListSensorValues([FromRoute] string id, Int32 pageIndex = 0, Int32 pageSize = 100) {
            return Json(await _sensorStore.GetValues(id, pageIndex, pageSize));
        }

        [Authorize(Policy = "ApiUser")]
        [HttpPost("{id}")]
        [SensorValueFilter]
        public async Task<IActionResult> StoreSensorValue([FromRoute] string id, [FromBody] SensorValue sensorValue) {
            await _sensorStore.SaveValue(sensorValue);

            return NoContent();
        }
    }
}
