using GitEnlistmentManager.ClientServer;
using System;
using System.IO;
using System.Threading;
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

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            this.ShutdownMode = ShutdownMode.OnLastWindowClose;

            bool serverAlreadyRunning = !mutex.WaitOne(TimeSpan.Zero, true);
            if (!serverAlreadyRunning)
            {
                var mainWindow = new MainWindow();
                mainWindow.Show();
                this.gemServer = new GemServer(mainWindow.ProcessCSCommand);
                this.gemServer.Start();
            }

            if (e.Args.Length > 0)
            {
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

        protected override void OnExit(ExitEventArgs e)
        {
            this.gemServer?.Stop();
        }
    }
}
