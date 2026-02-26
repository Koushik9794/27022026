using System.Net.Http.Json;
using GssWebApi.Dto;

namespace GssWebApi.Services
{
    public class AdminServiceClient : IAdminServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AdminServiceClient> _logger;

        public AdminServiceClient(HttpClient httpClient, ILogger<AdminServiceClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        // --- Roles ---

        public async Task<IEnumerable<RoleResponse>> GetRolesAsync(CancellationToken ct = default)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<RoleResponse>>("api/v1/roles", ct) ?? Enumerable.Empty<RoleResponse>();
        }

        public async Task<RoleResponse?> GetRoleByIdAsync(Guid id, CancellationToken ct = default)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<RoleResponse>($"api/v1/roles/{id}", ct);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<Guid> CreateRoleAsync(CreateRoleRequest request, CancellationToken ct = default)
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/roles", request, ct);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                throw new HttpRequestException($"Admin Service error: {response.StatusCode} - {error}", null, response.StatusCode);
            }
            
            var result = await response.Content.ReadFromJsonAsync<CreateRoleResult>(options: null, cancellationToken: ct);
            return result?.RoleId ?? Guid.Empty;
        }

        public async Task<bool> UpdateRoleAsync(Guid id, UpdateRoleRequest request, CancellationToken ct = default)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/v1/roles/{id}", request, ct);
            if (!response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.NotFound)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                throw new HttpRequestException($"Admin Service error: {response.StatusCode} - {error}", null, response.StatusCode);
            }
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteRoleAsync(Guid id, string? modifiedBy, CancellationToken ct = default)
        {
            var url = $"api/v1/roles/{id}";
            if (!string.IsNullOrEmpty(modifiedBy)) url += $"?modifiedBy={Uri.EscapeDataString(modifiedBy)}";
            
            var response = await _httpClient.DeleteAsync(url, ct);
            if (!response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.NotFound)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                throw new HttpRequestException($"Admin Service error: {response.StatusCode} - {error}", null, response.StatusCode);
            }
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ActivateRoleAsync(Guid id, bool activate, string? modifiedBy, CancellationToken ct = default)
        {
            var url = $"api/v1/roles/{id}/activate?activate={activate}";
            if (!string.IsNullOrEmpty(modifiedBy)) url += $"&modifiedBy={Uri.EscapeDataString(modifiedBy)}";
            
            var response = await _httpClient.PostAsync(url, null, ct);
            if (!response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.NotFound)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                throw new HttpRequestException($"Admin Service error: {response.StatusCode} - {error}", null, response.StatusCode);
            }
            return response.IsSuccessStatusCode;
        }

        // --- Permissions ---

        public async Task<IEnumerable<PermissionResponse>> GetPermissionsAsync(string? module = null, string? entityName = null, CancellationToken ct = default)
        {
            var url = "api/v1/permissions";
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(module)) queryParams.Add($"module={Uri.EscapeDataString(module)}");
            if (!string.IsNullOrEmpty(entityName)) queryParams.Add($"entityName={Uri.EscapeDataString(entityName)}");
            
            if (queryParams.Any()) url += "?" + string.Join("&", queryParams);

            return await _httpClient.GetFromJsonAsync<IEnumerable<PermissionResponse>>(url, ct) ?? Enumerable.Empty<PermissionResponse>();
        }

        public async Task<PermissionResponse?> GetPermissionByIdAsync(Guid id, CancellationToken ct = default)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<PermissionResponse>($"api/v1/permissions/{id}", ct);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<Guid> CreatePermissionAsync(CreatePermissionRequest request, CancellationToken ct = default)
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/permissions", request, ct);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                throw new HttpRequestException($"Admin Service error: {response.StatusCode} - {error}", null, response.StatusCode);
            }
            return await response.Content.ReadFromJsonAsync<Guid>(cancellationToken: ct);
        }

        public async Task<bool> UpdatePermissionAsync(Guid id, UpdatePermissionRequest request, CancellationToken ct = default)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/v1/permissions/{id}", request, ct);
            if (!response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.NotFound)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                throw new HttpRequestException($"Admin Service error: {response.StatusCode} - {error}", null, response.StatusCode);
            }
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeletePermissionAsync(Guid id, string? modifiedBy, CancellationToken ct = default)
        {
            var user = string.IsNullOrEmpty(modifiedBy) ? "System" : modifiedBy;
            var response = await _httpClient.DeleteAsync($"api/v1/permissions/{id}?modifiedBy={Uri.EscapeDataString(user)}", ct);
            if (!response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.NotFound)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                throw new HttpRequestException($"Admin Service error: {response.StatusCode} - {error}", null, response.StatusCode);
            }
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ActivatePermissionAsync(Guid id, bool activate, string? modifiedBy, CancellationToken ct = default)
        {
            var user = string.IsNullOrEmpty(modifiedBy) ? "System" : modifiedBy;
            var response = await _httpClient.PostAsync($"api/v1/permissions/{id}/activate?activate={activate}&modifiedBy={Uri.EscapeDataString(user)}", null, ct);
            if (!response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.NotFound)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                throw new HttpRequestException($"Admin Service error: {response.StatusCode} - {error}", null, response.StatusCode);
            }
            return response.IsSuccessStatusCode;
        }

        // --- Entities ---

        public async Task<IEnumerable<EntityResponse>> GetEntitiesAsync(CancellationToken ct = default)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<EntityResponse>>("api/v1/entities", ct) ?? Enumerable.Empty<EntityResponse>();
        }

        public async Task<string> CreateEntityAsync(CreateEntityRequest request, CancellationToken ct = default)
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/entities", request, ct);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync(ct);
        }

        // --- Role Permissions ---

        public async Task<Guid> AssignPermissionToRoleAsync(AssignPermissionRequest request, CancellationToken ct = default)
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/role-permissions", request, ct);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<Guid>(cancellationToken: ct);
        }

        public async Task<bool> RemovePermissionFromRoleAsync(Guid roleId, Guid permissionId, CancellationToken ct = default)
        {
            var response = await _httpClient.DeleteAsync($"api/v1/role-permissions?roleId={roleId}&permissionId={permissionId}", ct);
            return response.IsSuccessStatusCode;
        }

        public async Task<IEnumerable<PermissionResponse>> GetPermissionsByRoleAsync(Guid roleId, CancellationToken ct = default)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<PermissionResponse>>($"api/v1/role-permissions/roles/{roleId}/permissions", ct) ?? Enumerable.Empty<PermissionResponse>();
        }

        public async Task<IEnumerable<RoleResponse>> GetRolesByPermissionAsync(Guid permissionId, CancellationToken ct = default)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<RoleResponse>>($"api/v1/role-permissions/permissions/{permissionId}/roles", ct) ?? Enumerable.Empty<RoleResponse>();
        }

        // --- Users ---

        public async Task<IEnumerable<UserResponse>> GetUsersAsync(CancellationToken ct = default)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<UserResponse>>("api/v1/users", ct) ?? Enumerable.Empty<UserResponse>();
        }

        public async Task<UserResponse?> GetUserByIdAsync(Guid id, CancellationToken ct = default)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<UserResponse>($"api/v1/users/{id}", ct);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

    public async Task<UserResponse?> GetUserByEmailAsync(string email, CancellationToken ct = default)
        {
            var users = await GetUsersAsync(ct);
            return users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }

        // --- Dealers ---

        public async Task<IEnumerable<DealerDto>> GetDealersAsync(CancellationToken ct = default)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<DealerDto>>("api/v1/dealers", ct) ?? Enumerable.Empty<DealerDto>();
        }

        public async Task<DealerDto?> GetDealerByIdAsync(Guid id, CancellationToken ct = default)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<DealerDto>($"api/v1/dealers/{id}", ct);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<Guid> CreateDealerAsync(CreateDealerRequest request, CancellationToken ct = default)
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/dealers", request, ct);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                throw new HttpRequestException($"Admin Service error: {response.StatusCode} - {error}", null, response.StatusCode);
            }
            // Admin service returns the Guid directly or in a wrapper? 
            // Based on DealersController.cs line 73: return CreatedAtAction(nameof(GetById), new { id }, id); 
            // It returns the Guid in the body if we look at line 66 [ProducesResponseType(typeof(Guid), ...)]
            return await response.Content.ReadFromJsonAsync<Guid>(cancellationToken: ct);
        }

        public async Task<bool> UpdateDealerAsync(Guid id, UpdateDealerRequest request, CancellationToken ct = default)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/v1/dealers/{id}", request, ct);
            if (!response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.NotFound)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                throw new HttpRequestException($"Admin Service error: {response.StatusCode} - {error}", null, response.StatusCode);
            }
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteDealerAsync(Guid id, Guid updatedBy, CancellationToken ct = default)
        {
            var response = await _httpClient.DeleteAsync($"api/v1/dealers/{id}?updatedBy={updatedBy}", ct);
            if (!response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.NotFound)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                throw new HttpRequestException($"Admin Service error: {response.StatusCode} - {error}", null, response.StatusCode);
            }
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ActivateDealerAsync(Guid id, Guid updatedBy, CancellationToken ct = default)
        {
            var response = await _httpClient.PostAsync($"api/v1/dealers/{id}/activate?updatedBy={updatedBy}", null, ct);
            if (!response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.NotFound)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                throw new HttpRequestException($"Admin Service error: {response.StatusCode} - {error}", null, response.StatusCode);
            }
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeactivateDealerAsync(Guid id, Guid updatedBy, CancellationToken ct = default)
        {
            var response = await _httpClient.PostAsync($"api/v1/dealers/{id}/deactivate?updatedBy={updatedBy}", null, ct);
            if (!response.IsSuccessStatusCode && response.StatusCode != System.Net.HttpStatusCode.NotFound)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                throw new HttpRequestException($"Admin Service error: {response.StatusCode} - {error}", null, response.StatusCode);
            }
            return response.IsSuccessStatusCode;
        }
    }
}
