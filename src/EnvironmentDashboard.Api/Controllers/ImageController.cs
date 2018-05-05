using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using EnvironmentDashboard.Api.Contracts;
using EnvironmentDashboard.Api.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace EnvironmentDashboard.Api.Controllers {
    //[Authorize(Policy = "AdminUser")]
    [Route("admin/api/images/")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ImageController : Controller {
        private readonly AmazonWebServicesOptions _options;
        private readonly IAmazonS3 _client;

        public ImageController(IOptions<AmazonWebServicesOptions> optionsAccessor) {
            _options = optionsAccessor.Value;
            _client = new AmazonS3Client(_options.AccessKey, _options.SecretKey, _options.Region);
        }

        [HttpGet("image-stream")]
        public async Task GetImageStream() {
            string continuationToken = null;
            var objects = new List<S3Object>();
            do {
                var request = new ListObjectsV2Request();
                request.BucketName = _options.BucketName;
                request.MaxKeys = 1000;
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

                using(var sr = new StreamWriter(Response.Body, encoding, 4096, true)) {
                    await sr.WriteLineAsync("Content-type: image/jpeg");
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
    }
}
