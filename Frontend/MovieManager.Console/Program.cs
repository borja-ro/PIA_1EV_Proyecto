using MovieManager.Console.Services;
using MovieManager.Console.UI;

namespace MovieManager.Console;

class Program
{
    static async Task Main(string[] args)
    {
        var apiClient = new ApiClient("http://localhost:5001");
        var menuManager = new MenuManager(apiClient);
        
        await menuManager.RunAsync();
    }
}