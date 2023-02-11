using GitEnlistmentManager.ClientServer;
using GitEnlistmentManager.DTOs;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace GitEnlistmentManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // The mutex helps us make sure that only 1 copy of the app is running.
        static readonly Mutex mutex = new(true, "{DF828E54-BCCA-485B-8B49-633F7A9B794A}");
        private GemServer? gemServer;
        private bool mainWindowFullyLoaded = true;

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            this.ShutdownMode = ShutdownMode.OnLastWindowClose;

            bool serverAlreadyRunning = !mutex.WaitOne(TimeSpan.Zero, true);
            if (!serverAlreadyRunning)
            {
                mainWindowFullyLoaded = false;
                var mainWindow = new MainWindow();

                // Fully loaded will wait for the main form to refresh the treeview
                // If we didn't there is a chance that gem will be refreshing the data and any command sent won't be handled correctly
                mainWindow.FullyLoaded += MainWindow_FullyLoaded;
                mainWindow.Show();
                this.gemServer = new GemServer(mainWindow.ProcessCSCommand, Gem.Instance.LocalAppData.ServerPort);
                this.gemServer.Start();
            }

            if (e.Args.Length > 0)
            {
                // Wait for the main window to fully load before sending the command
                while (!this.mainWindowFullyLoaded)
                {
                    await Task.Delay(300).ConfigureAwait(false);
                }
                var cmd = new GemCSCommand()
                {
                    CommandType = GemCSCommandType.InterpretCommandLine,
                    CommandArgs = e.Args,
                    WorkingDirectory = Directory.GetCurrentDirectory()
                };
                GemClient.Instance.SendCommand(cmd);
            }

            // If the server was already running, then close this instance right after sending any command
            if (serverAlreadyRunning)
            {
                this.Shutdown();
            }
        }

        private void MainWindow_FullyLoaded(object? sender, EventArgs e)
        {
            mainWindowFullyLoaded = true;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            this.gemServer?.Stop();
        }
    }
}
