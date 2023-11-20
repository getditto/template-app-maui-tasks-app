﻿using DittoMauiTasksApp.Utils;
using DittoMauiTasksApp.ViewModels;
using DittoSDK;
using Microsoft.Extensions.Logging;

namespace DittoMauiTasksApp;

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
        builder.Services.AddSingleton(SetupDitto());
        builder.Services.AddSingleton<IPopupService, PopupService>();
        builder.Services.AddTransient<MainPageViewModel>();
        builder.Services.AddTransient<MainPage>();

        return builder.Build();
    }

    private static Ditto SetupDitto()
    {
        var workingDir = $"{FileSystem.AppDataDirectory}/ditto";
        var ditto = new Ditto(DittoIdentity.OnlinePlayground("YOUR_APP_ID", "YOUR_TOKEN", true), workingDir);
        ditto.StartSync();

        return ditto;
    }
}

