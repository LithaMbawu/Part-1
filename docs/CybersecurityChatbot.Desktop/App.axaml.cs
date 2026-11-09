using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace CybersecurityChatbot.Desktop;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            desktop.MainWindow.Closing += (_, args) =>
            {
                if (args.CloseReason == WindowCloseReason.WindowClosing && !args.IsProgrammatic)
                {
                    args.Cancel = true;
                }
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
