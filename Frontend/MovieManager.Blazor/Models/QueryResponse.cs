namespace MovieManager.Blazor.Models;

public class QueryResponse
{
    public bool Success { get; set; }
    public List<Movie> Results { get; set; } = new();
    public string Source { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int Count { get; set; }
}