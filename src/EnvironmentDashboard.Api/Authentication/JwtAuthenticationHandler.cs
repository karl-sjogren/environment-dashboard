using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using EnvironmentDashboard.Api.Contracts;
using EnvironmentDashboard.Api.Extensions;
using EnvironmentDashboard.Api.Options;
using Jose;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace EnvironmentDashboard.Api.Authentication {
    public class JwtAuthenticationHandler : AuthenticationHandler<JwtBearerOptions> {
        private readonly MongoDbOptions _mongoDbOptions;
        private readonly IUserStore _userStore;
        private readonly IApiKeyStore _apiKeyStore;

        public JwtAuthenticationHandler(IOptionsMonitor<JwtBearerOptions> options,
                                        ILoggerFactory loggerFactory,
                                        UrlEncoder encoder,
                                        ISystemClock clock,
                                        IOptions<MongoDbOptions> optionsAccessor,
                                        IUserStore userStore,
                                        IApiKeyStore apiKeyStore)
            : base(options, loggerFactory, encoder, clock) {
            _mongoDbOptions = optionsAccessor.Value;
            _userStore = userStore;
            _apiKeyStore = apiKeyStore;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync() {
            var token = (string)null;
            var authorization = Request.Headers["Authorization"].FirstOrDefault();

            if(string.IsNullOrEmpty(authorization)) {
                authorization = Request.Query["token"].FirstOrDefault();
            }

            if(string.IsNullOrEmpty(authorization)) {
                return AuthenticateResult.NoResult();
            }

            if(authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) {
                token = authorization.Substring("Bearer ".Length).Trim();
            } else {
                token = authorization;
            }

            if(string.IsNullOrEmpty(token)) {
                return AuthenticateResult.NoResult();
            }

            var sha = SHA256Managed.Create();
            var privateKey = sha.ComputeHash(Encoding.UTF8.GetBytes(_mongoDbOptions.MongoDbUri));

            var decryptedToken = Jose.JWT.Decode(token, privateKey, JweAlgorithm.A256KW, JweEncryption.A256CBC_HS512);

            if(!string.IsNullOrWhiteSpace(decryptedToken)) {
                try {
                    dynamic json = JsonConvert.DeserializeObject(decryptedToken);

                    var subject = (string)json.sub;
                    var claim = (string)json.claim;

                    if(claim == "AdminUser") {
                        var issuedAt = (long)json.exp;
                        var expireTime = issuedAt.FromUnixTime();

                        if(expireTime < DateTime.UtcNow) {
                            return AuthenticateResult.Fail("An expired security token was supplied.");
                        }

                        var user = await _userStore.GetById(subject);
                        
                        if(user == null) {
                            return AuthenticateResult.Fail("An unknown user id was supplied in an otherwise valid token.");
                        }

                        var claims = new List<Claim> {
                            new Claim("AdminUser", user.Id)
                        };

                        var identity = new ClaimsIdentity(claims, JwtBearerDefaults.AuthenticationScheme);
                        var principal = new ClaimsPrincipal(identity);
                        var props = new AuthenticationProperties {
                            IsPersistent = true
                        };

                        return AuthenticateResult.Success(new AuthenticationTicket(principal, props, JwtBearerDefaults.AuthenticationScheme));
                    } else if(claim == "ApiUser") {
                        var user = await _apiKeyStore.GetById(subject);
                        
                        if(user == null) {
                            return AuthenticateResult.Fail("An unknown user id was supplied in an otherwise valid token.");
                        }

                        var claims = new List<Claim> {
                            new Claim("ApiUser", user.Id)
                        };

                        var identity = new ClaimsIdentity(claims, JwtBearerDefaults.AuthenticationScheme);
                        var principal = new ClaimsPrincipal(identity);
                        var props = new AuthenticationProperties {
                            IsPersistent = true
                        };

                        await _apiKeyStore.IncrementReuests(user.Id);

                        return AuthenticateResult.Success(new AuthenticationTicket(principal, props, JwtBearerDefaults.AuthenticationScheme));
                    }
                } catch {
                    return AuthenticateResult.Fail("An invalid security token was supplied.");
                }
            }

            return AuthenticateResult.Fail("An empty security token was supplied.");
        }
    }
}