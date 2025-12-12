using Microsoft.EntityFrameworkCore;
using MovieManager.Core.Models;

namespace MovieManager.Infrastructure.Data;

/// <summary>
/// Entity Framework DbContext for SQLite database
/// </summary>
public class MovieDbContext : DbContext
{
    public MovieDbContext(DbContextOptions<MovieDbContext> options) : base(options)
    {
    }

    public DbSet<Movie> Movies { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Movie entity
        modelBuilder.Entity<Movie>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Year)
                .IsRequired();

            entity.Property(e => e.Genre)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Rating)
                .IsRequired();

            entity.Property(e => e.Director)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Runtime)
                .IsRequired();

            entity.Property(e => e.Overview)
                .HasMaxLength(1000);

            entity.Property(e => e.Certificate)
                .HasMaxLength(10);

            entity.Property(e => e.PosterUrl)
                .HasMaxLength(500);

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.CreatedBy)
                .HasMaxLength(100);

            // Indexes for better query performance
            entity.HasIndex(e => e.Year);
            entity.HasIndex(e => e.Genre);
            entity.HasIndex(e => e.Rating);
            entity.HasIndex(e => e.Director);
        });
    }
}