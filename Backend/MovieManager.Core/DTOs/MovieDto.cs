namespace MovieManager.Core.DTOs;

/// <summary>
/// DTO para representar una película en las respuestas de la API
/// </summary>
public class MovieDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Genre { get; set; } = string.Empty;
    public double Rating { get; set; }
    public string Director { get; set; } = string.Empty;
    public int Runtime { get; set; }
    public string? Overview { get; set; }
    public string? Certificate { get; set; }
    public string? PosterUrl { get; set; }
    public string? Star1 { get; set; }
}