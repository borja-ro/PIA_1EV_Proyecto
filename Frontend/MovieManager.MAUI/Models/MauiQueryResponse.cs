using MovieManager.Core.Models;

namespace MovieManager.MAUI.Models;

public class MauiQueryResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public int Count { get; set; }
    public List<Movie> Results { get; set; } = new();
}
