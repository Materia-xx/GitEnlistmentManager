using GitEnlistmentManager.CommandSets;
using GitEnlistmentManager.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public static async Task<bool> RunCommandSets(this List<CommandSet> commandSets, GemNodeContext nodeContext)
        {
            foreach (var commandSet in commandSets)
            {
                if (!await commandSet.RunCommandSet(
                    nodeContext: nodeContext
                    ).ConfigureAwait(false))
                {
                    return false;
                }
            }
            return true;
        }

        public static async Task<bool> RunCommandSet(this CommandSet commandSet, GemNodeContext nodeContext)
        {
            try
            {
                foreach (var command in commandSet.Commands)
                {
                    command.NodeContext.BaseNodeContext.SetFrom(nodeContext);

                    // Execute the command and if the command was not successful then end now, returning false for the command set
                    if (!await command.Execute().ConfigureAwait(false))
                    {
                        return false;
                    }
                }
                return true;
            }
            finally
            {
                // Command sets will inherit node context while they are running, clear that now
                foreach (var command in commandSet.Commands)
                {
                    command.NodeContext.BaseNodeContext.Clear();
                }
            }
        }
    }
}
