using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using EnvironmentDashboard.Api.Contracts;
using EnvironmentDashboard.Api.Models;
using EnvironmentDashboard.Api.Options;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Text;
using SixLabors.ImageSharp.Processing.Transforms;
using SixLabors.Primitives;

namespace EnvironmentDashboard.Api.Services {
    public class ImageService : IImageService {
        private readonly ILogger<ImageService> _log;
        private readonly AmazonWebServicesOptions _options;
        private readonly IAmazonS3 _client;
        private readonly ISystemClock _systemClock;
        private readonly FontFamily _fontFamily;

        public ImageService(IOptions<AmazonWebServicesOptions> optionsAccessor, ISystemClock systemClock, ILogger<ImageService> log) {
            _options = optionsAccessor.Value;
            _client = new AmazonS3Client(_options.AccessKey, _options.SecretKey, _options.Region);
            _systemClock = systemClock;
            _log = log;

            var fonts = new FontCollection();
            _fontFamily = fonts.Install("./Resources/Roboto-Regular.ttf");
        }

        private string GetBucketPrefix(DateTimeOffset date, Camera camera) {
            var prefix = $"{date.ToUniversalTime():yyyy-MM-dd}/{camera.Id}";
            _log.LogDebug("Calculated prefix: " + prefix);
            return prefix;
        }

        private IEnumerable<S3Object> GetImagesForDate(DateTimeOffset date, Camera camera) {
            string continuationToken = null;

            do {
                var request = new ListObjectsV2Request();
                request.BucketName = _options.BucketName;
                request.MaxKeys = 1000;
                request.Prefix = GetBucketPrefix(date, camera);
                request.ContinuationToken = continuationToken;
                
                // Doing this sync to keep this an IEnumerable
                var response = _client.ListObjectsV2Async(request).GetAwaiter().GetResult();
                continuationToken = response.ContinuationToken;

                foreach(var obj in response.S3Objects)
                    yield return obj;
            } while(!string.IsNullOrWhiteSpace(continuationToken));
        }

        private IEnumerable<S3Object> GetLatestImages(Camera camera) {
            var gotImages = false;

            foreach(var obj in GetImagesForDate(_systemClock.UtcNow, camera)) {
                gotImages = true;
                yield return obj;
            }

            if(gotImages)
                yield break;

            _log.LogInformation("No images available for today, trying yesterday.");
            
            foreach(var obj in GetImagesForDate(_systemClock.UtcNow.AddDays(-1), camera)) {
                yield return obj;
            }
        }

        private void MutateImage(Image<Rgba32> image, Int32? maxWidth, DateTime lastModified) {
            var text = lastModified.ToString("HH:mm");

            var font = new Font(_fontFamily, 39, FontStyle.Regular);

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

        public async Task WriteReplacingHttpStream(Camera camera, Stream stream, string boundary, Int32? maxWidth) {
            var mimeType = "image/jpeg";
        
            var objects = GetLatestImages(camera);
            objects = objects.OrderBy(o => o.LastModified).ToList();

            var encoding = new UTF8Encoding(false);

            foreach(var obj in objects) {
                _log.LogDebug($"Fetching image with key " + obj.Key);

                var request = new GetObjectRequest();
                request.BucketName = _options.BucketName;
                request.Key = obj.Key;

                var sw = Stopwatch.StartNew();
                var response = await _client.GetObjectAsync(request);
                sw.Stop();

                _log.LogDebug($"Fetching took " + sw.Elapsed.ToString());

                using(var sr = new StreamWriter(stream, encoding, 4096, true)) {
                    await sr.WriteLineAsync("Content-type: " + mimeType);
                    await sr.WriteLineAsync();
                }

                using(Image<Rgba32> image = Image.Load(response.ResponseStream)) {
                    MutateImage(image, maxWidth, obj.LastModified);

                    var imageEncoder = new JpegEncoder {
                        Quality = 70
                    };
                    
                    image.Save(stream, imageEncoder);
                }

                using(var sr = new StreamWriter(stream, encoding, 4096, true)) {
                    await sr.WriteLineAsync("--" + boundary);
                }

                await stream.FlushAsync();
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }
        }
    
        public async Task WriteLatestImageToStream(Camera camera, Stream stream, Int32? maxWidth) {
            var objects = GetLatestImages(camera);
            var obj = objects.OrderByDescending(o => o.LastModified).FirstOrDefault();

            if(obj == null)
                return;

            var encoding = new UTF8Encoding(false);

            var request = new GetObjectRequest();
            request.BucketName = _options.BucketName;
            request.Key = obj.Key;

            var response = await _client.GetObjectAsync(request);

            using(Image<Rgba32> image = Image.Load(response.ResponseStream)) {
                MutateImage(image, maxWidth, obj.LastModified);

                var imageEncoder = new JpegEncoder {
                    Quality = 80
                };
                
                image.Save(stream, imageEncoder);
            }
        }

        public async Task SaveImageStream(Camera camera, Stream stream, string extension) {
            var fileName = GetBucketPrefix(_systemClock.UtcNow, camera) + "/" + _systemClock.UtcNow.ToString("yyyy-MM-ddTHH-mm-ss") + extension;

            using(var ms = new MemoryStream()) {
                await stream.CopyToAsync(ms);
                ms.Seek(0, SeekOrigin.Begin);
                var utility = new TransferUtility(_client);

                await utility.UploadAsync(ms, _options.BucketName, fileName);
            }
        }
    }
}