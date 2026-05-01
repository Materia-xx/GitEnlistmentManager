using GitEnlistmentManager.Commands;

namespace GitEnlistmentManager.CommandSets
{
    public class OpenRootSolutionCommandSet : CommandSet
    {
        public OpenRootSolutionCommandSet()
        {
            Placement = CommandSetPlacement.Enlistment;
            OverrideKey = "openrootsolution";
            RightClickText = "Open Root Solution";
            Verb = "openrootsolution";
            Filename = "gemopenrootsolution.cmdjson";

            Commands.Add(new OpenRootSolutionCommand());

            Documentation = "Opens the .sln file at the root of the enlistment in Visual Studio 2022. Path must resolve to an enlistment. Errors if no .sln is found at the enlistment root or if VS 2022 Community/Enterprise is not installed. Side effect: launches Visual Studio 2022. Returns immediately after launching; MCP success means devenv.exe started, not that the solution finished loading or that the user has done anything in VS.";
        }
    }
}
