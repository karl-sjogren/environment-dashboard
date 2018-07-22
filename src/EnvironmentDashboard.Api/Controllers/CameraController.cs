using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EnvironmentDashboard.Api.Contracts;
using EnvironmentDashboard.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnvironmentDashboard.Api.Controllers {
    [Route("admin/api/cameras/")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class CameraController : Controller {
        private readonly ICameraStore _cameraStore;
        private readonly IImageService _imageService;

        public CameraController(ICameraStore cameraStore, IImageService imageService) {
            _cameraStore = cameraStore;
            _imageService = imageService;
        }

        [Authorize(Policy = "AdminUser")]
        [HttpGet]
        public async Task<IActionResult> ListCameras(Int32 pageIndex = 0, Int32 pageSize = 100) {
            return Json(await _cameraStore.GetPaged(pageIndex, pageSize));
        }

        [Authorize(Policy = "AdminUser")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCamera([FromRoute] string id) {
            var camera = await _cameraStore.GetById(id);

            if(camera == null)
                return NotFound();

            return Json(camera);
        }

        [Authorize(Policy = "AdminUser")]
        [HttpPost]
        public async Task<IActionResult> CreateCamera([FromBody] Camera camera) {
            var result = await _cameraStore.Save(camera);

            return Json(result);
        }

        [Authorize(Policy = "AdminUser")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCamera([FromRoute] string id, [FromBody] Camera camera) {
            if(id != camera?.Id)
                return BadRequest();

            var result = await _cameraStore.Save(camera);

            return Json(result);
        }

        [Authorize(Policy = "AdminUser")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCamera([FromRoute] string id) {
            await _cameraStore.Delete(id);
            return NoContent();
        }


        [Authorize(Policy = "ApiUser")]
        [HttpPost("{id}")]
        public async Task<IActionResult> SaveImageStream([FromRoute]string id) {
            var camera = await _cameraStore.GetById(id);

            if(camera == null)
                return NotFound();

            if(!Request.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                return BadRequest();

            var extension = ".jpg";
            if(Request.ContentType.Equals("image/png", StringComparison.OrdinalIgnoreCase))
                extension = ".png";
            
            await _imageService.SaveImageStream(camera, Request.Body, extension);

            return NoContent();
        }

        [Authorize(Policy = "AdminUser")]
        [HttpGet("{id}/image-stream")]
        public async Task GetTodaysImageStream([FromRoute]string id) {
            var camera = await _cameraStore.GetById(id);

            if(camera == null) {
                Response.StatusCode = (Int32)HttpStatusCode.NotFound;
                return;
            }

            Int32? maxWidth = null;

            if(Request.Headers.ContainsKey("Viewport-Width")) {
                var viewportWidth = Request.Headers["Viewport-Width"].First();
                if(Int32.TryParse(viewportWidth, out var tmp))
                    maxWidth = tmp;
            }

            var boundary = Guid.NewGuid().ToString("d");
            Response.Headers.Add("Content-Type", "multipart/x-mixed-replace; boundary=" + boundary);
            Response.StatusCode = (Int32)HttpStatusCode.OK;

            await _imageService.WriteReplacingHttpStream(camera, Response.Body, boundary, maxWidth);
        }

        [Authorize(Policy = "AdminUser")]
        [HttpGet("{id}/latest-image")]
        public async Task GetLatestImage([FromRoute]string id) {
            var camera = await _cameraStore.GetById(id);

            if(camera == null) {
                Response.StatusCode = (Int32)HttpStatusCode.NotFound;
                return;
            }
        
            Int32? maxWidth = null;

            if(Request.Headers.ContainsKey("Viewport-Width")) {
                var viewportWidth = Request.Headers["Viewport-Width"].First();
                if(Int32.TryParse(viewportWidth, out var tmp))
                    maxWidth = tmp;
            }

            var mimeType = "image/jpeg";
            Response.Headers.Add("Content-Type", mimeType);
            Response.StatusCode = (Int32)HttpStatusCode.OK;

            await _imageService.WriteLatestImageToStream(camera, Response.Body, maxWidth);
        }
    }
}
