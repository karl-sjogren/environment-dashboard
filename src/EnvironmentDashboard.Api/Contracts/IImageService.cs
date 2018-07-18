using System;
using System.IO;
using System.Threading.Tasks;
using EnvironmentDashboard.Api.Models;

namespace EnvironmentDashboard.Api.Contracts {
    public interface IImageService {
        Task WriteReplacingHttpStream(Camera camera, Stream stream, string boundary, Int32? maxWidth);
        Task WriteLatestImageToStream(Camera camera, Stream stream, Int32? maxWidth);
        Task SaveImageStream(Camera camera, Stream stream, string extension);
    }
}