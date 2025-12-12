using System.Text;
using System.Text.Json;
using MovieManager.Core.Interfaces;
using MovieManager.Core.Models;
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

// Configurar repositorio (usar In-Memory por simplicidad)
builder.Services.AddSingleton<IRepository<Movie>, MemoryRepository<Movie>>();
// Configurar OpenRouter API Key desde configuración
var openRouterApiKey = builder.Configuration["OpenRouter:ApiKey"] ?? 
                       Environment.GetEnvironmentVariable("OPENROUTER_API_KEY") ??
                       throw new InvalidOperationException("OpenRouter API Key no configurada");

var openRouterModel = builder.Configuration["OpenRouter:Model"] ?? "anthropic/claude-3.5-sonnet";

// Registrar servicios
builder.Services.AddSingleton(sp => new LLMRouter(openRouterApiKey, openRouterModel));
builder.Services.AddScoped<QueryProcessor>();

var app = builder.Build();

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

// Endpoint para cargar datos de prueba
app.MapPost("/load-test-data", async (IRepository<Movie> repository) =>
{
    // Cargar algunas películas de prueba
    var testMovies = new List<Movie>
    {
        new Movie
        {
            Title = "Inception",
            Year = 2010,
            Genre = "Sci-Fi",
            Rating = 8.8,
            Director = "Christopher Nolan",
            Runtime = 148,
            Overview = "A thief who steals corporate secrets through dream-sharing technology."
        },
        new Movie
        {
            Title = "The Dark Knight",
            Year = 2008,
            Genre = "Action",
            Rating = 9.0,
            Director = "Christopher Nolan",
            Runtime = 152,
            Overview = "Batman faces the Joker in Gotham City."
        },
        new Movie
        {
            Title = "Interstellar",
            Year = 2014,
            Genre = "Sci-Fi",
            Rating = 8.6,
            Director = "Christopher Nolan",
            Runtime = 169,
            Overview = "A team of explorers travel through a wormhole in space."
        },
        new Movie
        {
            Title = "Pulp Fiction",
            Year = 1994,
            Genre = "Crime",
            Rating = 8.9,
            Director = "Quentin Tarantino",
            Runtime = 154,
            Overview = "The lives of two mob hitmen, a boxer, and a pair of diner bandits intertwine."
        },
        new Movie
        {
            Title = "The Shawshank Redemption",
            Year = 1994,
            Genre = "Drama",
            Rating = 9.3,
            Director = "Frank Darabont",
            Runtime = 142,
            Overview = "Two imprisoned men bond over years, finding redemption."
        }
    };

    foreach (var movie in testMovies)
    {
        await repository.AddAsync(movie);
    }

    return Results.Ok(new { message = "Datos de prueba cargados", count = testMovies.Count });
});

Console.WriteLine(@"
╔══════════════════════════════════════════════════════════╗
║                                                          ║
║          MovieManager MCP Server v1.0                    ║
║          Model Context Protocol + LLM Router             ║
║                                                          ║
╚══════════════════════════════════════════════════════════╝

🚀 Servidor iniciado en: http://localhost:5001
📡 Endpoints disponibles:
   • GET  /           - Info del servidor
   • GET  /health     - Estado del servidor
   • POST /query      - Procesar consulta en lenguaje natural
   • POST /load-test-data - Cargar datos de prueba

💡 Ejemplo de uso:
   POST /query
   { ""query"": ""películas de Nolan del 2010"" }

⚙️  OpenRouter Model: " + openRouterModel + @"
");

app.Run("http://localhost:5001");