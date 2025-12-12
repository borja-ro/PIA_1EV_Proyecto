using System.ComponentModel.DataAnnotations;

namespace MovieManager.Core.DTOs;

/// <summary>
/// DTO para actualizar una película existente.
/// Todos los campos son opcionales para permitir actualizaciones parciales.
/// </summary>
public class UpdateMovieDto
{
    [StringLength(200)]
    public string? Title { get; set; }
    
    [Range(1888, 2030)]
    public int? Year { get; set; }
    
    [StringLength(100)]
    public string? Genre { get; set; }
    
    [Range(0, 10)]
    public double? Rating { get; set; }
    
    [StringLength(100)]
    public string? Director { get; set; }
    
    [Range(1, 600)]
    public int? Runtime { get; set; }
    
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