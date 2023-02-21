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

        public string? Documentation { get; set; }

        [JsonIgnore]
        public string? Filename { get; set; }

        [JsonIgnore]
        public string? LoadedFromPath { get; set; }

        public List<Command> Commands { get; } = new();

        public List<ICommandSetFilter> Filters { get; set; } = new();

        public bool Matches(RepoCollection? repoCollection, Repo? repo, Bucket? bucket, Enlistment? enlistment)
        {
            return Filters.All(f => f.Matches(repoCollection, repo, bucket, enlistment));
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
                commandSet.LoadedFromPath = commandSetPath;
                return (commandSet, string.Empty);
            }
            catch (Exception ex)
            {
                return (null, $"Deserializing {commandSetPath} produced exception: {ex.Message}");
            }
        }
    }
}
