using System.Text;
using System.Text.Json;
using MovieManager.Core.Models;
using MovieManager.MAUI.Models;

namespace MovieManager.MAUI.Services;

public class McpApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl = "http://localhost:5001"; // O la IP si se prueba en un dispositivo físico

    public McpApiClient()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_baseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    public async Task<(bool Success, string Message)> LoadTestDataAsync()
    {
        try
        {
            var response = await _httpClient.PostAsync("/load-test-data", null);
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content);
            var message = result?["message"].GetString() ?? "Error al deserializar";
            return (response.IsSuccessStatusCode, message);
        }
        catch (Exception ex)
        {
            return (false, $"Error de conexión: {ex.Message}");
        }
    }

    public async Task<(bool Success, string Message)> LoadFullDatasetAsync()
    {
        try
        {
            var response = await _httpClient.PostAsync("/load-data", null);
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(content);
            var message = result?["message"].GetString() ?? "Error al deserializar";
            return (response.IsSuccessStatusCode, message);
        }
        catch (Exception ex)
        {
            return (false, $"Error de conexión: {ex.Message}");
        }
    }

    public async Task<MauiQueryResponse?> QueryAsync(string query)
    {
        try
        {
            var request = new { Query = query };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/query", content);
            response.EnsureSuccessStatusCode();
            
            var responseJson = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<MauiQueryResponse>(responseJson, options);
        }
        catch (Exception ex)
        {
            // Devolver un MauiQueryResponse con el error
            return new MauiQueryResponse
            {
                Success = false,
                Message = $"Error al conectar con el servidor: {ex.Message}"
            };
        }
    }
}
