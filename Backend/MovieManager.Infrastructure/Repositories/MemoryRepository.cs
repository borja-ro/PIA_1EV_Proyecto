using MovieManager.Core.Interfaces;
using MovieManager.Infrastructure.Data;

namespace MovieManager.Infrastructure.Repositories;

/// <summary>
/// In-memory implementation of IRepository using a static List
/// Thread-safe for async operations
/// </summary>
public class MemoryRepository<T> : IRepository<T> where T : class
{
    private static readonly List<T> _data = new();
    private static readonly SemaphoreSlim _semaphore = new(1, 1);
    private static int _nextId = 1;

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            return _data.ToList(); // Return a copy to avoid external modifications
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        await _semaphore.WaitAsync();
        try
        {
            var idProperty = typeof(T).GetProperty("Id");
            if (idProperty == null)
                throw new InvalidOperationException($"Type {typeof(T).Name} does not have an Id property");

            return _data.FirstOrDefault(item =>
            {
                var itemId = idProperty.GetValue(item);
                return itemId != null && (int)itemId == id;
            });
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<T> AddAsync(T entity)
    {
        await _semaphore.WaitAsync();
        try
        {
            var idProperty = typeof(T).GetProperty("Id");
            if (idProperty == null)
                throw new InvalidOperationException($"Type {typeof(T).Name} does not have an Id property");

            // Auto-assign ID
            idProperty.SetValue(entity, _nextId++);

            // Set CreatedAt if exists
            var createdAtProperty = typeof(T).GetProperty("CreatedAt");
            if (createdAtProperty != null && createdAtProperty.PropertyType == typeof(DateTime))
            {
                createdAtProperty.SetValue(entity, DateTime.UtcNow);
            }

            _data.Add(entity);
            return entity;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<T> UpdateAsync(T entity)
    {
        await _semaphore.WaitAsync();
        try
        {
            var idProperty = typeof(T).GetProperty("Id");
            if (idProperty == null)
                throw new InvalidOperationException($"Type {typeof(T).Name} does not have an Id property");

            var entityId = (int?)idProperty.GetValue(entity);
            if (entityId == null)
                throw new ArgumentException("Entity Id cannot be null");

            var existingIndex = _data.FindIndex(item =>
            {
                var itemId = idProperty.GetValue(item);
                return itemId != null && (int)itemId == entityId;
            });

            if (existingIndex == -1)
                throw new KeyNotFoundException($"Entity with Id {entityId} not found");

            // Set UpdatedAt if exists
            var updatedAtProperty = typeof(T).GetProperty("UpdatedAt");
            if (updatedAtProperty != null && updatedAtProperty.PropertyType == typeof(DateTime?))
            {
                updatedAtProperty.SetValue(entity, DateTime.UtcNow);
            }

            _data[existingIndex] = entity;
            return entity;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await _semaphore.WaitAsync();
        try
        {
            var idProperty = typeof(T).GetProperty("Id");
            if (idProperty == null)
                throw new InvalidOperationException($"Type {typeof(T).Name} does not have an Id property");

            var itemToRemove = _data.FirstOrDefault(item =>
            {
                var itemId = idProperty.GetValue(item);
                return itemId != null && (int)itemId == id;
            });

            if (itemToRemove == null)
                return false;

            _data.Remove(itemToRemove);
            return true;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task LoadFromCsvAsync(string filePath)
    {
        if (typeof(T).Name != "Movie")
            throw new NotSupportedException("CSV loading is only supported for Movie entities");

        await _semaphore.WaitAsync();
        try
        {
            var movies = await CsvLoader.LoadFromCsvAsync(filePath);
            
            _data.Clear();
            _nextId = 1;

            foreach (var movie in movies)
            {
                _data.Add((movie as T)!);
                _nextId++;
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<int> CountAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            return _data.Count;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}