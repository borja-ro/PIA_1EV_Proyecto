using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using MovieManager.Core.Models;

namespace MovieManager.Infrastructure.Data;

/// <summary>
/// Helper class to load movies from CSV files using CsvHelper library
/// </summary>
public static class CsvLoader
{
    /// <summary>
    /// Loads movies from a CSV file
    /// </summary>
    /// <param name="filePath">Path to the CSV file</param>
    /// <returns>List of movies</returns>
    public static async Task<List<Movie>> LoadFromCsvAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"CSV file not found: {filePath}");
        }

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            MissingFieldFound = null, // Ignore missing fields
            HeaderValidated = null,   // Don't validate headers
            TrimOptions = TrimOptions.Trim
        };

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, config);
        
        csv.Context.RegisterClassMap<MovieCsvRecordMap>();

        var movies = new List<Movie>();
        var records = csv.GetRecords<MovieCsvRecord>();
        
        int id = 1;
        foreach (var record in records)
        {
            movies.Add(new Movie
            {
                Id = id++,
                Title = record.Title,
                Year = record.Year,
                Genre = record.Genre,
                Rating = record.Rating,
                Director = record.Director,
                Runtime = record.Runtime,
                Overview = record.Overview,
                Certificate = record.Certificate,
                PosterUrl = record.PosterUrl,
                Star1 = record.Star1,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "System"
            });
        }

        return await Task.FromResult(movies);
    }

    /// <summary>
    /// Private class to map CSV columns to Movie properties
    /// </summary>
    private class MovieCsvRecord
    {
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
    
    /// <summary>
    /// Defines the mapping between the CSV file and the MovieCsvRecord class.
    /// </summary>
    private sealed class MovieCsvRecordMap : ClassMap<MovieCsvRecord>
    {
        public MovieCsvRecordMap()
        {
            Map(m => m.PosterUrl).Name("PosterUrl");
            Map(m => m.Title).Name("Title");
            Map(m => m.Year).Name("Year");
            Map(m => m.Runtime).Name("Runtime");
            Map(m => m.Genre).Name("Genre");
            Map(m => m.Rating).Name("Rating");
            Map(m => m.Overview).Name("Overview");
            Map(m => m.Director).Name("Director");
            Map(m => m.Star1).Name("Star1");
        }
    }
}