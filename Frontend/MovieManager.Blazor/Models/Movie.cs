namespace MovieManager.Blazor.Models;

public class Movie
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Genre { get; set; } = string.Empty;
    public double Rating { get; set; }
    public string Director { get; set; } = string.Empty;
    public int Runtime { get; set; }
    public string? Overview { get; set; }
}