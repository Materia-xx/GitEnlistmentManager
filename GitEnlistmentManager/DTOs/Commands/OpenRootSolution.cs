using GitEnlistmentManager.Extensions;
using GitEnlistmentManager.Globals;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace GitEnlistmentManager.DTOs.Commands
{
    public class OpenRootSolution : ICommand
    {
        public bool OpenNewWindow { get; set; } = true;

        public void ParseArgs(GemNodeContext nodeContext, Stack<string> arguments)
        {
        }

        public async Task<bool> Execute(GemNodeContext nodeContext, MainWindow mainWindow)
        {
            if (nodeContext.Enlistment == null || nodeContext.Repo == null)
            {
                return false;
            }

            var enlistmentDirectory = nodeContext.Enlistment?.GetDirectoryInfo();
            if (enlistmentDirectory == null)
            {
                return false;
            }

            var repoDirectory = nodeContext.Repo?.GetDirectoryInfo();
            if (repoDirectory == null)
            {
                return false;
            }

            // Look for Visual Studio
            var vsSkus = new List<string>() { "Community", "Enterprise" };
            string? devenvExe = null;
            foreach (var vsSku in vsSkus)
            {
                var potentialDevenvExe = @$"C:\Program Files\Microsoft Visual Studio\2022\{vsSku}\Common7\IDE\devenv.exe";
                if (File.Exists(potentialDevenvExe))
                {
                    devenvExe = potentialDevenvExe;
                }
            }
            if (string.IsNullOrWhiteSpace(devenvExe))
            {
                MessageBox.Show("Unable to find VS 2022 Community or Enterprise installed");
                return false;
            }

            // Look for sln files in the root of the enlistment
            var slnFiles = Directory.GetFiles(enlistmentDirectory.FullName, "*.sln");
            if (slnFiles.Length == 0)
            {
                MessageBox.Show("No sln files were found in the root of the enlistment");
                return false;
            }

            // Open the solution
            if (!await ProgramHelper.RunProgram(
                programPath: devenvExe,
                arguments: slnFiles[0],
                tokens: null, // There are no tokens in the above programPath/arguments
                useShellExecute: false,
                openNewWindow: true,
                workingFolder: repoDirectory.FullName
                ).ConfigureAwait(false))
            {
                return false;
            }
            return true;
        }
    }
}
