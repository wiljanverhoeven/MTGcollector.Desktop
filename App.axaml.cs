using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MTGcollector_app.Data;
using MTGcollector_app.Services;
using MTGcollector_app.ViewModels;
using MTGcollector_app.Views;
using System;
using System.IO;


namespace MTGcollector_app;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();

        ConfigureServices(services);

        Services = services.BuildServiceProvider();

        using (var db = Services.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext())
        {
            db.Database.EnsureCreated();
        }

        if (ApplicationLifetime
            is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = Services
                .GetRequiredService<MainWindow>();

            var mainViewModel = Services
                .GetRequiredService<MainWindowViewModel>();

            mainWindow.DataContext = mainViewModel;

            desktop.MainWindow = mainWindow;

            // Initialize the view model asynchronously after showing the window
            _ = InitializeViewModelAsync(mainViewModel);
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static async System.Threading.Tasks.Task InitializeViewModelAsync(MainWindowViewModel viewModel)
    {
        try
        {
            await viewModel.InitializeAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing MainWindowViewModel: {ex}");
        }
    }

    private static void ConfigureServices(
        IServiceCollection services)
    {
        // -----------------------------
        // Database
        // -----------------------------

        var appData =
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData);

        var mtgFolder =
            Path.Combine(appData, "MTGcollector");

        Directory.CreateDirectory(mtgFolder);

        var databasePath =
            Path.Combine(mtgFolder, "mtgcollector.db");

        services.AddDbContextFactory<AppDbContext>(
            options =>
                options.UseSqlite(
                    $"Data Source={databasePath}"));

        // -----------------------------
        // HTTP / Scryfall
        // -----------------------------

        services.AddHttpClient<ScryfallService>();

        // -----------------------------
        // Application services
        // -----------------------------

        services.AddTransient<CollectionService>();

        // -----------------------------
        // ViewModels
        // -----------------------------

        services.AddTransient<MainWindowViewModel>();

        services.AddTransient<CollectionViewModel>();

        // -----------------------------
        // Views
        // -----------------------------

        services.AddTransient<MainWindow>();
        services.AddTransient<CollectionView>();
    }
}