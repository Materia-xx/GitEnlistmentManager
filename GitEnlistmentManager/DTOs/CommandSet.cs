using GitEnlistmentManager.DTOs.Commands;
using GitEnlistmentManager.DTOs.CommandSetFilters;
using GitEnlistmentManager.Globals;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace GitEnlistmentManager.DTOs
{
    public class CommandSet
    {
        public CommandSetPlacement Placement { get; set; }
        
        public string? Verb { get; set; }

        public string? RightClickText { get; set; }

        [JsonIgnore]
        public string? CommandSetPath { get; set; }

        public List<ICommand> Commands { get; } = new();

        public List<ICommandSetFilter> Filters { get; set; } = new();

        public bool Matches(RepoCollection? repoCollection, Repo? repo, Bucket? bucket, Enlistment? enlistment)
        {
            return this.Filters.All(f => f.Matches(repoCollection, repo, bucket, enlistment));
        }

        public static bool WriteCommandSet(CommandSet commandSet)
        {
            if (string.IsNullOrWhiteSpace(commandSet?.CommandSetPath))
            {
                MessageBox.Show($"Command set path not set, unable to save command {commandSet?.Verb}");
                return false;
            }

            try
            {
                var commandDefinitionInfo = new FileInfo(commandSet.CommandSetPath);
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

        public static CommandSet? ReadCommandSet(string commandSetPath)
        {
            try
            {
                var commandSetJson = File.ReadAllText(commandSetPath);
                var commandSet = JsonConvert.DeserializeObject<CommandSet>(commandSetJson, GemJsonSerializer.Settings);
                if (commandSet == null)
                {
                    MessageBox.Show($"Unable to deserialize Command set from {commandSetPath}");
                    return null;
                }
                commandSet.CommandSetPath = commandSetPath;
                return commandSet;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading Command set: {ex.Message}");
            }
            return null;
        }
    }
}
