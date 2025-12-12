using MovieManager.Core.Models;

namespace MovieManager.MCP.Models;

public class QueryResponse
{
    public bool Success { get; set; }
    public List<Movie> Results { get; set; } = new();
    public string Source { get; set; } = string.Empty; // "rule" o "llm"
    public string Message { get; set; } = string.Empty;
    public int Count { get; set; }
}