using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EnvironmentDashboard.Api.Contracts;
using EnvironmentDashboard.Api.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson.Serialization.IdGenerators;

namespace EnvironmentDashboard.Api.Controllers {
    [Authorize(Policy = "AdminUser")]
    [Route("admin/api/api-keys")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ApiKeyController : Controller {
        
        private readonly IApiKeyStore _apiKeyStore;
        private readonly IApiKeyService _apiKeyService;
        
        public ApiKeyController(IApiKeyStore apiKeyStore, IApiKeyService apiKeyService) {
            _apiKeyStore = apiKeyStore;
            _apiKeyService = apiKeyService;
        }

        [HttpGet]
        public async Task<IActionResult> ListApiKeys(Int32 pageIndex = 0, Int32 pageSize = 100) {
            return Json(await _apiKeyStore.GetPaged(pageIndex, pageSize));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetApiKey([FromRoute] string id) {
            var apiKey = await _apiKeyStore.GetById(id);

            if(apiKey == null)
                return NotFound();

            return Json(apiKey);
        }

        [HttpGet("{id}/token")]
        public async Task<IActionResult> GetApiKeyToken([FromRoute] string id) {
            var result = await _apiKeyStore.GetById(id);

            var token = await _apiKeyService.GenerateApiToken(result.Id);

            return Json(token);
        }

        [HttpPost]
        public async Task<IActionResult> CreateApiKey([FromBody] ApiKey apiKey) {
            var result = await _apiKeyStore.Save(apiKey);

            await _apiKeyService.SendApiKey(result.Id, (Request.IsHttps ? "https://" : "http://") + Request.Host.Host);

            return Json(result);
        }

        [HttpPut("{id}/resend")]
        public async Task<IActionResult> ResendApiKey([FromRoute] string id) {
            var apiKey = await _apiKeyStore.GetById(id);

            if(apiKey == null)
                return NotFound();

            await _apiKeyService.SendApiKey(id, (Request.IsHttps ? "https://" : "http://") + Request.Host.Host);

            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateApiKey([FromRoute] string id, [FromBody] ApiKey apiKey) {
            if(id != apiKey?.Id)
                return BadRequest();

            var result = await _apiKeyStore.Save(apiKey);

            return Json(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteApiKey([FromRoute] string id) {
            await _apiKeyStore.Delete(id);
            return NoContent();
        }
    }
}