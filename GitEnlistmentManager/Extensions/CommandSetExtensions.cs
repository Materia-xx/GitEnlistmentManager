using GitEnlistmentManager.CommandSets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace GitEnlistmentManager.Extensions
{
    public static class CommandSetExtensions
    {
        public static List<CommandSet> RemoveTheOverriddenDefaultCommandSets(this List<CommandSet> allCommandSets)
        {
            var commandSets = new List<CommandSet>();
            // Process in reverse so it's easier to add overridden command sets. They are overridden by the override key.
            allCommandSets.Reverse();
            foreach (var acs in allCommandSets)
            {
                // A command set that doesn't have a override key isn't valid
                if (acs.OverrideKey == null)
                {
                    MessageBox.Show($"Command set {acs.Filename} is missing an override key and is being skipped.");
                    continue;
                }
                if (!commandSets.Any(cs => cs.OverrideKey != null && cs.OverrideKey.Equals(acs.OverrideKey, StringComparison.OrdinalIgnoreCase)))
                {
                    commandSets.Add(acs);
                }
            }
            commandSets.Reverse();
            return commandSets;
        }
    }
}
