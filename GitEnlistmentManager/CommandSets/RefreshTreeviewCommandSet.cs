using GitEnlistmentManager.Commands;
using System;

namespace GitEnlistmentManager.CommandSets
{
    internal class RefreshTreeviewCommandSet : CommandSet
    {
        public RefreshTreeviewCommandSet()
        {
            Placement = CommandSetPlacement.Gem;
            OverrideKey = "refresh";
            RightClickText = "Refresh";
            Verb = string.Empty;
            Filename = "gemrefresh.cmdjson";

            Commands.Add(new RefreshTreeviewCommand());

            Documentation = "Refreshes the Gem Treeview.";
        }
    }
}
