using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using EnvironmentDashboard.Api.Contracts;
using EnvironmentDashboard.Api.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Options;

namespace EnvironmentDashboard.Api.Controllers {
    [Route("admin/api/images/")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ImageController : Controller {
        private readonly AmazonWebServicesOptions _options;
        private readonly IAmazonS3 _client;
        private readonly ISystemClock _systemClock;

        public ImageController(IOptions<AmazonWebServicesOptions> optionsAccessor, ISystemClock systemClock) {
            _options = optionsAccessor.Value;
            _client = new AmazonS3Client(_options.AccessKey, _options.SecretKey, _options.Region);
            _systemClock = systemClock;
        }

        private string BucketPrefix => _systemClock.UtcNow.ToString("yyyy-MM-dd");

        [Authorize(Policy = "AdminUser")]
        [HttpGet("image-stream")]
        public async Task GetImageStream() {
            string continuationToken = null;
            var objects = new List<S3Object>();
            do {
                var request = new ListObjectsV2Request();
                request.BucketName = _options.BucketName;
                request.MaxKeys = 1000;
                request.Prefix = BucketPrefix;
                request.ContinuationToken = continuationToken;
                
                var response = await _client.ListObjectsV2Async(request);
                continuationToken = response.ContinuationToken;

                objects.AddRange(response.S3Objects);
            } while(!string.IsNullOrWhiteSpace(continuationToken));

            objects = objects.OrderByDescending(o => o.LastModified).ToList();

            var boundary = Guid.NewGuid().ToString("d");
            Response.Headers.Add("Content-Type", "multipart/x-mixed-replace; boundary=" + boundary);
            Response.StatusCode = (Int32)HttpStatusCode.OK;

            var encoding = new UTF8Encoding(false);

            foreach(var obj in objects) {
                var request = new GetObjectRequest();
                request.BucketName = _options.BucketName;
                request.Key = obj.Key;

                var response = await _client.GetObjectAsync(request);
                
                var mimeType = "image/jpeg";
                if(Path.GetExtension(response.Key)?.Equals(".png", StringComparison.OrdinalIgnoreCase) == true)
                    mimeType = "image/png";

                using(var sr = new StreamWriter(Response.Body, encoding, 4096, true)) {
                    await sr.WriteLineAsync("Content-type: " + mimeType);
                    await sr.WriteLineAsync();
                }

                await response.ResponseStream.CopyToAsync(Response.Body);

                using(var sr = new StreamWriter(Response.Body, encoding, 4096, true)) {
                    await sr.WriteLineAsync("--" + boundary);
                }

                await Response.Body.FlushAsync();
                await Task.Delay(TimeSpan.FromMilliseconds(500));
            }
        }

        [Authorize(Policy = "ApiUser")]
        [HttpPost("image-stream")]
        public async Task<IActionResult> SaveImageStream() {
            if(!Request.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                return BadRequest();

            var extension = ".jpg";
            if(Request.ContentType.Equals("image/png", StringComparison.OrdinalIgnoreCase))
                extension = ".png";
            
            var fileName = BucketPrefix + "/" + _systemClock.UtcNow.ToString("yyyy-MM-ddTHH-mm-ss") + extension;

            using(var ms = new MemoryStream()) {
                await Request.Body.CopyToAsync(ms);
                ms.Seek(0, SeekOrigin.Begin);
                var utility = new TransferUtility(_client);

                await utility.UploadAsync(ms, _options.BucketName, fileName);
            }
            
            return NoContent();
        }
    }
}
