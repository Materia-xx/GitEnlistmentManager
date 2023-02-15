using GitEnlistmentManager.Commands;
using System.Collections.Generic;
using System.IO;

namespace GitEnlistmentManager.CommandSets
{
    public class OpenDevVS2022CommandSet : CommandSet
    {
        public OpenDevVS2022CommandSet()
        {
            Placement = CommandSetPlacement.Enlistment;
            OverrideKey = "dev2022";
            RightClickText = "Open with 2022 Developer Prompt";
            Verb = "dev2022";
            Filename = "gemdev2022.cmdjson";

            // Check a couple different places for the vsDevCmd.bat take the first one found. There is always the option to override the command set too.
            // Though we still write a non-existing cmd if it doesn't exist. It won't work, but there will be an example in the default command sets directory to work with.
            var vsSkus = new List<string>() { "Community", "Enterprise" };
            var vsDevCmd = @"C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\Tools\VsDevCmd.bat";
            foreach (var vsSku in vsSkus)
            {
                var potentialVsDevCmd = @$"C:\Program Files\Microsoft Visual Studio\2022\{vsSku}\Common7\Tools\VsDevCmd.bat";
                if (File.Exists(potentialVsDevCmd))
                {
                    vsDevCmd = potentialVsDevCmd;
                }
            }

            Commands.Add(
                new RunProgramCommand()
                {
                    Program = "wt",
                                    // Setting the starting directory and then doing CD later into the right directory isn't necessary here. I'm just keeping it around as an example of how to do the escaping for multiple commands
                    Arguments = $@"-w gem nt --title ""{{RepoName}}"" --startingDirectory ""{{ReposDirectory}}"" ""%comspec%"" /k \""\""{vsDevCmd}\""&&CD /d \""{{EnlistmentDirectory}}\""\""",
                    OpenNewWindow = true,
                    // This actually starts with a working directory of the ReposDirectory and later on changes directory to the desired directory
                    // This directory ends up being locked by the terminal no matter what directory you CD to, which then would prevent enlistments from
                    // being archived correctly from the command prompt if we let it default to the enlistment directory
                    WorkingDirectory = @"{ReposDirectory}"
                }
            );

            Documentation = "Opens Visual Studio 2022 Developer command prompt for the selected enlistment.";
        }
    }
}
