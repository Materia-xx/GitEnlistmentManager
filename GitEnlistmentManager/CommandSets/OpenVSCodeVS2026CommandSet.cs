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

            // Check a couple different places for the vsDevCmd.bat take the first one found. There is always the option to override the command set too.
            // Though we still write a non-existing cmd if it doesn't exist. It won't work, but there will be an example in the default command sets directory to work with.
            var vsSkus = new List<string>() { "Community", "Enterprise" };
            var vsDevCmd = @"C:\Program Files\Microsoft Visual Studio\18\Community\Common7\Tools\VsDevCmd.bat";
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
                    WorkingDirectory = @"{ReposDirectory}",
                    FireAndForget = true
                }
            );

            Documentation = "Launches a VS 2026 Developer command prompt scoped to the enlistment and opens VS Code inside that environment. Path must resolve to an enlistment. Side effect: spawns a console window and a VS Code instance for the user. Returns immediately after launching; MCP success means the launcher process started, not that VS Code finished loading.";
        }
    }
}
