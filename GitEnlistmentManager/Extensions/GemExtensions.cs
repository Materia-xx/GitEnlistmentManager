using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.DTOs.Commands;
using GitEnlistmentManager.DTOs.CommandSetFilters;
using GitEnlistmentManager.Globals;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace GitEnlistmentManager.Extensions
{
    public static class GemExtensions
    {
        private const string gemLocalAppDataFilename = "GemSettings.json";

        public static bool ReloadSettings(this Gem gem)
        {
            // If the Gem settings are not yet set, these need to be set first before
            // doing anything else
            while (!gem.ReadLocalAppData() || string.IsNullOrWhiteSpace(gem.LocalAppData.ReposFolder))
            {
                var gemSettingsEditor = new GemSettings(gem);
                var result = gemSettingsEditor.ShowDialog();
                if (result != null && !result.Value)
                {
                    return false;
                }
            }

            // For each Gem repo collection folder we have, look through it for repo definitions
            gem.RepoCollections.Clear();
            foreach (var repoCollectionDefinitionFolder in gem.LocalAppData.RepoCollectionDefinitionFolders)
            {
                var repoCollectionDefinitionInfo = new DirectoryInfo(repoCollectionDefinitionFolder);
                if (!repoCollectionDefinitionInfo.Exists)
                {
                    MessageBox.Show($"Repo collection definition folder {repoCollectionDefinitionInfo.FullName} was not found");
                    continue;
                }

                var repoCollection = new RepoCollection(gem, repoCollectionDefinitionFolder)
                {
                    Name = repoCollectionDefinitionInfo.Name
                };
                gem.RepoCollections.Add(repoCollection);

                // Loop through all the repo registrations that are present here
                var repoRegistrations = repoCollectionDefinitionInfo.GetFiles("*.repojson", SearchOption.TopDirectoryOnly);
                foreach (var repoRegistration in repoRegistrations)
                {
                    var repo = new Repo(repoCollection)
                    {
                        Name = Path.GetFileNameWithoutExtension(repoRegistration.Name)
                    };

                    // Best attempt to load repo metadata, but if it fails then still add it to the UI so the user can re-create it.
                    repo.ReadMetadata(repoRegistration.FullName);
                    repoCollection.Repos.Add(repo);
                }
            }

            // Scan the actual repos/buckets/enlistments to discover the current state
            foreach (var repoCollection in gem.RepoCollections)
            {
                foreach (var repo in repoCollection.Repos)
                {
                    var repoFolder = repo.GetDirectoryInfo();
                    if (repoFolder == null)
                    {
                        continue;
                    }

                    foreach (var bucketFolder in repoFolder.GetDirectories())
                    {
                        // Don't show the archive folder in the UI
                        if (bucketFolder.Name.Equals("archive", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        var bucket = new Bucket(repo)
                        {
                            Name = bucketFolder.Name
                        };
                        repo.Buckets.Add(bucket);

                        foreach (var enlistmentFolder in bucketFolder.GetDirectories())
                        {
                            var enlistment = new Enlistment(bucket)
                            {
                                Name = enlistmentFolder.Name
                            };
                            bucket.Enlistments.Add(enlistment);
                        }
                    }
                }
            }

            WriteDefaultCommandSets(gem);

            // Look for commands in the command set folders
            gem.CommandSets.Clear();

            // Inject the default command set folder at the beginning, but it's not exposed in the UI
            var commandSetFolders = new List<string>();
            commandSetFolders.Add(gem.GetDefaultCommandSetsFolder().FullName);
            commandSetFolders.AddRange(gem.LocalAppData.CommandSetFolders);

            foreach (var commandSetFolder in commandSetFolders)
            {
                var commandSetFolderInfo = new DirectoryInfo(commandSetFolder);
                if (!commandSetFolderInfo.Exists)
                {
                    MessageBox.Show($"Command set folder {commandSetFolderInfo.FullName} was not found");
                    continue;
                }

                // Loop through all the command set definitions that are present here
                var commandDefinitions = commandSetFolderInfo.GetFiles("*.cmdjson", SearchOption.TopDirectoryOnly);
                foreach (var commandDefinition in commandDefinitions)
                {
                    var commandSet = CommandSet.ReadCommandSet(commandDefinition.FullName);
                    if (commandSet == null)
                    {
                        continue;
                    }

                    // We always load all command sets. It's when we run them that we decide how to pick which one overrides the others
                    // The important part here is that they are loaded in the same order that the command set folders are defined by the user.
                    gem.CommandSets.Add(commandSet);
                }
            }

            return true;
        }

        private static void WriteDefaultCommandSets(Gem gem)
        {
            var defaultCommandSetsFolder = gem.GetDefaultCommandSetsFolder();

            // Currently there is no UI support for creating or editing command sets, so we give an example of what one looks like and write it out
            // The example is always written out to the first command set folder.
            // Note that shell commands like 'echo' are not directly supported, but you could call cmd.exe and pass parameters to a .cmd and use them.
            if (gem.LocalAppData.CommandSetFolders.Count > 0)
            {
                static void writeCommandSetIfNotExist(CommandSet cs)
                {
                    if (!File.Exists(cs.CommandSetPath))
                    {
                        CommandSet.WriteCommandSet(cs);
                    }
                }
                {
                    var exampleCommandSet = new CommandSet()
                    {
                        Placement = CommandSetPlacement.Enlistment,
                        RightClickText = "Status",
                        Verb = "Status",
                        CommandSetPath = Path.Combine(defaultCommandSetsFolder.FullName, "gemstatus.cmdjson"),
                    };
                    exampleCommandSet.Commands.Add(
                        new RunProgramCommand()
                        {
                            Program = "{GitExePath}",
                            Arguments = "status"
                        }
                    );
                    exampleCommandSet.Filters.Add(
                        new CommandSetFilterCloneUrlContains()
                        {
                            SearchFor = "GitEnlistmentManager" // TODO: will need some good documentation around how to set these command sets and filters up.
                        }
                    );
                    writeCommandSetIfNotExist(exampleCommandSet);
                }
                {
                    var prCommandSet = new CommandSet()
                    {
                        Placement = CommandSetPlacement.Enlistment,
                        RightClickText = "Pull Request",
                        Verb = "pr",
                        CommandSetPath = Path.Combine(defaultCommandSetsFolder.FullName, "gempr.cmdjson"),
                    };
                    prCommandSet.Commands.Add(
                        new RunProgramCommand()
                        {
                            OpenNewWindow = true,
                            Program = "{EnlistmentPullRequestUrl}"
                        }
                    );
                    writeCommandSetIfNotExist(prCommandSet);
                }
                {
                    var createEnlistmentCommandSet = new CommandSet()
                    {
                        Placement = CommandSetPlacement.Bucket,
                        RightClickText = "Create New Enlistment",
                        Verb = "ce",
                        CommandSetPath = Path.Combine(defaultCommandSetsFolder.FullName, "gemce.cmdjson"),
                    };
                    createEnlistmentCommandSet.Commands.Add(
                        new CreateEnlistmentCommand()
                    );
                    writeCommandSetIfNotExist(createEnlistmentCommandSet);
                }
                {
                    var createEnlistmentCommandSet = new CommandSet()
                    {
                        Placement = CommandSetPlacement.Enlistment,
                        RightClickText = "Open with 2022 Developer Prompt",
                        Verb = "dev2022",
                        CommandSetPath = Path.Combine(defaultCommandSetsFolder.FullName, "gemdev2022.cmdjson"),
                    };
                    createEnlistmentCommandSet.Commands.Add(
                        new RunProgramCommand()
                        {
                            Program = "wt",
                            Arguments = @"-w gem nt --title ""{RepoName}"" --startingDirectory ""{EnlistmentDirectory}"" %comspec% /k ""C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\Tools\VsDevCmd.bat""",
                            OpenNewWindow = true,
                        }
                    );
                    writeCommandSetIfNotExist(createEnlistmentCommandSet);
                }
                foreach (var csCreate in new List<CommandSetPlacement>()
                {
                    CommandSetPlacement.Enlistment,
                    CommandSetPlacement.Bucket,
                    CommandSetPlacement.Repo,
                    CommandSetPlacement.RepoCollection
                })
                {
                    var listTokensCommandSet = new CommandSet()
                    {
                        Placement = csCreate,
                        RightClickText = "List Tokens",
                        Verb = "lt",
                        CommandSetPath = Path.Combine(defaultCommandSetsFolder.FullName, $"lt{csCreate}.cmdjson"),
                    };
                    listTokensCommandSet.Commands.Add(
                        new ListTokensCommand()
                    );
                    writeCommandSetIfNotExist(listTokensCommandSet);
                }
                {
                    // UI version
                    var archiveEnlistmentCommandSet = new CommandSet()
                    {
                        Placement = CommandSetPlacement.Enlistment,
                        RightClickText = "Archive Enlistment",
                        Verb = string.Empty,
                        CommandSetPath = Path.Combine(defaultCommandSetsFolder.FullName, "gemaeui.cmdjson"),
                    };
                    archiveEnlistmentCommandSet.Commands.Add(
                        new ArchiveEnlistmentCommand()
                    );
                    writeCommandSetIfNotExist(archiveEnlistmentCommandSet);
                }
                {
                    // UI version
                    var archiveEnlistmentCommandSet = new CommandSet()
                    {
                        Placement = CommandSetPlacement.Bucket,
                        RightClickText = string.Empty,
                        Verb = "ae",
                        CommandSetPath = Path.Combine(defaultCommandSetsFolder.FullName, "gemaecmd.cmdjson"),
                    };
                    archiveEnlistmentCommandSet.Commands.Add(
                        new ArchiveEnlistmentCommand()
                    );
                    writeCommandSetIfNotExist(archiveEnlistmentCommandSet);
                }
                {
                    var recreateFromRemoteCommandSet = new CommandSet()
                    {
                        Placement = CommandSetPlacement.Repo,
                        RightClickText = "Re-create all from remote",
                        Verb = "recreate",
                        CommandSetPath = Path.Combine(defaultCommandSetsFolder.FullName, "gemrecreate.cmdjson"),
                    };
                    recreateFromRemoteCommandSet.Commands.Add(
                        new RecreateFromRemoteCommand()
                    );
                    writeCommandSetIfNotExist(recreateFromRemoteCommandSet);
                }
                {
                    var createBucketCommandSet = new CommandSet()
                    {
                        Placement = CommandSetPlacement.Repo,
                        RightClickText = "Create bucket",
                        Verb = "createbucket",
                        CommandSetPath = Path.Combine(defaultCommandSetsFolder.FullName, "gemcreatebucket.cmdjson"),
                    };
                    createBucketCommandSet.Commands.Add(
                        new CreateBucketCommand()
                    );
                    writeCommandSetIfNotExist(createBucketCommandSet);
                }
            }
        }

        public static DirectoryInfo GetAppDataFolder(this Gem _)
        {
            var gemAppDataFolder = new DirectoryInfo(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Gem"));
            if (!gemAppDataFolder.Exists)
            {
                gemAppDataFolder.Create();
            }
            return gemAppDataFolder;
        }

        public static DirectoryInfo GetDefaultCommandSetsFolder(this Gem gem)
        {
            var gemAppDataFolder = gem.GetAppDataFolder();

            var defaultCommandSetsFolder = new DirectoryInfo(Path.Combine(gemAppDataFolder.FullName, "DefaultCommandSets"));
            if (!defaultCommandSetsFolder.Exists)
            {
                defaultCommandSetsFolder.Create();
            }

            return defaultCommandSetsFolder;
        }

        public static bool WriteLocalAppData(this Gem gem)
        {
            var gemAppDataFolder = gem.GetAppDataFolder();

            // Write the metadata
            try
            {
                var gemLocalAppDataFile = new FileInfo(Path.Combine(gemAppDataFolder.FullName, gemLocalAppDataFilename));
                var gemLocalAppDataJson = JsonConvert.SerializeObject(gem.LocalAppData, GemJsonSerializer.Settings);
                File.WriteAllText(gemLocalAppDataFile.FullName, gemLocalAppDataJson);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error writing Gem local application data: {ex.Message}");
                return false;
            }
            return true;
        }

        public static bool ReadLocalAppData(this Gem gem)
        {
            var gemAppDataFolder = gem.GetAppDataFolder();

            try
            {
                var gemLocalAppDataFile = new FileInfo(Path.Combine(gemAppDataFolder.FullName, gemLocalAppDataFilename));
                if (!gemLocalAppDataFile.Exists)
                {
                    // If the local app data doesn't exist then don't default it. The user needs to update the settings.
                    return false;
                }
                else
                {
                    var gemLocalAppDataJson = File.ReadAllText(gemLocalAppDataFile.FullName);
                    var gemLocalAppData = JsonConvert.DeserializeObject<GemLocalAppData>(gemLocalAppDataJson, GemJsonSerializer.Settings);
                    if (gemLocalAppData == null)
                    {
                        MessageBox.Show($"Unable to deserialize Gem local application data from {gemLocalAppDataFile.FullName}");
                        return false;
                    }
                    gem.LocalAppData = gemLocalAppData;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading Gem local application data: {ex.Message}");
                return false;
            }
            return true;
        }

        public static Dictionary<string, string> GetTokens(this Gem gem)
        {
            var tokens = new Dictionary<string, string>();
            tokens["GitExePath"] = gem.LocalAppData.GitExePath;
            if (gem.LocalAppData.ReposFolder != null)
            {
                tokens["ReposFolder"] = gem.LocalAppData.ReposFolder;
            }
            return tokens;
        }

        public static List<CommandSet> GetCommandSets(this Gem gem, CommandSetPlacement placement, CommandSetMode mode, RepoCollection repoCollection, Repo? repo = null, Bucket? bucket = null, Enlistment? enlistment = null)
        {
            var allCommandSets = gem.CommandSets.Where(cs =>
                cs.Placement == placement
                && cs.Matches(
                    repoCollection: repoCollection,
                    repo: repo,
                    bucket: bucket,
                    enlistment: enlistment));

            switch (mode)
            {
                case CommandSetMode.UserInterface:
                    allCommandSets = allCommandSets.Where(cs => !string.IsNullOrWhiteSpace(cs.RightClickText));
                    break;
                case CommandSetMode.CommandPrompt:
                    allCommandSets = allCommandSets.Where(cs => !string.IsNullOrWhiteSpace(cs.Verb));
                    break;
                case CommandSetMode.Any:
                    // Keep everything, no filtering
                    break;
            }

            var commandSets = new List<CommandSet>();
            // Process in reverse so it's easier to add overridden command sets. They are overridden by the verb.
            foreach (var acs in allCommandSets.Reverse())
            {
                // A commandset that doesn't have a verb isn't valid
                if (acs.Verb == null)
                {
                    continue;
                }
                if (!commandSets.Any(cs => cs.Verb != null && cs.Verb.Equals(acs.Verb, StringComparison.OrdinalIgnoreCase)))
                {
                    commandSets.Add(acs);
                }
            }
            commandSets.Reverse();

            return commandSets;
        }
    }
}
