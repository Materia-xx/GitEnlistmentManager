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
                    Arguments = $@"-w gem nt --title ""{{RepoName}}"" --startingDirectory ""{{ReposDirectory}}"" ""%comspec%"" /k \""\""{vsDevCmd}\""&&CD /d \""{{EnlistmentDirectory}}\""\""",
                    OpenNewWindow = true,
                    WorkingDirectory = @"{ReposDirectory}"
                }
            );

            Documentation = "Opens Visual Studio 2026 Developer command prompt for the selected enlistment.";
        }
    }
}


