using Microsoft.EntityFrameworkCore;
using MovieManager.Core.Interfaces;
using MovieManager.Core.Models;
using MovieManager.Infrastructure.Data;

namespace MovieManager.Infrastructure.Repositories;

/// <summary>
/// SQLite implementation of IRepository using Entity Framework Core
/// </summary>
public class SqliteRepository<T> : IRepository<T> where T : class
{
    private readonly MovieDbContext _context;
    private readonly DbSet<T> _dbSet;

    public SqliteRepository(MovieDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        var idProperty = typeof(T).GetProperty("Id");
        if (idProperty == null)
            throw new InvalidOperationException($"Type {typeof(T).Name} does not have an Id property");

        return await _dbSet.FirstOrDefaultAsync(item =>
            EF.Property<int>(item, "Id") == id);
    }

    public async Task<T> AddAsync(T entity)
    {
        // Set CreatedAt if exists
        var createdAtProperty = typeof(T).GetProperty("CreatedAt");
        if (createdAtProperty != null && createdAtProperty.PropertyType == typeof(DateTime))
        {
            createdAtProperty.SetValue(entity, DateTime.UtcNow);
        }

        await _dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<T> UpdateAsync(T entity)
    {
        // Set UpdatedAt if exists
        var updatedAtProperty = typeof(T).GetProperty("UpdatedAt");
        if (updatedAtProperty != null && updatedAtProperty.PropertyType == typeof(DateTime?))
        {
            updatedAtProperty.SetValue(entity, DateTime.UtcNow);
        }

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
        if (typeof(T).Name != "Movie")
            throw new NotSupportedException("CSV loading is only supported for Movie entities");

        var movies = await CsvLoader.LoadFromCsvAsync(filePath);

        // Clear existing data
        _dbSet.RemoveRange(_dbSet);
        await _context.SaveChangesAsync();

        // Add new data
        foreach (var movie in movies)
        {
            await _dbSet.AddAsync((movie as T)!);
        }
        
        await _context.SaveChangesAsync();
    }

    public async Task<int> CountAsync()
    {
        return await _dbSet.CountAsync();
    }
}