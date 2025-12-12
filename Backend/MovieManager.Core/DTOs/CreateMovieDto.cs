using System.ComponentModel.DataAnnotations;

namespace MovieManager.Core.DTOs;

/// <summary>
/// DTO para crear una nueva película
/// </summary>
public class CreateMovieDto
{
    [Required(ErrorMessage = "El título es obligatorio")]
    [StringLength(200, ErrorMessage = "El título no puede exceder 200 caracteres")]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [Range(1888, 2030, ErrorMessage = "Año debe estar entre 1888 y 2030")]
    public int Year { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Genre { get; set; } = string.Empty;
    
    [Required]
    [Range(0, 10, ErrorMessage = "Rating debe estar entre 0 y 10")]
    public double Rating { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Director { get; set; } = string.Empty;
    
    [Range(1, 600, ErrorMessage = "Runtime debe estar entre 1 y 600 minutos")]
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
}