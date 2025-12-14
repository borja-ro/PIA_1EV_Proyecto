using Microsoft.EntityFrameworkCore;
using MovieManager.Core.Interfaces;
using MovieManager.Core.Models;
using MovieManager.Infrastructure.Data;
using MovieManager.Infrastructure.Repositories;
using MovieManager.MCP.Models;
using MovieManager.MCP.Routers;
using MovieManager.MCP.Services;

var builder = WebApplication.CreateBuilder(args);

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Leer tipo de repositorio desde configuración
var repositoryType = builder.Configuration["RepositoryType"] ?? "Memory";
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (repositoryType == "Sqlite")
{
    Console.WriteLine($"[CONFIG] Usando SQLite: {connectionString}");
    
    // Registrar DbContext
    builder.Services.AddDbContext<MovieDbContext>(options =>
        options.UseSqlite(connectionString));
    
    // Registrar SqliteRepository
    builder.Services.AddScoped<IRepository<Movie>, SqliteRepository>();
}
else
{
    Console.WriteLine("[CONFIG] Usando MemoryRepository (en RAM)");
    builder.Services.AddSingleton<IRepository<Movie>, MemoryRepository<Movie>>();
}

// Configurar OpenRouter API Key desde configuración
var openRouterApiKey = builder.Configuration["OpenRouter:ApiKey"] ?? 
                       Environment.GetEnvironmentVariable("OPENROUTER_API_KEY") ??
                       throw new InvalidOperationException("OpenRouter API Key no configurada");

var openRouterModel = builder.Configuration["OpenRouter:Model"] ?? "anthropic/claude-sonnet-4.5";

// Registrar servicios
builder.Services.AddSingleton(sp => new LLMRouter(openRouterApiKey, openRouterModel));
builder.Services.AddScoped<QueryProcessor>();

var app = builder.Build();

// Asegurar que la base de datos existe (solo si usamos SQLite)
if (repositoryType == "Sqlite")
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<MovieDbContext>();
        dbContext.Database.EnsureCreated();
        Console.WriteLine("[INFO] Base de datos SQLite inicializada");
    }
}

app.UseCors("AllowAll");

// Endpoint de salud
app.MapGet("/", () => new
{
    service = "MovieManager MCP Server",
    version = "1.0",
    status = "running",
    endpoints = new
    {
        health = "GET /health",
        query = "POST /query"
    }
});

app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

// Endpoint principal para consultas
app.MapPost("/query", async (QueryRequest request, QueryProcessor processor) =>
{
    Console.WriteLine($"\n{'=',-60}");
    Console.WriteLine($"[MCP] Nueva consulta recibida: {request.Query}");
    Console.WriteLine($"{'=',-60}");
    
    var response = await processor.ProcessQuery(request.Query);
    
    Console.WriteLine($"[MCP] Respuesta: {response.Message}");
    Console.WriteLine($"{'=',-60}\n");
    
    return Results.Ok(response);
});

// Endpoint para cargar datos de prueba (hardcodeados)
app.MapPost("/load-test-data", async (IRepository<Movie> repository) =>
{
    var testMovies = new List<Movie>
    {
        new() { Title = "Inception", Year = 2010, Genre = "Sci-Fi", Rating = 8.8, 
                Director = "Christopher Nolan", Runtime = 148, 
                Overview = "A thief who steals corporate secrets through dream-sharing technology." },
        new() { Title = "The Dark Knight", Year = 2008, Genre = "Action", Rating = 9.0, 
                Director = "Christopher Nolan", Runtime = 152, 
                Overview = "Batman faces the Joker in Gotham City." },
        new() { Title = "Interstellar", Year = 2014, Genre = "Sci-Fi", Rating = 8.6, 
                Director = "Christopher Nolan", Runtime = 169, 
                Overview = "A team of explorers travel through a wormhole in space." },
        new() { Title = "Pulp Fiction", Year = 1994, Genre = "Crime", Rating = 8.9, 
                Director = "Quentin Tarantino", Runtime = 154, 
                Overview = "The lives of two mob hitmen, a boxer, and other criminals intertwine." },
        new() { Title = "The Matrix", Year = 1999, Genre = "Sci-Fi", Rating = 8.7, 
                Director = "Wachowski Brothers", Runtime = 136, 
                Overview = "A computer hacker learns about the true nature of reality." }
    };
    
    foreach (var movie in testMovies)
    {
        await repository.AddAsync(movie);
    }
    
    return Results.Ok(new 
    { 
        success = true,
        message = "Datos de prueba cargados",
        count = testMovies.Count
    });
});

// Endpoint para cargar dataset completo desde CSV
app.MapPost("/load-data", async (IRepository<Movie> repository) =>
{
    try
    {
        // Construir ruta al CSV desde la raíz del proyecto
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var projectRoot = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", ".."));
        var csvPath = Path.Combine(projectRoot, "Data", "movies.csv");
        
        Console.WriteLine($"[DEBUG] Buscando CSV en: {csvPath}");
        
        if (!File.Exists(csvPath))
        {
            Console.WriteLine($"[ERROR] CSV no encontrado");
            return Results.NotFound(new 
            { 
                success = false,
                message = $"CSV no encontrado en: {csvPath}",
                count = 0 
            });
        }
        
        Console.WriteLine($"[INFO] CSV encontrado, cargando...");
        var movies = await CsvLoader.LoadFromCsvAsync(csvPath);
        
        Console.WriteLine($"[INFO] {movies.Count} películas leídas del CSV");
        
        foreach (var movie in movies)
        {
            await repository.AddAsync(movie);
        }
        
        Console.WriteLine($"[SUCCESS] {movies.Count} películas cargadas en el repositorio");
        
        return Results.Ok(new 
        { 
            success = true,
            message = "Dataset completo cargado correctamente",
            count = movies.Count,
            repositoryType = repository.GetType().Name
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] Excepción: {ex.Message}");
        return Results.Problem($"Error: {ex.Message}");
    }
});

Console.WriteLine(@"
╔══════════════════════════════════════════════════════════╗
║                                                          ║
║          MovieManager MCP Server v1.0                    ║
║          Model Context Protocol + LLM Router             ║
║                                                          ║
╚══════════════════════════════════════════════════════════╝

🚀 Servidor iniciado en: http://0.0.0.0:5001
📡 Endpoints disponibles:
   • GET  /              - Info del servidor
   • GET  /health        - Estado del servidor
   • POST /query         - Procesar consulta en lenguaje natural
   • POST /load-data     - Cargar dataset completo (1000 películas)
   • POST /load-test-data - Cargar 5 películas de prueba

💡 Ejemplo de uso:
   POST /query
   { ""query"": ""películas de Nolan del 2010"" }

⚙️  OpenRouter Model: " + openRouterModel + @"
");

app.Run("http://0.0.0.0:5001");