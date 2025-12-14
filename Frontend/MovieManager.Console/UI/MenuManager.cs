using MovieManager.Console.Services;

namespace MovieManager.Console.UI;

public class MenuManager
{
    private readonly ApiClient _apiClient;

    public MenuManager(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task RunAsync()
    {
        ShowBanner();
        
        // Check server health
        System.Console.Write("🔍 Verificando servidor MCP... ");
        var isHealthy = await _apiClient.CheckHealthAsync();
        
        if (!isHealthy)
        {
            System.Console.WriteLine("✗");
            System.Console.WriteLine("\n⚠️  ERROR: El servidor MCP no está disponible.");
            System.Console.WriteLine("   Asegúrate de que está corriendo en http://localhost:5001");
            System.Console.WriteLine("\n   Ejecuta: cd Backend/MovieManager.MCP && dotnet run");
            return;
        }
        
        System.Console.WriteLine("✓\n");

        while (true)
        {
            ShowMenu();
            var option = System.Console.ReadLine()?.Trim();

            switch (option)
            {
                case "1":
                    await LoadTestDataAsync();
                    break;
                case "2":
                    await LoadFullDatasetAsync();
                    break;
                case "3":
                    await QueryMoviesAsync();
                    break;
                case "4":
                    ShowExamples();
                    break;
                case "5":
                    System.Console.WriteLine("\n👋 ¡Hasta pronto!");
                    return;
                default:
                    System.Console.WriteLine("\n⚠️  Opción no válida. Intenta de nuevo.\n");
                    break;
            }

            System.Console.WriteLine("\nPresiona ENTER para continuar...");
            System.Console.ReadLine();
        }
    }

    private void ShowBanner()
    {
        System.Console.Clear();
        System.Console.ForegroundColor = ConsoleColor.Cyan;
        System.Console.WriteLine(@"
╔══════════════════════════════════════════════════════════╗
║                                                          ║
║          🎬 MovieManager Console Client                 ║
║          Consultas en Lenguaje Natural con MCP           ║
║                                                          ║
╚══════════════════════════════════════════════════════════╝
        ");
        System.Console.ResetColor();
    }

    private void ShowMenu()
    {
        System.Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        System.Console.WriteLine("  MENÚ PRINCIPAL");
        System.Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        System.Console.WriteLine("  1. 📥 Cargar datos de prueba (5 películas)");
        System.Console.WriteLine("  2. 📦 Cargar dataset completo (1000 películas)");
        System.Console.WriteLine("  3. 🔍 Consultar películas (lenguaje natural)");
        System.Console.WriteLine("  4. 💡 Ver ejemplos de consultas");
        System.Console.WriteLine("  5. 🚪 Salir");
        System.Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        System.Console.Write("\n👉 Selecciona una opción: ");
    }

    private async Task LoadTestDataAsync()
    {
        System.Console.WriteLine("\n📥 Cargando datos de prueba...");
        var result = await _apiClient.LoadTestDataAsync();
        System.Console.WriteLine($"   {result}");
    }

    private async Task LoadFullDatasetAsync()
    {
        System.Console.WriteLine("\n📦 Cargando dataset completo...");
        var result = await _apiClient.LoadFullDatasetAsync();
        System.Console.WriteLine($"   {result}");
    }

    private async Task QueryMoviesAsync()
    {
        System.Console.WriteLine("\n🔍 CONSULTA EN LENGUAJE NATURAL");
        System.Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        System.Console.Write("\n💬 Escribe tu consulta: ");
        var query = System.Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(query))
        {
            System.Console.WriteLine("⚠️  Consulta vacía.");
            return;
        }

        System.Console.WriteLine($"\n⏳ Procesando: \"{query}\"...\n");

        var response = await _apiClient.QueryAsync(query);

        if (response == null)
        {
            System.Console.WriteLine("✗ Error al obtener respuesta del servidor.");
            return;
        }

        if (!response.Success)
        {
            System.Console.WriteLine($"✗ Error: {response.Message}");
            return;
        }

        // Mostrar información de la consulta
        System.Console.ForegroundColor = ConsoleColor.Yellow;
        System.Console.WriteLine($"📊 Fuente: {(response.Source == "rule" ? "🔧 Reglas" : "🤖 IA (LLM)")}");
        System.Console.WriteLine($"📈 Resultados: {response.Count}");
        
        if (response.Source == "llm")
        {
            System.Console.ForegroundColor = ConsoleColor.DarkGray;
            System.Console.WriteLine($"🧠 LINQ: {response.Message.Replace("Búsqueda con IA. LINQ: ", "")}");
        }
        System.Console.ResetColor();

        if (response.Count == 0)
        {
            System.Console.WriteLine("\n❌ No se encontraron películas con esos criterios.");
            return;
        }

        System.Console.WriteLine("\n🎬 PELÍCULAS ENCONTRADAS:");
        System.Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");

        foreach (var movie in response.Results)
        {
            System.Console.ForegroundColor = ConsoleColor.Cyan;
            System.Console.WriteLine($"🎥 {movie.Title} ({movie.Year})");
            System.Console.ResetColor();
            
            System.Console.WriteLine($"   📂 Género: {movie.Genre}");
            System.Console.WriteLine($"   ⭐ Rating: {movie.Rating}/10");
            System.Console.WriteLine($"   🎬 Director: {movie.Director}");
            System.Console.WriteLine($"   ⏱️  Duración: {movie.Runtime} min");
            
            if (!string.IsNullOrEmpty(movie.Overview))
            {
                System.Console.ForegroundColor = ConsoleColor.DarkGray;
                System.Console.WriteLine($"   📝 {movie.Overview}");
                System.Console.ResetColor();
            }
            
            System.Console.WriteLine();
        }
    }

    private void ShowExamples()
    {
        System.Console.WriteLine("\n💡 EJEMPLOS DE CONSULTAS");
        System.Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");

        var examples = new[]
        {
            ("🔧 Reglas", new[]
            {
                "películas de 2010",
                "películas de Nolan",
                "películas de acción",
                "películas de 2008 a 2014",
                "películas dirigidas por Nolan después de 2010"
            }),
            ("🤖 IA (LLM)", new[]
            {
                "películas de ciencia ficción con más de 8.5 de rating",
                "películas dramáticas de menos de 2 horas",
                "películas de Tarantino con más de 8 de rating",
                "películas de acción cortas"
            })
        };

        foreach (var (category, queries) in examples)
        {
            System.Console.ForegroundColor = ConsoleColor.Yellow;
            System.Console.WriteLine($"{category}:");
            System.Console.ResetColor();
            
            foreach (var query in queries)
            {
                System.Console.WriteLine($"  • {query}");
            }
            System.Console.WriteLine();
        }
    }
}