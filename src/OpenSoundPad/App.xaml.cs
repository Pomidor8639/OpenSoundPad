using System;
using System.Windows;
using System.Windows.Threading;

namespace OpenSoundPad;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        if (e.Args.Length >= 2)
        {
            if (e.Args[0] == "--install-apo")
            {
                bool ok = Audio.OspApoManager.InstallApoOnEndpoint(e.Args[1]);
                Environment.Exit(ok ? 0 : 1);
                return;
            }
            if (e.Args[0] == "--uninstall-apo")
            {
                bool ok = Audio.OspApoManager.UninstallApoFromEndpoint(e.Args[1]);
                Environment.Exit(ok ? 0 : 1);
                return;
            }
        }

        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

        var config = Audio.OspConfig.Load();
        if (!config.LanguageChosen)
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;
            var welcome = new WelcomeLanguageWindow();
            welcome.ShowDialog();
            config.Language = welcome.SelectedLanguage;
            config.LanguageChosen = true;
            config.Save();
            ShutdownMode = ShutdownMode.OnMainWindowClose;
        }

        Loc.Lang = config.Language;
        var main = new MainWindow(config);
        MainWindow = main;
        main.Show();
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show("Ошибка при работе приложения:\n" + e.Exception.Message, "OpenSoundPad Error", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            MessageBox.Show("Критическая ошибка:\n" + ex.Message, "OpenSoundPad Crash", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
