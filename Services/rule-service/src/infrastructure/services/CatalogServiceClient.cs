using GssCommon.Common.Models.Configurator;
using RuleService.Domain.Services;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace RuleService.Infrastructure.Services
{
    public class CatalogServiceClient : ICatalogServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public CatalogServiceClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["Services:CatalogService:BaseUrl"] ?? "http://catalog-service";
        }

        public async Task<PartMetadata?> LookupPartAsync(string componentType, string attributeName, double targetValue)
        {
            try
            {
                var url = $"{_baseUrl}/api/catalog-lookup/lookup?componentType={Uri.EscapeDataString(componentType)}&attributeName={Uri.EscapeDataString(attributeName)}&targetValue={targetValue}";
                var response = await _httpClient.GetAsync(url);
                
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    return null;

                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<PartMetadata>();
            }
            catch (Exception)
            {
                // In a real system, we'd log this. For now, we'll return null to keep the POC flow moving
                return null;
            }
        }
    }
}
