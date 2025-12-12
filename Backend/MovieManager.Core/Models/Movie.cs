using System.ComponentModel.DataAnnotations;

namespace MovieManager.Core.Models;

/// <summary>
/// Represents a movie entity
/// </summary>
public class Movie
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [Range(1888, 2030)]
    public int Year { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Genre { get; set; } = string.Empty;
    
    [Required]
    [Range(0, 10)]
    public double Rating { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Director { get; set; } = string.Empty;
    
    [Required]
    [Range(1, 600)]
    public int Runtime { get; set; }
    
    [StringLength(1000)]
    public string? Overview { get; set; }
    
    [StringLength(10)]
    public string? Certificate { get; set; }
    
    [Url]
    [StringLength(500)]
    public string? PosterUrl { get; set; }
    
    [StringLength(100)]
    public string? Star1 { get; set; }
    
    // Auditoría
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
}