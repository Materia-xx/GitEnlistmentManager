using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Extensions;
using GitEnlistmentManager.Globals;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace GitEnlistmentManager.Commands
{
    public class OpenRootSolutionCommand : Command
    {
        public OpenRootSolutionCommand() 
        {
            this.Documentation = "Opens a root solution in an enlistment.";
            this.OpenNewWindow = true;
        }

        public override async Task<bool> Execute()
        {
            if (this.NodeContext.Enlistment == null || this.NodeContext.Repo == null)
            {
                return false;
            }

            var enlistmentDirectory = this.NodeContext.Enlistment?.GetDirectoryInfo();
            if (enlistmentDirectory == null)
            {
                return false;
            }

            var repoDirectory = this.NodeContext.Repo?.GetDirectoryInfo();
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
                workingDirectory: repoDirectory.FullName
                ).ConfigureAwait(false))
            {
                return false;
            }
            return true;
        }
    }
}
