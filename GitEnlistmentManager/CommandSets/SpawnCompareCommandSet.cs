using GitEnlistmentManager.Commands;

namespace GitEnlistmentManager.CommandSets
{
    public class SpawnCompareCommandSet : CommandSet
    {
        public SpawnCompareCommandSet()
        {
            // Anchor at RepoCollection level so an MCP caller can pass any RepoCollection
            // directory as the dispatch path. The two compared directories are supplied as
            // args and do NOT need to be under the GEM tree.
            Placement = CommandSetPlacement.RepoCollection;
            OverrideKey = "spawncompare";
            // No right-click text — this command takes two path args which the UI cannot
            // supply. Invoke from MCP or the command line only.
            RightClickText = string.Empty;
            Verb = "spawncompare";
            Filename = "gemspawncompare.cmdjson";

            Commands.Add(new SpawnCompareCommand());

            Documentation = "Launches the user's configured diff tool comparing two arbitrary directories. Args: <leftPath> <rightPath> — both should be absolute paths (e.g. `spawncompare C:\\\\repos\\\\left C:\\\\repos\\\\right`). The dispatch path argument is just a context anchor; pass any RepoCollection directory (use list_tree to find one). The two compared paths do NOT need to be enlistments or even inside the GEM tree, but they must exist. Returns immediately after launching; MCP success only indicates the diff tool started.";
        }
    }
}
