using GitEnlistmentManager.Commands;
using GitEnlistmentManager.CommandSetFilters;
using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Globals;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace GitEnlistmentManager.CommandSets
{
    public class CommandSet
    {
        public CommandSetPlacement Placement { get; set; }

        public string? Verb { get; set; }

        public string? OverrideKey { get; set; }

        public string? RightClickText { get; set; }

        public string? CommandSetDocumentation { get; set; }

        [JsonIgnore]
        public string? Filename { get; set; }

        public List<ICommand> Commands { get; } = new();

        public List<ICommandSetFilter> Filters { get; set; } = new();

        public bool Matches(RepoCollection? repoCollection, Repo? repo, Bucket? bucket, Enlistment? enlistment)
        {
            return Filters.All(f => f.Matches(repoCollection, repo, bucket, enlistment));
        }

        public static bool WriteCommandSet(CommandSet commandSet, string commandSetDirectory, bool overwrite)
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

        public static (CommandSet? CommandSet, string LoadingError) ReadCommandSet(string commandSetPath)
        {
            try
            {
                var commandSetJson = File.ReadAllText(commandSetPath);
                var commandSet = JsonConvert.DeserializeObject<CommandSet>(commandSetJson, GemJsonSerializer.Settings);
                if (commandSet == null)
                {
                    return (null, $"Deserializing {commandSetPath} returned null");
                }
                commandSet.Filename = Path.GetFileName(commandSetPath);
                return (commandSet, string.Empty);
            }
            catch (Exception ex)
            {
                return (null, $"Deserializing {commandSetPath} produced exception: {ex.Message}");
            }
        }
    }
}
