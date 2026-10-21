using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace OpenSoundPad;

public partial class App : Application
{
    public static void Log(string msg)
    {
        try
        {
            string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "OpenSoundPad");
            Directory.CreateDirectory(dir);
            string file = Path.Combine(dir, "debug.log");
            var fi = new FileInfo(file);
            if (fi.Exists && fi.Length > 100 * 1024)
            {
                File.Delete(file);
            }
            File.AppendAllText(file, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " " + msg + Environment.NewLine);
        }
        catch { }
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        Log("OnStartup started, args count: " + e.Args.Length);

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

        try
        {
            Log("Loading config...");
            var config = Audio.OspConfig.Load();
            Log("Config loaded. LanguageChosen=" + config.LanguageChosen + ", Lang=" + config.Language);

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
            Log("Creating MainWindow...");
            var main = new MainWindow(config);
            MainWindow = main;
            Log("Showing MainWindow...");
            main.Show();
            Log("MainWindow shown successfully.");
        }
        catch (Exception ex)
        {
            Log("Fatal exception during startup: " + ex);
            MessageBox.Show("Fatal startup error:\n" + ex, "OpenSoundPad", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Log("DispatcherUnhandledException: " + e.Exception);
        MessageBox.Show("Ошибка при работе приложения:\n" + e.Exception.Message, "OpenSoundPad Error", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Log("UnhandledException: " + e.ExceptionObject);
        if (e.ExceptionObject is Exception ex)
        {
            MessageBox.Show("Критическая ошибка:\n" + ex.Message, "OpenSoundPad Crash", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
