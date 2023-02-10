using GitEnlistmentManager.DTOs.Commands;
using System;

namespace GitEnlistmentManager.DTOs.CommandSets
{
    internal class RefreshTreeviewCommandSet : CommandSet
    {
        public RefreshTreeviewCommandSet()
        {
            Placement = CommandSetPlacement.Gem;
            OverrideKey = "refresh";
            RightClickText = "Refresh";
            Verb = String.Empty;
            Filename = "gemrefresh.cmdjson";

            Commands.Add(new RefreshTreeviewCommand());
        }
    }
}
