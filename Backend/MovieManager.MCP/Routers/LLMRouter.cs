using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Globalization;
using MovieManager.Core.Models;

namespace MovieManager.MCP.Routers;

public class LLMRouter
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;

    public LLMRouter(string apiKey, string model = "anthropic/claude-3.5-sonnet")
    {
        _apiKey = apiKey;
        _model = model;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://openrouter.ai/api/v1/")
        };
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "http://localhost:5000");
        _httpClient.DefaultRequestHeaders.Add("X-Title", "MovieManager MCP");
    }

    public async Task<LLMQueryResult> GenerateLinqQuery(string userQuery)
    {
        var prompt = BuildPrompt(userQuery);
        
        var requestBody = new
        {
            model = _model,
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            temperature = 0.3,
            max_tokens = 500
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync("chat/completions", content);
            var responseJson = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
            {
                return new LLMQueryResult
                {
                    Success = false,
                    Error = $"OpenRouter API error: {response.StatusCode} - {responseJson}"
                };
            }

            // Parseo tolerante: content puede ser string o array de partes con { text }
            string generatedCode = string.Empty;
            try
            {
                using var doc = JsonDocument.Parse(responseJson);
                var root = doc.RootElement;
                if (root.TryGetProperty("choices", out var choices) && choices.ValueKind == JsonValueKind.Array && choices.GetArrayLength() > 0)
                {
                    var firstChoice = choices[0];
                    if (firstChoice.TryGetProperty("message", out var message))
                    {
                        if (message.TryGetProperty("content", out var contentElem))
                        {
                            if (contentElem.ValueKind == JsonValueKind.String)
                            {
                                generatedCode = contentElem.GetString() ?? string.Empty;
                            }
                            else if (contentElem.ValueKind == JsonValueKind.Array)
                            {
                                var sb = new StringBuilder();
                                foreach (var part in contentElem.EnumerateArray())
                                {
                                    if (part.ValueKind == JsonValueKind.Object && part.TryGetProperty("text", out var txt))
                                    {
                                        sb.Append(txt.GetString());
                                    }
                                }
                                generatedCode = sb.ToString();
                            }
                        }
                    }
                }
            }
            catch
            {
                // Si el esquema cambia, como último recurso devolvemos el JSON completo
                generatedCode = responseJson;
            }

            // Extraer solo el código LINQ (eliminar markdown y explicaciones)
            var linqCode = ExtractLinqCode(generatedCode);

            // Validar que haya un filtro LINQ válido
            if (string.IsNullOrWhiteSpace(linqCode) || !linqCode.TrimStart().StartsWith("movies.Where", StringComparison.Ordinal))
            {
                return new LLMQueryResult
                {
                    Success = false,
                    Error = "La respuesta del LLM no contiene una expresión LINQ válida.",
                    RawResponse = generatedCode
                };
            }

            return new LLMQueryResult
            {
                Success = true,
                LinqQuery = linqCode,
                RawResponse = generatedCode
            };
        }
        catch (Exception ex)
        {
            return new LLMQueryResult
            {
                Success = false,
                Error = $"Error al llamar a OpenRouter: {ex.Message}"
            };
        }
    }

    private string BuildPrompt(string userQuery)
    {
        return $@"Eres un experto en C# y LINQ. Tu tarea es generar una expresión LINQ para filtrar películas.

Esquema de datos:
```csharp
public class Movie
{{
    public int Id {{ get; set; }}
    public string Title {{ get; set; }}
    public int Year {{ get; set; }}
    public string Genre {{ get; set; }}
    public double Rating {{ get; set; }}
    public string Director {{ get; set; }}
    public int Runtime {{ get; set; }}
    public string? Overview {{ get; set; }}
    public string? Star1 {{ get; set; }}
}}
```

Consulta del usuario: ""{userQuery}""

INSTRUCCIONES CRÍTICAS:
1. Genera SOLO el código LINQ que filtra la colección 'movies'
2. La expresión debe empezar con: movies.Where(...)
3. NO incluyas código de ejecución, contexto, ni explicaciones
4. NO uses ToList(), First(), Count() - solo el filtro Where
5. Usa StringComparison.OrdinalIgnoreCase para strings
6. Si la consulta es ambigua, genera el filtro más razonable
7. Importante: si el género es Ciencia Ficción, usa exactamente ""Sci-Fi"" (así aparece en el dataset)

Ejemplos:
- ""películas dramáticas con más de 8 de rating"" → movies.Where(m => m.Genre.Contains(""Drama"", StringComparison.OrdinalIgnoreCase) && m.Rating > 8.0)
- ""films dirigidos por Nolan después de 2010"" → movies.Where(m => m.Director.Contains(""Nolan"", StringComparison.OrdinalIgnoreCase) && m.Year > 2010)
- ""películas de acción cortas"" → movies.Where(m => m.Genre.Contains(""Action"", StringComparison.OrdinalIgnoreCase) && m.Runtime < 120)

Responde ÚNICAMENTE con el código LINQ:";
    }

    private string ExtractLinqCode(string response)
    {
        if (string.IsNullOrWhiteSpace(response)) return string.Empty;

        // Eliminar markdown code blocks
        response = response
            .Replace("```csharp", string.Empty)
            .Replace("```cs", string.Empty)
            .Replace("```", string.Empty)
            .Trim();
        
        // Buscar la línea que empieza con "movies.Where"
        var lines = response.Split('\n');
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("movies.Where", StringComparison.OrdinalIgnoreCase))
            {
                // Eliminar punto y coma final si existe
                return trimmed.TrimEnd(';');
            }
        }

        // Intentar encontrarlo en medio del texto
        var idx = response.IndexOf("movies.Where", StringComparison.OrdinalIgnoreCase);
        if (idx >= 0)
        {
            var slice = response.Substring(idx).Trim();
            var semi = slice.IndexOf(';');
            if (semi >= 0) slice = slice.Substring(0, semi);
            return slice;
        }

        // Si no encontramos la línea, no devolver texto libre
        return string.Empty;
    }

    public IEnumerable<Movie> ExecuteLinq(string linqQuery, IEnumerable<Movie> movies)
    {
        try
        {
            // Aquí usaremos reflexión o compilación dinámica
            // Por simplicidad, vamos a usar un evaluador simple
            // En producción usarías algo como Roslyn o DynamicLinq

            // Por ahora, evaluamos expresiones comunes
            // Esta es una versión simplificada - en la siguiente iteración podemos mejorarla
            
            return EvaluateSimpleLinq(linqQuery, movies);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error ejecutando LINQ: {ex.Message}");
            return Enumerable.Empty<Movie>();
        }
    }

    private IEnumerable<Movie> EvaluateSimpleLinq(string linqQuery, IEnumerable<Movie> movies)
    {
        // Evaluador simplificado de LINQ
        // En producción usarías Roslyn o DynamicLinq
        
        Console.WriteLine($"[LLM] Evaluando LINQ: {linqQuery}");
        
        var result = movies.AsEnumerable();

        // Extraer condiciones con regex más flexible
        // Buscar patrones: m.Property.Contains("value") o m.Property > value
        
        // Pattern: m.Genre.Contains("xxx")
        var genreMatches = System.Text.RegularExpressions.Regex.Matches(
            linqQuery, 
            @"m\.Genre\.Contains\([""']([^""']+)[""']",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
        foreach (System.Text.RegularExpressions.Match match in genreMatches)
        {
            var genre = match.Groups[1].Value;
            var normalizedGenre = NormalizeGenre(genre);
            Console.WriteLine($"[LLM] Aplicando filtro: Genre contains '{normalizedGenre}'");
            result = result.Where(m => m.Genre.Contains(normalizedGenre, StringComparison.OrdinalIgnoreCase));
        }

        // Pattern: m.Director.Contains("xxx")
        var directorMatches = System.Text.RegularExpressions.Regex.Matches(
            linqQuery,
            @"m\.Director\.Contains\([""']([^""']+)[""']",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
        foreach (System.Text.RegularExpressions.Match match in directorMatches)
        {
            var director = match.Groups[1].Value;
            Console.WriteLine($"[LLM] Aplicando filtro: Director contains '{director}'");
            result = result.Where(m => m.Director.Contains(director, StringComparison.OrdinalIgnoreCase));
        }

        // Pattern: m.Rating > 8.5
        var ratingGreaterMatch = System.Text.RegularExpressions.Regex.Match(
            linqQuery,
            @"m\.Rating\s*>\s*(\d+\.?\d*)",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
        if (ratingGreaterMatch.Success)
        {
            var rating = double.Parse(ratingGreaterMatch.Groups[1].Value, CultureInfo.InvariantCulture);
            Console.WriteLine($"[LLM] Aplicando filtro: Rating > {rating}");
            result = result.Where(m => m.Rating > rating);
        }

        // Pattern: m.Rating < 8.5
        var ratingLessMatch = System.Text.RegularExpressions.Regex.Match(
            linqQuery,
            @"m\.Rating\s*<\s*(\d+\.?\d*)",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
        if (ratingLessMatch.Success)
        {
            var rating = double.Parse(ratingLessMatch.Groups[1].Value, CultureInfo.InvariantCulture);
            Console.WriteLine($"[LLM] Aplicando filtro: Rating < {rating}");
            result = result.Where(m => m.Rating < rating);
        }

        // Pattern: m.Rating >= 8.5
        var ratingGreaterEqualMatch = System.Text.RegularExpressions.Regex.Match(
            linqQuery,
            @"m\.Rating\s*>=\s*(\d+\.?\d*)",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
        if (ratingGreaterEqualMatch.Success)
        {
            var rating = double.Parse(ratingGreaterEqualMatch.Groups[1].Value, CultureInfo.InvariantCulture);
            Console.WriteLine($"[LLM] Aplicando filtro: Rating >= {rating}");
            result = result.Where(m => m.Rating >= rating);
        }

        // Pattern: m.Year > 2010
        var yearGreaterMatch = System.Text.RegularExpressions.Regex.Match(
            linqQuery,
            @"m\.Year\s*>\s*(\d{4})",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
        if (yearGreaterMatch.Success)
        {
            var year = int.Parse(yearGreaterMatch.Groups[1].Value);
            Console.WriteLine($"[LLM] Aplicando filtro: Year > {year}");
            result = result.Where(m => m.Year > year);
        }

        // Pattern: m.Year < 2010
        var yearLessMatch = System.Text.RegularExpressions.Regex.Match(
            linqQuery,
            @"m\.Year\s*<\s*(\d{4})",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
        if (yearLessMatch.Success)
        {
            var year = int.Parse(yearLessMatch.Groups[1].Value);
            Console.WriteLine($"[LLM] Aplicando filtro: Year < {year}");
            result = result.Where(m => m.Year < year);
        }

        // Pattern: m.Year == 2010
        var yearEqualMatch = System.Text.RegularExpressions.Regex.Match(
            linqQuery,
            @"m\.Year\s*==\s*(\d{4})",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
        if (yearEqualMatch.Success)
        {
            var year = int.Parse(yearEqualMatch.Groups[1].Value);
            Console.WriteLine($"[LLM] Aplicando filtro: Year == {year}");
            result = result.Where(m => m.Year == year);
        }

        // Pattern: m.Runtime < 120
        var runtimeLessMatch = System.Text.RegularExpressions.Regex.Match(
            linqQuery,
            @"m\.Runtime\s*<\s*(\d+)",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
        if (runtimeLessMatch.Success)
        {
            var runtime = int.Parse(runtimeLessMatch.Groups[1].Value);
            Console.WriteLine($"[LLM] Aplicando filtro: Runtime < {runtime}");
            result = result.Where(m => m.Runtime < runtime);
        }

        // Pattern: m.Runtime > 120
        var runtimeGreaterMatch = System.Text.RegularExpressions.Regex.Match(
            linqQuery,
            @"m\.Runtime\s*>\s*(\d+)",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase
        );
        if (runtimeGreaterMatch.Success)
        {
            var runtime = int.Parse(runtimeGreaterMatch.Groups[1].Value);
            Console.WriteLine($"[LLM] Aplicando filtro: Runtime > {runtime}");
            result = result.Where(m => m.Runtime > runtime);
        }

        var finalResults = result.ToList();
        Console.WriteLine($"[LLM] Resultados después de aplicar filtros: {finalResults.Count}");
        
        return finalResults;
    }

    private static string NormalizeGenre(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return raw;
        var key = raw.Trim().ToLowerInvariant().Replace("-", " ");
        return key switch
        {
            "acción" => "Action",
            "action" => "Action",
            "ciencia ficción" => "Sci-Fi",
            "science fiction" => "Sci-Fi",
            "scifi" => "Sci-Fi",
            "sci fi" => "Sci-Fi",
            "crime" => "Crime",
            "crimen" => "Crime",
            "drama" => "Drama",
            "comedia" => "Comedy",
            "comedy" => "Comedy",
            "terror" => "Horror",
            "horror" => "Horror",
            "romance" => "Romance",
            _ => raw
        };
    }
}

public class LLMQueryResult
{
    public bool Success { get; set; }
    public string LinqQuery { get; set; } = string.Empty;
    public string RawResponse { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
}

// Clases para deserializar la respuesta de OpenRouter
public class OpenRouterResponse
{
    public List<Choice>? Choices { get; set; }
}

public class Choice
{
    public Message? Message { get; set; }
}

public class Message
{
    public string? Content { get; set; }
}