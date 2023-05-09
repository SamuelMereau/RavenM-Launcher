using System.Diagnostics;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using RavenM_Launcher.ViewModels;
using RavenM_Launcher.Views;

namespace RavenM_Launcher
{
    public partial class App : Application
    {
        public static bool steamInitialised = false;
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);

            try
            {
                Steamworks.SteamClient.Init(636480, true);
                steamInitialised = true;
            }
            catch (System.Exception e)
            {
                // Something went wrong! Steam is closed?
                var messageBox =
                    MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow("Error",
                        $"Error when connecting to Steam: {e.Message}");
                messageBox.Show();
            }
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}