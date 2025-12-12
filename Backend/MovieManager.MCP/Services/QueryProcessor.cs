using MovieManager.Core.Interfaces;
using MovieManager.Core.Models;
using MovieManager.MCP.Models;
using MovieManager.MCP.Routers;

namespace MovieManager.MCP.Services;

public class QueryProcessor
{
    private readonly RuleRouter _ruleRouter;
    private readonly LLMRouter _llmRouter;
    private readonly IRepository<Movie> _repository;

    public QueryProcessor(IRepository<Movie> repository, LLMRouter llmRouter)
    {
        _repository = repository;
        _ruleRouter = new RuleRouter();
        _llmRouter = llmRouter;
    }

    public async Task<QueryResponse> ProcessQuery(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return new QueryResponse
            {
                Success = false,
                Message = "La consulta no puede estar vacía"
            };
        }

        Console.WriteLine($"[MCP] Procesando consulta: {query}");

        // Obtener todas las películas de forma asíncrona
        var allMovies = (await _repository.GetAllAsync()).ToList();
        Console.WriteLine($"[MCP] Total de películas en repositorio: {allMovies.Count}");

        // PASO 1: Intentar con RuleRouter primero
        Console.WriteLine("[MCP] Intentando RuleRouter...");
        var ruleResult = _ruleRouter.TryMatch(query, allMovies);
        
        if (ruleResult.Success && ruleResult.Results.Any())
        {
            Console.WriteLine($"[MCP] ✓ Regla encontrada: {ruleResult.RuleDescription}");
            Console.WriteLine($"[MCP] ✓ Resultados encontrados: {ruleResult.Results.Count}");
            
            return new QueryResponse
            {
                Success = true,
                Results = ruleResult.Results,
                Source = "rule",
                Message = $"Búsqueda por regla: {ruleResult.RuleDescription}",
                Count = ruleResult.Results.Count
            };
        }

        Console.WriteLine("[MCP] No se encontró regla aplicable, usando LLM...");

        // PASO 2: Si no hay regla, usar LLMRouter
        try
        {
            var llmResult = await _llmRouter.GenerateLinqQuery(query);
            
            if (!llmResult.Success)
            {
                Console.WriteLine($"[MCP] ✗ Error en LLM: {llmResult.Error}");
                return new QueryResponse
                {
                    Success = false,
                    Message = $"Error al generar consulta: {llmResult.Error}"
                };
            }

            Console.WriteLine($"[MCP] ✓ LINQ generado: {llmResult.LinqQuery}");

            // Ejecutar el LINQ generado
            var results = _llmRouter.ExecuteLinq(llmResult.LinqQuery, allMovies).ToList();
            
            Console.WriteLine($"[MCP] ✓ Resultados encontrados: {results.Count}");

            return new QueryResponse
            {
                Success = true,
                Results = results,
                Source = "llm",
                Message = $"Búsqueda con IA. LINQ: {llmResult.LinqQuery}",
                Count = results.Count
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MCP] ✗ Excepción: {ex.Message}");
            return new QueryResponse
            {
                Success = false,
                Message = $"Error al procesar con IA: {ex.Message}"
            };
        }
    }
}