using GitEnlistmentManager.CommandSets;
using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Globals;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GitEnlistmentManager.Extensions
{
    public static class CommandSetExtensions
    {
        public static bool WriteCommandSet(this CommandSet commandSet, string commandSetDirectory, bool overwrite)
        {
            if (string.IsNullOrWhiteSpace(commandSet?.Filename))
            {
                MessageBox.Show($"Command set filename not set, unable to save command {commandSet?.Verb}");
                return false;
            }
            if (string.IsNullOrWhiteSpace(commandSetDirectory))
            {
                MessageBox.Show($"Command set directory not set, unable to save command {commandSet?.Verb}");
                return false;
            }
            var commandSetPath = Path.Combine(commandSetDirectory, commandSet.Filename);
            if (File.Exists(commandSetPath) && !overwrite)
            {
                return true;
            }

            try
            {
                var commandDefinitionInfo = new FileInfo(commandSetPath);
                var commandJson = JsonConvert.SerializeObject(commandSet, GemJsonSerializer.Settings);
                File.WriteAllText(commandDefinitionInfo.FullName, commandJson);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error writing Command set: {ex.Message}");
                return false;
            }
            return true;
        }

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
            foreach (var command in commandSet.Commands)
            {
                // Commands are intended to run only 1 time because they are stateful.
                if (command.Executed)
                {
                    MessageBox.Show("Commands in a command set can only be run 1 time. This most likely represents a coding error in the program that needs to be fixed.");
                    return false;
                }
                command.MarkAsExecuted();
                command.NodeContext.SetIfNotNullFrom(nodeContext);

                // Execute the command and if the command was not successful then end now, returning false for the command set
                if (!await command.Execute().ConfigureAwait(false))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
