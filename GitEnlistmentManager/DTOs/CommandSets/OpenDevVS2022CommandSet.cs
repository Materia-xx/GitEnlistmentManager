using GitEnlistmentManager.DTOs.Commands;

namespace GitEnlistmentManager.DTOs.CommandSets
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

            Commands.Add(
                new RunProgramCommand()
                {
                    Program = "wt", // TODO: if wt isn't installed this will error out // TODO: also only works for community. What about other SKUs?
                                    // Setting the starting directory and then doing CD later into the right directory isn't necessary here. I'm just keeping it around as an example of how to do the escaping for multiple commands
                    Arguments = @"-w gem nt --title ""{RepoName}"" --startingDirectory ""{ReposFolder}"" ""%comspec%"" /k \""\""C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\Tools\VsDevCmd.bat\""&&CD /d \""{EnlistmentDirectory}\""\""",
                    OpenNewWindow = true,
                    // This actually starts with a working folder of the ReposFolder and later on changes directory to the desired directory
                    // This directory ends up being locked by the terminal no matter what directory you CD to, which then would prevent enlistments from
                    // being archived correctly from the command prompt if we let it default to the enlistment directory
                    WorkingFolder = @"{ReposFolder}"
                }
            );
        }
    }
}
