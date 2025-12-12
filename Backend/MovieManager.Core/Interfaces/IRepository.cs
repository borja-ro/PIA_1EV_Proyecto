namespace MovieManager.Core.Interfaces;

/// <summary>
/// Contrato genérico para operaciones de persistencia.
/// Implementa el principio Open/Close: abierto para extensión, cerrado para modificación.
/// </summary>
/// <typeparam name="T">Tipo de entidad a gestionar</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Obtiene todas las entidades de forma asíncrona
    /// </summary>
    /// <returns>Colección de entidades</returns>
    Task<IEnumerable<T>> GetAllAsync();
    
    /// <summary>
    /// Obtiene una entidad por su identificador
    /// </summary>
    /// <param name="id">Identificador de la entidad</param>
    /// <returns>La entidad si existe, null en caso contrario</returns>
    Task<T?> GetByIdAsync(int id);
    
    /// <summary>
    /// Añade una nueva entidad al repositorio
    /// </summary>
    /// <param name="entity">Entidad a añadir</param>
    /// <returns>La entidad añadida con su ID asignado</returns>
    Task<T> AddAsync(T entity);
    
    /// <summary>
    /// Actualiza una entidad existente
    /// </summary>
    /// <param name="entity">Entidad con los datos actualizados</param>
    /// <returns>La entidad actualizada</returns>
    Task<T> UpdateAsync(T entity);
    
    /// <summary>
    /// Elimina una entidad por su identificador
    /// </summary>
    /// <param name="id">Identificador de la entidad a eliminar</param>
    /// <returns>True si se eliminó correctamente, false si no existía</returns>
    Task<bool> DeleteAsync(int id);
    
    /// <summary>
    /// Carga datos desde un archivo CSV
    /// </summary>
    /// <param name="filePath">Ruta completa al archivo CSV</param>
    Task LoadFromCsvAsync(string filePath);
    
    /// <summary>
    /// Obtiene el número total de entidades
    /// </summary>
    /// <returns>Cantidad de entidades</returns>
    Task<int> CountAsync();
}