using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using JetBrains.Annotations;
using System.Net.Http;

namespace EnvironmentDashboard.Api.Middlewares {
    public static class ReverseProxyExtensions {
        /// <summary>
        /// Adds the reverse proxy middleware to the request pipeline. This should be the last middleware in the pipeline.
        /// </summary>
        /// <param name="app">The application builder instance.</param>
        /// <param name="proxyToScheme">The scheme of the host to proxy to.</param>
        /// <param name="proxyToHost">The host to proxy to.</param>
        /// <param name="proxyToPort">The port of the host to proxy to. Defaults to 80.</param>
        public static void UseReverseProxy(this IApplicationBuilder app, string proxyToScheme, string proxyToHost, Int32 proxyToPort = 80) {
            var loggerFactory = (ILoggerFactory)app.ApplicationServices.GetService(typeof(ILoggerFactory));

            var options = new ReverseProxyOptions { Scheme = proxyToScheme, Host = proxyToHost, Port = proxyToPort };            
            app.UseMiddleware<ReverseProxyMiddleware>(options, loggerFactory);
        }
    }

    public class ReverseProxyOptions {
        public string Scheme { get; set; }
        public string Host { get; set; }
        public Int32 Port { get; set; }
    }

    [UsedImplicitly]
    public class ReverseProxyMiddleware {
        private readonly RequestDelegate _next;
        private readonly ReverseProxyOptions _options;
        private readonly HttpClient _httpClient;
        private readonly ILogger _log;

        public ReverseProxyMiddleware(RequestDelegate next, ReverseProxyOptions options, ILoggerFactory loggerFactory) {
            _next = next;
            _options = options;

            _log = loggerFactory.CreateLogger<ReverseProxyMiddleware>();
            _httpClient = new HttpClient();
        }

        public async Task Invoke(HttpContext ctx) {
            if(ctx.Response.HasStarted)
                return;

            _log.LogDebug($"Called with {ctx.Request.Method} {ctx.Request.Path}?{ctx.Request.QueryString.Value}");
            
            var request = new HttpRequestMessage();

            try {
                foreach (var header in ctx.Request.Headers){
                    _log.LogDebug($"Adding header {header.Key} : {header.Value}");
                    if (!request.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()) && request.Content != null) {
                        _log.LogWarning($"Sorry, I mean content header {header.Key} : {header.Value}");
                        request.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                    }
                }

                var method = ctx.Request.Method;
                request.Method = new HttpMethod(method);

                var canHazBody = new[] { "POST", "PUT" };
                if(canHazBody.Contains(method))
                    request.Content = new StreamContent(ctx.Request.Body);

                var builder = new UriBuilder();
                builder.Scheme = _options.Scheme;
                builder.Host = _options.Host;
                builder.Port = _options.Port;
                builder.Path = ctx.Request.Path;
                builder.Query = ctx.Request.QueryString.ToString();

                request.RequestUri = builder.Uri;
                _log.LogDebug($"Passing request to {builder.Uri}");

                request.Headers.Host = $"{_options.Host}:{_options.Port}";

                using(var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ctx.RequestAborted)) {
                    foreach(var header in response.Headers.Concat(response.Content.Headers)) {
                        _log.LogTrace($"Recieved header {header.Key} : {header.Value}");
                        if(header.Key == "Transfer-Encoding")
                            continue;

                        ctx.Response.Headers[header.Key] = header.Value.ToArray();
                    }

                    ctx.Response.StatusCode = (Int32)response.StatusCode;
                    await response.Content.CopyToAsync(ctx.Response.Body);
                }
            } finally {
                request.Dispose();
            }
        }
    }
}
