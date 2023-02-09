using GitEnlistmentManager.DTOs.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitEnlistmentManager.DTOs.CommandSets
{
    internal class RefreshTreeviewCommandSet : CommandSet
    {
        public RefreshTreeviewCommandSet()
        {
            Placement = CommandSetPlacement.RepoCollection;
            OverrideKey = "refresh";
            RightClickText = "Refresh Treeview";
            Verb = String.Empty;
            Filename = "gemrefresh.cmdjson";

            Commands.Add(
                new RefreshTreeviewCommand()
            );
        }
    }
}
