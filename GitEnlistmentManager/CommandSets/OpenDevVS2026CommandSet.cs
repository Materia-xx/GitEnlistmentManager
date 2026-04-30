using GitEnlistmentManager.Commands;
using System.Collections.Generic;
using System.IO;

namespace GitEnlistmentManager.CommandSets
{
    public class OpenDevVS2026CommandSet : CommandSet
    {
        public OpenDevVS2026CommandSet()
        {
            Placement = CommandSetPlacement.Enlistment;
            OverrideKey = "dev2026";
            RightClickText = "VS 2026 CMD";
            Verb = "dev2026";
            Filename = "gemdev2026.cmdjson";

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
                    Arguments = $@"-w gem nt --title ""{{RepoName}}"" --startingDirectory ""{{ReposDirectory}}"" ""%comspec%"" /k \""\""{vsDevCmd}\""&&CD /d \""{{EnlistmentDirectory}}\""\""",
                    OpenNewWindow = true,
                    WorkingDirectory = @"{ReposDirectory}",
                    FireAndForget = true
                }
            );

            Documentation = "Launches an interactive Visual Studio 2026 Developer command prompt window scoped to the enlistment directory. Path must resolve to an enlistment. Side effect: opens a new console window for the user. The console is interactive — output is NOT captured back through MCP. Returns immediately after launching the console; MCP success only indicates that the launcher process started. Use only when the user wants a dev prompt; not useful for programmatic operations.";
        }
    }
}


