using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MathVerse.Desktop.ViewModels;
using MathVerse.Desktop.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MathVerse.Desktop;

public class App : Application
{
    private static IHost? _appHost;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        _appHost = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                services.AddSingleton<GraphViewModel>();
                services.AddSingleton<WorkspaceViewModel>();
            })
            .Build();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var workspace = _appHost.Services.GetRequiredService<WorkspaceViewModel>();
            desktop.MainWindow = new MainWindow { DataContext = workspace };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
