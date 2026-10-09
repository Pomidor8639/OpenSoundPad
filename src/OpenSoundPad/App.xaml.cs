using System;
using System.Windows;
using System.Windows.Threading;

namespace OpenSoundPad;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
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

