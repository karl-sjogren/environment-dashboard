using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using EnvironmentDashboard.Api.Contracts;
using EnvironmentDashboard.Api.Extensions;
using EnvironmentDashboard.Api.Models;
using EnvironmentDashboard.Api.Options;
using Jose;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization.IdGenerators;

namespace EnvironmentDashboard.Api.Controllers {
    [Authorize(Policy = "AdminUser")]
    [Route("admin/api/user/")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class UserController : Controller {
        private readonly IUserStore _userStore;
        private readonly MongoDbOptions _options;

        public UserController(IUserStore userStore, IOptions<MongoDbOptions> optionsAccessor) {
            _userStore = userStore;
            _options = optionsAccessor.Value;
        }

        [HttpGet]
        public async Task<IActionResult> ListUsers(Int32 pageIndex = 0, Int32 pageSize = 100) {
            return Json(await _userStore.GetPaged(pageIndex, pageSize));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser([FromRoute] string id) {
            var user = await _userStore.GetById(id);

            if(user == null)
                return NotFound();

            return Json(user);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] User user) {
            var result = await _userStore.Save(user);

            return Json(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser([FromRoute] string id, [FromBody] User user) {
            if(id != user?.Id)
                return BadRequest();

            var result = await _userStore.Save(user);

            return Json(result);
        }

        [HttpPut("{id}/password")]
        public async Task<IActionResult> SetPassword([FromRoute] string id, [FromBody] SetPasswordRequest request) {
            var result = await _userStore.SetPassword(id, request.Password);

            if(result == null)
                return NotFound();

            return NoContent();
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] AuthenticateRequest request) {
            var user = await _userStore.Authenticate(request.Username, request.Password);
            if(user == null)
                return Unauthorized();

            var payload = new Dictionary<string, object>() {
                { "sub", user.Id },
                { "iss", "EnvironmentDashboard" },
                { "claim", "AdminUser" },
                { "exp", DateTime.UtcNow.AddDays(30).ToUnixTime() }
            };

            var sha = SHA256Managed.Create();
            var privateKey = sha.ComputeHash(Encoding.UTF8.GetBytes(_options.MongoDbUri));
            var token = Jose.JWT.Encode(payload, privateKey, JweAlgorithm.A256KW, JweEncryption.A256CBC_HS512);

            return Json(new { token = token, userId = user.Id, name = $"{user.FirstName} {user.LastName}"});
        }

        #region Request models

        public class AuthenticateRequest {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        public class SetPasswordRequest {
            public string Password { get; set; }
        }

        #endregion
    }
}