using ConfigurationService.Application.Services;
using GssCommon.Common.Models.Configurator;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ConfigurationService.Infrastructure.Services
{
    public class BomServiceClient : IBomServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public BomServiceClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["Services:BomService:BaseUrl"] ?? "http://bom-service";
        }

        public async Task PushBomBatchAsync(Guid configurationId, string projectName, IEnumerable<BomItem> items)
        {
            var url = $"{_baseUrl}/api/bom/batch";
            var request = new { ConfigurationId = configurationId, ProjectName = projectName, Items = items };
            var response = await _httpClient.PostAsJsonAsync(url, request);
            response.EnsureSuccessStatusCode();
        }
    }
}
