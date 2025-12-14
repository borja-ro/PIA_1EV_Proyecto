using System.Text;
using System.Text.Json;
using MovieManager.Console.Models;

namespace MovieManager.Console.Services;

public class ApiClient
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public ApiClient(string baseUrl = "http://localhost:5001")
    {
        _baseUrl = baseUrl;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    public async Task<bool> CheckHealthAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/health");
            return response.IsSuccessStatusCode;
        }
        catch
        {
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
                var result = JsonSerializer.Deserialize<Dictionary<string, object>>(content);
                return result != null ? $"✓ Datos cargados: {result["count"]} películas" : "✓ Datos cargados";
            }
            
            return $"✗ Error: {response.StatusCode}";
        }
        catch (Exception ex)
        {
            return $"✗ Error: {ex.Message}";
        }
    }
    
    public async Task<string> LoadFullDatasetAsync()
    {
        try
        {
            var response = await _httpClient.PostAsync("/load-data", null);
            var content = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<Dictionary<string, object>>(content);
                return result != null ? $"✓ Dataset completo cargado: {result["count"]} películas" : "✓ Dataset cargado";
            }
            
            return $"✗ Error: {response.StatusCode}";
        }
        catch (Exception ex)
        {
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
                System.Console.WriteLine($"Error del servidor: {response.StatusCode}");
                System.Console.WriteLine(responseJson);
                return null;
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<QueryResponse>(responseJson, options);
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error al consultar: {ex.Message}");
            return null;
        }
    }
}