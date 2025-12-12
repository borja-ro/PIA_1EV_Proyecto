using System.Text;
using System.Text.Json;
using MovieManager.Blazor.Models;

namespace MovieManager.Blazor.Services;

public class McpApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<McpApiClient> _logger;

    public McpApiClient(HttpClient httpClient, ILogger<McpApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<bool> CheckHealthAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/health");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking MCP server health");
            return false;
        }
    }

    public async Task<string> LoadTestDataAsync()
    {
        try
        {
            var response = await _httpClient.PostAsync("/load-test-data", null);
            var content = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content);
                if (result != null && result.TryGetValue("count", out var countElement))
                {
                    return $"✓ {countElement} películas cargadas";
                }
                return "✓ Datos cargados";
            }
            
            return $"✗ Error: {response.StatusCode}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading test data");
            return $"✗ Error: {ex.Message}";
        }
    }

    public async Task<QueryResponse?> QueryAsync(string query)
    {
        try
        {
            var request = new QueryRequest { Query = query };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/query", content);
            var responseJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("MCP server error: {StatusCode} - {Response}", response.StatusCode, responseJson);
                return null;
            }

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<QueryResponse>(responseJson, options);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying MCP server");
            return null;
        }
    }
}