using Microsoft.EntityFrameworkCore;
using MovieManager.Core.Interfaces;
using MovieManager.Core.Models;
using MovieManager.Infrastructure.Data;

namespace MovieManager.Infrastructure.Repositories;

/// <summary>
/// SQLite implementation of IRepository for Movie entities using Entity Framework Core
/// </summary>
public class SqliteRepository : IRepository<Movie>
{
    private readonly MovieDbContext _context;
    private readonly DbSet<Movie> _dbSet;

    public SqliteRepository(MovieDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<Movie>();
    }

    public async Task<IEnumerable<Movie>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<Movie?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<Movie> AddAsync(Movie entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<Movie> UpdateAsync(Movie entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _dbSet.Update(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await GetByIdAsync(id);
        if (entity == null)
            return false;

        _dbSet.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task LoadFromCsvAsync(string filePath)
    {
        var movies = await CsvLoader.LoadFromCsvAsync(filePath);

        // Clear existing data
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM Movies");

        // Add new data
        await _dbSet.AddRangeAsync(movies);
        
        await _context.SaveChangesAsync();
    }

    public async Task<int> CountAsync()
    {
        return await _dbSet.CountAsync();
    }
}