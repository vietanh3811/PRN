using System.Windows;
using FlowerStore.Data;
using FlowerStore.Services;
using Microsoft.Extensions.Configuration;

namespace FlowerStore;

public partial class App : Application
{
    private IConfiguration? _configuration;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ShutdownMode = ShutdownMode.OnExplicitShutdown;
        _configuration = BuildConfiguration();

        var loginWindow = new LoginWindow();
        var loginResult = loginWindow.ShowDialog();

        if (loginResult != true)
        {
            Shutdown();
            return;
        }

        var connectionString = _configuration["ConnectionStrings:DefaultConnection"];
        var connectionFactory = new DbConnectionFactory(connectionString ?? string.Empty);
        var service = new FlowerStoreService(connectionFactory);
        var mainWindow = new MainWindow(service);
        MainWindow = mainWindow;
        ShutdownMode = ShutdownMode.OnMainWindowClose;
        mainWindow.Show();
    }

    private static IConfiguration BuildConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();
    }
}
