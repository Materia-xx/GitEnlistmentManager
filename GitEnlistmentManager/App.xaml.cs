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

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            this.ShutdownMode = ShutdownMode.OnLastWindowClose;

            if (!mutex.WaitOne(TimeSpan.Zero, true))
            {
                var cmd = new GemCSCommand()
                {
                    CommandType = GemCSCommandType.InterpretCommandLine,
                    CommandArgs = e.Args,
                    WorkingDirectory = Directory.GetCurrentDirectory()
                };
                GemClient.Instance.SendCommand(cmd);
                this.Shutdown();
            }
        }
    }
}
