using System.Text.RegularExpressions;
using MovieManager.Core.Models;

namespace MovieManager.MCP.Routers;

public class RuleRouter
{
    private readonly List<QueryRule> _rules;

    public RuleRouter()
    {
        _rules = new List<QueryRule>
        {
            // ⚠️ IMPORTANTE: Reglas específicas PRIMERO

            // Regla: "películas/director [nombre] después de/antes de [año]"
            new QueryRule(
                pattern: @"(director\s+)?([a-zA-Z\s]+)?\s*(después|despues|after|antes|before)\s+(?:de\s+)?(\d{4})",
                filter: (movies, match) =>
                {
                    var director = match.Groups[2].Value.Trim();
                    var comparator = match.Groups[3].Value.ToLower();
                    var year = int.Parse(match.Groups[4].Value);
                    
                    var filtered = movies.AsEnumerable();
                    
                    // Aplicar filtro de director si existe
                    if (!string.IsNullOrWhiteSpace(director) && director.Length > 2)
                    {
                        filtered = filtered.Where(m => m.Director.Contains(director, StringComparison.OrdinalIgnoreCase));
                    }
                    
                    // Aplicar filtro temporal
                    if (comparator.Contains("despues") || comparator.Contains("después") || comparator.Contains("after"))
                    {
                        filtered = filtered.Where(m => m.Year > year);
                    }
                    else if (comparator.Contains("antes") || comparator.Contains("before"))
                    {
                        filtered = filtered.Where(m => m.Year < year);
                    }
                    
                    return filtered;
                },
                description: "Buscar por director y año con comparador"
            ),

            // Regla: "películas de [año1] a [año2]" o "entre [año1] y [año2]"
            new QueryRule(
                pattern: @"(?:de|desde|from)\s+(\d{4})\s+(?:a|hasta|to|y|and)\s+(\d{4})",
                filter: (movies, match) =>
                {
                    var startYear = int.Parse(match.Groups[1].Value);
                    var endYear = int.Parse(match.Groups[2].Value);
                    return movies.Where(m => m.Year >= startYear && m.Year <= endYear);
                },
                description: "Buscar por rango de años"
            ),

            // Regla: "películas de [director] en [año]" (específica)
            new QueryRule(
                pattern: @"director\s+([a-zA-Z\s]+).*?en.*?(\d{4})",
                filter: (movies, match) =>
                {
                    var director = match.Groups[1].Value.Trim();
                    var year = int.Parse(match.Groups[2].Value);
                    return movies.Where(m => 
                        m.Director.Contains(director, StringComparison.OrdinalIgnoreCase) && 
                        m.Year == year);
                },
                description: "Buscar por director y año exacto"
            ),

            // Regla: "películas de [año]" (genérica)
            new QueryRule(
                pattern: @"(películas?|movies?|films?).*?(\d{4})",
                filter: (movies, match) => 
                {
                    var year = int.Parse(match.Groups[2].Value);
                    return movies.Where(m => m.Year == year);
                },
                description: "Buscar por año"
            ),

            // Regla: "director [nombre]" (sin año)
            new QueryRule(
                pattern: @"director\s+([a-zA-Z\s]+)",
                filter: (movies, match) =>
                {
                    var director = match.Groups[1].Value.Trim();
                    return movies.Where(m => m.Director.Contains(director, StringComparison.OrdinalIgnoreCase));
                },
                description: "Buscar por director"
            ),

            // Resto de reglas...

            // Regla: "[género] movies/películas" (español e inglés)
            new QueryRule(
                pattern: @"(drama|action|acción|comedy|comedia|thriller|horror|terror|sci-?fi|ciencia ficción|romance|crime|crimen|adventure|aventura).*?(películas?|movies?|films?)",
                filter: (movies, match) =>
                {
                    var genre = match.Groups[1].Value;
                    // Normalizar géneros en español a inglés
                    var normalizedGenre = genre.ToLower() switch
                    {
                        "acción" => "Action",
                        "comedia" => "Comedy",
                        "terror" => "Horror",
                        "ciencia ficción" => "Sci-Fi",
                        "scifi" => "Sci-Fi",
                        "sci-fi" => "Sci-Fi",
                        "sci fi" => "Sci-Fi",
                        "crimen" => "Crime",
                        "aventura" => "Adventure",
                        _ => genre
                    };
                    return movies.Where(m => m.Genre.Contains(normalizedGenre, StringComparison.OrdinalIgnoreCase));
                },
                description: "Buscar por género"
            ),

            // Regla: "películas con rating mayor/menor que [número]"
            new QueryRule(
                pattern: @"rating\s*(mayor|menor|>|<|>=|<=)\s*(?:que\s*)?(\d+\.?\d*)",
                filter: (movies, match) =>
                {
                    var op = match.Groups[1].Value;
                    var rating = double.Parse(match.Groups[2].Value);
                    
                    return op switch
                    {
                        "mayor" or ">" or ">=" => movies.Where(m => m.Rating >= rating),
                        "menor" or "<" or "<=" => movies.Where(m => m.Rating <= rating),
                        _ => movies
                    };
                },
                description: "Buscar por rating"
            ),
            
        };
    }

    public RuleMatchResult TryMatch(string query, IEnumerable<Movie> movies)
    {
        query = query.ToLower();

        foreach (var rule in _rules)
        {
            var match = Regex.Match(query, rule.Pattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var filtered = rule.Filter(movies, match).ToList();
                return new RuleMatchResult
                {
                    Success = true,
                    Results = filtered,
                    RuleDescription = rule.Description
                };
            }
        }

        return new RuleMatchResult { Success = false };
    }
}

public class QueryRule
{
    public string Pattern { get; }
    public Func<IEnumerable<Movie>, Match, IEnumerable<Movie>> Filter { get; }
    public string Description { get; }

    public QueryRule(string pattern, Func<IEnumerable<Movie>, Match, IEnumerable<Movie>> filter, string description)
    {
        Pattern = pattern;
        Filter = filter;
        Description = description;
    }
}

public class RuleMatchResult
{
    public bool Success { get; set; }
    public List<Movie> Results { get; set; } = new();
    public string RuleDescription { get; set; } = string.Empty;
}