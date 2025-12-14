using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MovieManager.Core.Models;
using MovieManager.MAUI.Models;
using MovieManager.MAUI.Services;

namespace MovieManager.MAUI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly McpApiClient _apiClient;

    [ObservableProperty]
    private string _queryText = "";

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = "";

    [ObservableProperty]
    private MauiQueryResponse? _queryResponse;

    public ObservableCollection<Movie> Movies { get; } = new();

    public MainViewModel(McpApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [RelayCommand]
    private async Task LoadTestData()
    {
        await LoadData(async () => await _apiClient.LoadTestDataAsync());
    }

    [RelayCommand]
    private async Task LoadFullDataset()
    {
        await LoadData(async () => await _apiClient.LoadFullDatasetAsync());
    }

    [RelayCommand]
    private async Task ExecuteQuery()
    {
        if (string.IsNullOrWhiteSpace(QueryText) || IsLoading)
            return;

        IsLoading = true;
        StatusMessage = "Procesando consulta...";
        Movies.Clear();

        var response = await _apiClient.QueryAsync(QueryText);
        if (response != null)
        {
            QueryResponse = response;
            if (response.Success && response.Results != null)
            {
                foreach (var movie in response.Results)
                {
                    Movies.Add(movie);
                }
                StatusMessage = $"Encontrados {response.Count} resultados.";
            }
            else
            {
                StatusMessage = response.Message;
            }
        }
        else
        {
            StatusMessage = "Error: No se pudo obtener respuesta del servidor.";
        }

        IsLoading = false;
    }

    private async Task LoadData(Func<Task<(bool Success, string Message)>> loadAction)
    {
        if (IsLoading) return;
        IsLoading = true;
        StatusMessage = "Cargando datos...";

        var (success, message) = await loadAction();
        StatusMessage = message;

        IsLoading = false;
    }
}
