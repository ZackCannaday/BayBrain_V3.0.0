using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace BayBrain
{
    public partial class App : Application
    {
        private static string _lastDispatcherError = string.Empty;
        private static DateTime _lastDispatcherErrorShownAt = DateTime.MinValue;

        protected override void OnStartup(StartupEventArgs e)
        {
            // Register global exception handlers
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            base.OnStartup(e);
        }

        private void App_DispatcherUnhandledException(object? sender, DispatcherUnhandledExceptionEventArgs e)
        {
            LogException("DispatcherUnhandledException", e.Exception);
            var signature = $"{e.Exception.GetType().FullName}:{e.Exception.Message}";
            var shouldShow = signature != _lastDispatcherError ||
                             DateTime.Now - _lastDispatcherErrorShownAt > TimeSpan.FromSeconds(10);

            if (shouldShow)
            {
                _lastDispatcherError = signature;
                _lastDispatcherErrorShownAt = DateTime.Now;
                MessageBox.Show("An unexpected error occurred. See crash.log for details.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object? sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                LogException("CurrentDomain_UnhandledException", ex);
            }
            else
            {
                LogText("CurrentDomain_UnhandledException: non-Exception object thrown\n");
            }
        }

        private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            LogException("UnobservedTaskException", e.Exception);
            e.SetObserved();
        }

        private static void LogException(string source, Exception ex)
        {
            try
            {
                var text = $"[{DateTime.Now:O}] {source}: {ex.GetType().FullName} - {ex.Message}\n{ex.StackTrace}\n";
                LogText(text);
            }
            catch
            {
                // swallow
            }
        }

        private static void LogText(string text)
        {
            try
            {
                var dir = AppDomain.CurrentDomain.BaseDirectory ?? Environment.CurrentDirectory;
                var path = Path.Combine(dir, "crash.log");
                File.AppendAllText(path, text + "\n");
            }
            catch
            {
                // ignore logging failures
            }
        }
    }
}
