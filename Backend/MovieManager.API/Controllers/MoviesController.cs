using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieManager.Core.DTOs;
using MovieManager.Core.Interfaces;
using MovieManager.Core.Models;

namespace MovieManager.API.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MoviesController : ControllerBase
{
    private readonly IRepository<Movie> _repository;

    public MoviesController(IRepository<Movie> repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Get all movies
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MovieDto>>> GetAll()
    {
        var movies = await _repository.GetAllAsync();
        var moviesDto = movies.Select(m => new MovieDto
        {
            Id = m.Id,
            Title = m.Title,
            Year = m.Year,
            Genre = m.Genre,
            Rating = m.Rating,
            Director = m.Director,
            Runtime = m.Runtime,
            Overview = m.Overview,
            Certificate = m.Certificate,
            PosterUrl = m.PosterUrl,
            Star1 = m.Star1
        });

        return Ok(moviesDto);
    }

    /// <summary>
    /// Get movie by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<MovieDto>> GetById(int id)
    {
        var movie = await _repository.GetByIdAsync(id);
        
        if (movie == null)
            return NotFound(new { message = $"Movie with ID {id} not found" });

        var movieDto = new MovieDto
        {
            Id = movie.Id,
            Title = movie.Title,
            Year = movie.Year,
            Genre = movie.Genre,
            Rating = movie.Rating,
            Director = movie.Director,
            Runtime = movie.Runtime,
            Overview = movie.Overview,
            Certificate = movie.Certificate,
            PosterUrl = movie.PosterUrl,
            Star1 = movie.Star1
        };

        return Ok(movieDto);
    }

    /// <summary>
    /// Create a new movie
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<MovieDto>> Create([FromBody] CreateMovieDto createDto)
    {
        var movie = new Movie
        {
            Title = createDto.Title,
            Year = createDto.Year,
            Genre = createDto.Genre,
            Rating = createDto.Rating,
            Director = createDto.Director,
            Runtime = createDto.Runtime,
            Overview = createDto.Overview,
            Certificate = createDto.Certificate,
            PosterUrl = createDto.PosterUrl,
            Star1 = createDto.Star1,
            CreatedBy = User.Identity?.Name ?? "Anonymous"
        };

        var created = await _repository.AddAsync(movie);

        var movieDto = new MovieDto
        {
            Id = created.Id,
            Title = created.Title,
            Year = created.Year,
            Genre = created.Genre,
            Rating = created.Rating,
            Director = created.Director,
            Runtime = created.Runtime,
            Overview = created.Overview,
            Certificate = created.Certificate,
            PosterUrl = created.PosterUrl,
            Star1 = created.Star1
        };

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, movieDto);
    }

    /// <summary>
    /// Update an existing movie
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<MovieDto>> Update(int id, [FromBody] UpdateMovieDto updateDto)
    {
        var existing = await _repository.GetByIdAsync(id);
        
        if (existing == null)
            return NotFound(new { message = $"Movie with ID {id} not found" });

        // Update only provided fields
        if (updateDto.Title != null) existing.Title = updateDto.Title;
        if (updateDto.Year.HasValue) existing.Year = updateDto.Year.Value;
        if (updateDto.Genre != null) existing.Genre = updateDto.Genre;
        if (updateDto.Rating.HasValue) existing.Rating = updateDto.Rating.Value;
        if (updateDto.Director != null) existing.Director = updateDto.Director;
        if (updateDto.Runtime.HasValue) existing.Runtime = updateDto.Runtime.Value;
        if (updateDto.Overview != null) existing.Overview = updateDto.Overview;
        if (updateDto.Certificate != null) existing.Certificate = updateDto.Certificate;
        if (updateDto.PosterUrl != null) existing.PosterUrl = updateDto.PosterUrl;
        if (updateDto.Star1 != null) existing.Star1 = updateDto.Star1;

        var updated = await _repository.UpdateAsync(existing);

        var movieDto = new MovieDto
        {
            Id = updated.Id,
            Title = updated.Title,
            Year = updated.Year,
            Genre = updated.Genre,
            Rating = updated.Rating,
            Director = updated.Director,
            Runtime = updated.Runtime,
            Overview = updated.Overview,
            Certificate = updated.Certificate,
            PosterUrl = updated.PosterUrl,
            Star1 = updated.Star1
        };

        return Ok(movieDto);
    }

    /// <summary>
    /// Delete a movie
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _repository.DeleteAsync(id);
        
        if (!success)
            return NotFound(new { message = $"Movie with ID {id} not found" });

        return NoContent();
    }

    /// <summary>
    /// Get total count of movies
    /// </summary>
    [HttpGet("count")]
    public async Task<ActionResult<int>> GetCount()
    {
        var count = await _repository.CountAsync();
        return Ok(new { count });
    }

    /// <summary>
    /// Load movies from CSV file
    /// </summary>
    [HttpPost("load-csv")]
    public async Task<IActionResult> LoadFromCsv()
    {
        var csvPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "Data", "movies.csv");
        
        if (!System.IO.File.Exists(csvPath))
            return NotFound(new { message = $"CSV file not found at {csvPath}" });

        await _repository.LoadFromCsvAsync(csvPath);
        var count = await _repository.CountAsync();

        return Ok(new { message = $"Successfully loaded {count} movies from CSV" });
    }
}