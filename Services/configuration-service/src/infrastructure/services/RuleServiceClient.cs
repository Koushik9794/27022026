using ConfigurationService.Application.Services;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ConfigurationService.Infrastructure.Services
{
    public class RuleServiceClient : IRuleServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public RuleServiceClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["Services:RuleService:BaseUrl"] ?? "http://rule-service";
        }

        public async Task<RuleEvaluationResponse> EvaluateRulesAsync(RuleEvaluationRequest request)
        {
            var url = $"{_baseUrl}/api/rules/evaluate"; // Update to match actual endpoint
            var response = await _httpClient.PostAsJsonAsync(url, request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<RuleEvaluationResponse>() ?? new RuleEvaluationResponse();
        }
    }
}
