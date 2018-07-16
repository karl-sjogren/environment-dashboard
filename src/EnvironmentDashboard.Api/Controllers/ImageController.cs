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
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Text;
using SixLabors.ImageSharp.Processing.Transforms;
using SixLabors.Primitives;

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

        private IEnumerable<S3Object> GetTodaysImages() {
            string continuationToken = null;

            do {
                var request = new ListObjectsV2Request();
                request.BucketName = _options.BucketName;
                request.MaxKeys = 1000;
                request.Prefix = BucketPrefix;
                request.ContinuationToken = continuationToken;
                
                // Doing this sync to keep this an IEnumerable
                var response = _client.ListObjectsV2Async(request).GetAwaiter().GetResult();
                continuationToken = response.ContinuationToken;

                foreach(var obj in response.S3Objects)
                    yield return obj;
            } while(!string.IsNullOrWhiteSpace(continuationToken));
        }

        [Authorize(Policy = "AdminUser")]
        [HttpGet("image-stream")]
        public async Task GetImageStream() {
            var mimeType = "image/jpeg";
            Int32? maxWidth = null;

            if(Request.Headers.ContainsKey("Viewport-Width")) {
                var viewportWidth = Request.Headers["Viewport-Width"].First();
                if(Int32.TryParse(viewportWidth, out var tmp))
                    maxWidth = tmp;
            }
        
            var objects = GetTodaysImages();
            objects = objects.OrderBy(o => o.LastModified).ToList();

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
                    await sr.WriteLineAsync("Content-type: " + mimeType);
                    await sr.WriteLineAsync();
                }

                using(Image<Rgba32> image = Image.Load(response.ResponseStream)) {
                    MutateImage(image, maxWidth, obj.LastModified);

                    var imageEncoder = new JpegEncoder {
                        Quality = 70
                    };
                    
                    image.Save(Response.Body, imageEncoder);
                }

                using(var sr = new StreamWriter(Response.Body, encoding, 4096, true)) {
                    await sr.WriteLineAsync("--" + boundary);
                }

                await Response.Body.FlushAsync();
                await Task.Delay(TimeSpan.FromMilliseconds(250));
            }
        }

        private void MutateImage(Image<Rgba32> image, Int32? maxWidth, DateTime lastModified) {
            var text = lastModified.ToString("HH:mm");
            var font = SystemFonts.CreateFont("Arial", 39, FontStyle.Regular);

            if(maxWidth.HasValue) {
                var resizeOptions = new ResizeOptions {
                    Mode = ResizeMode.Max,
                    Position = AnchorPositionMode.TopLeft,
                    Size = new Size(maxWidth.Value, maxWidth.Value)
                };

                if(image.Width > maxWidth.Value) {
                    image.Mutate(x => x
                        .Resize(resizeOptions));
                }
            }
            
            var textGraphicsOptions = new TextGraphicsOptions(true) {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom
            };

            image.Mutate(x => x
                .DrawText(textGraphicsOptions, text, font, Rgba32.LawnGreen, new PointF(image.Width - 10f, image.Height - 10f))
            );
        }
    
        [Authorize(Policy = "AdminUser")]
        [HttpGet("latest")]
        public async Task GetLatestImage() {
            Int32? maxWidth = null;

            if(Request.Headers.ContainsKey("Viewport-Width")) {
                var viewportWidth = Request.Headers["Viewport-Width"].First();
                if(Int32.TryParse(viewportWidth, out var tmp))
                    maxWidth = tmp;
            }
        
            var objects = GetTodaysImages();
            var obj = objects.OrderByDescending(o => o.LastModified).FirstOrDefault();

            var encoding = new UTF8Encoding(false);

            var request = new GetObjectRequest();
            request.BucketName = _options.BucketName;
            request.Key = obj.Key;

            var response = await _client.GetObjectAsync(request);
            
            var mimeType = "image/jpeg";
            Response.Headers.Add("Content-Type", mimeType);
            Response.StatusCode = (Int32)HttpStatusCode.OK;

            using(Image<Rgba32> image = Image.Load(response.ResponseStream)) {
                MutateImage(image, maxWidth, obj.LastModified);

                var imageEncoder = new JpegEncoder {
                    Quality = 80
                };
                
                image.Save(Response.Body, imageEncoder);
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
