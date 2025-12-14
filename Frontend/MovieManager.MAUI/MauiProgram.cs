using Microsoft.Extensions.Logging;
using MovieManager.MAUI.Services;
using MovieManager.MAUI.ViewModels;
using MovieManager.MAUI.Views;
using MovieManager.MAUI.Converters;

namespace MovieManager.MAUI;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

        // Inyección de dependencias
        builder.Services.AddSingleton<McpApiClient>();
        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<MainPage>();

        // Registrar el conversor como un recurso estático
        builder.Services.AddSingleton<InverseBoolConverter>();
        
		return builder.Build();
	}
}
