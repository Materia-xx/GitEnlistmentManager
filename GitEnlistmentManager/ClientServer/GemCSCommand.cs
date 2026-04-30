using GitEnlistmentManager.Globals;
using Newtonsoft.Json;

namespace GitEnlistmentManager.ClientServer
{
    public class GemCSCommand
    {
        public GemCSCommandType CommandType { get; set; }

        public object[]? CommandArgs { get; set; }

        public string? WorkingDirectory { get; set; }

        /// <summary>
        /// When true, the dispatcher will expand the GEM tree view to the resolved working
        /// directory (RepoCollection → Repo → TargetBranch → Bucket → Enlistment) both
        /// before and after the command set runs. Useful for MCP-driven invocations so the
        /// human watching the GUI can see exactly where the AI is operating, especially for
        /// commands like createbucket / createenlistment whose output appears as a new child
        /// of the working-directory node.
        ///
        /// The before-pass shows the current location immediately. The after-pass restores
        /// the expansion when the command set ends with a tree refresh (which rebuilds the
        /// data objects and would otherwise reset IsExpanded to false).
        /// </summary>
        public bool AutoExpandTreeView { get; set; }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this, GemJsonSerializer.Settings);
        }

        public static GemCSCommand? DeSerialize(string jsonString)
        {
            return JsonConvert.DeserializeObject<GemCSCommand>(jsonString, GemJsonSerializer.Settings);
        }
    }
}
