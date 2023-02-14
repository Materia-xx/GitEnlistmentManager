using System;

namespace GitEnlistmentManager.Globals
{
    public class Global
    {
        private static readonly Lazy<Global> global = new(() =>
        {
            var newGlobal = new Global();
            return newGlobal;
        });

        public static Global Instance => global.Value;

        private Global()
        {
            this.MainWindow = new MainWindow();
        }

        public MainWindow MainWindow { get; set; }
    }
}
