using GitEnlistmentManager.Commands;
using System.Collections.Generic;
using System.IO;

namespace GitEnlistmentManager.CommandSets
{
    public class OpenVSCodeVS2026CommandSet : CommandSet
    {
        public OpenVSCodeVS2026CommandSet()
        {
            Placement = CommandSetPlacement.Enlistment;
            OverrideKey = "vs2026vscode";
            RightClickText = "VS 2026 CMD -> VS Code";
            Verb = "vs2026vscode";
            Filename = "gemvs2026vscode.cmdjson";

            var vsSkus = new List<string>() { "Community", "Enterprise" };
            var vsDevCmd = string.Empty;
            foreach (var vsSku in vsSkus)
            {
                var potentialVsDevCmd = @$"C:\Program Files\Microsoft Visual Studio\18\{vsSku}\Common7\Tools\VsDevCmd.bat";
                if (File.Exists(potentialVsDevCmd))
                {
                    vsDevCmd = potentialVsDevCmd;
                }
            }

            Commands.Add(
                new RunProgramCommand()
                {
                    Program = "wt",
                    Arguments = $@"-w gem nt --title ""{{RepoName}}"" --startingDirectory ""{{ReposDirectory}}"" ""%comspec%"" /c \""\""{vsDevCmd}\""&&CD /d \""{{EnlistmentDirectory}}\""\""&&code .",
                    OpenNewWindow = true,
                    WorkingDirectory = @"{ReposDirectory}"
                }
            );

            Documentation = "Opens VSCode under a Visual Studio 2026 Developer command prompt for the selected enlistment.";
        }
    }
}
