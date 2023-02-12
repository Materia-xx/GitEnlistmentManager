using GitEnlistmentManager.Commands;
using GitEnlistmentManager.CommandSets;
using GitEnlistmentManager.DTOs;
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

        /// <summary>
        /// Creates a clone of the repoCollections, but just the GemName, IsSelected and IsExpanded properties
        /// </summary>
        /// <param name="gem"></param>
        /// <returns></returns>
        private static List<RepoCollection> CloneRepoCollectionsMeta(this Gem gem)
        {
            var clone = new List<RepoCollection>();
            foreach (var rcBase in gem.RepoCollections)
            {
                var rcClone = new RepoCollection(gem, rcBase.RepoCollectionDirectoryPath)
                {
                    GemName = rcBase.GemName,
                    IsExpanded = rcBase.IsExpanded,
                    IsSelected = rcBase.IsSelected
                };
                clone.Add(rcClone);
                foreach (var rBase in rcBase.Repos)
                {
                    var rClone = new Repo(rcClone)
                    {
                        GemName = rBase.GemName,
                        IsExpanded = rBase.IsExpanded,
                        IsSelected = rBase.IsSelected
                    };
                    rcClone.Repos.Add(rClone);
                    foreach (var bBase in rBase.Buckets)
                    {
                        var bClone = new Bucket(rClone)
                        {
                            GemName = bBase.GemName,
                            IsExpanded = bBase.IsExpanded,
                            IsSelected = bBase.IsSelected
                        };
                        rClone.Buckets.Add(bClone);
                        foreach (var eBase in bBase.Enlistments)
                        {
                            var eClone = new Enlistment(bClone)
                            {
                                GemName = eBase.GemName,
                                IsExpanded = eBase.IsExpanded,
                                IsSelected = eBase.IsSelected
                            };
                            bClone.Enlistments.Add(eClone);
                        }
                    }
                }
            }
            return clone;
        }

        private static void ApplyRepoCollectionsMetaClone(this Gem gem, List<RepoCollection> clone)
        {
            foreach (var rcBase in gem.RepoCollections)
            {
                var rcClone = clone.FirstOrDefault(rcClone => rcClone.GemName == rcBase.GemName);
                if (rcClone == null)
                {
                    continue;
                }
                rcBase.IsExpanded = rcClone.IsExpanded;
                rcBase.IsSelected= rcClone.IsSelected;

                foreach (var rBase in rcBase.Repos)
                {
                    var rClone = rcClone.Repos.FirstOrDefault(rClone => rClone.GemName == rBase.GemName);
                    if (rClone == null)
                    {
                        continue;
                    }
                    rBase.IsExpanded = rClone.IsExpanded;
                    rBase.IsSelected = rClone.IsSelected;

                    foreach (var bBase in rBase.Buckets)
                    {
                        var bClone = rClone.Buckets.FirstOrDefault(bClone => bClone.GemName == bBase.GemName);
                        if (bClone == null)
                        {
                            continue;
                        }
                        bBase.IsExpanded = bClone.IsExpanded;
                        bBase.IsSelected = bClone.IsSelected;

                        foreach (var eBase in bBase.Enlistments)
                        {
                            var eClone = bClone.Enlistments.FirstOrDefault(eClone => eClone.GemName == eBase.GemName);
                            if (eClone == null)
                            {
                                continue;
                            }
                            eBase.IsExpanded = eClone.IsExpanded;
                            eBase.IsSelected = eClone.IsSelected;
                        }
                    }
                }
            }
        }

        public static bool ReloadSettings(this Gem gem)
        {
            Gem.LoadingErrors.Clear();

            // If the Gem settings are not yet set, these need to be set first before
            // doing anything else
            while (!gem.ReadLocalAppData() || string.IsNullOrWhiteSpace(gem.LocalAppData.ReposDirectory))
            {
                var gemSettingsEditor = new GemSettings(gem);
                var result = gemSettingsEditor.ShowDialog();
                if (result != null && !result.Value)
                {
                    return false;
                }
            }

            // Remember the state of IsExpanded, IsSelected before reloading
            var repoMetaClone = gem.CloneRepoCollectionsMeta();
            // For each Gem repo collection directory we have, look through it for repo definitions
            gem.RepoCollections.Clear();
            foreach (var repoCollectionDefinitionDirectory in gem.LocalAppData.RepoCollectionDefinitionDirectories)
            {
                var repoCollectionDefinitionInfo = new DirectoryInfo(repoCollectionDefinitionDirectory);
                if (!repoCollectionDefinitionInfo.Exists)
                {
                    Gem.LoadingErrors.Add($"Repo collection definition directory {repoCollectionDefinitionInfo.FullName} was not found");
                    continue;
                }

                var repoCollection = new RepoCollection(gem, repoCollectionDefinitionDirectory)
                {
                    GemName = repoCollectionDefinitionInfo.Name
                };
                gem.RepoCollections.Add(repoCollection);

                // Loop through all the repo registrations that are present here
                var repoRegistrations = repoCollectionDefinitionInfo.GetFiles("*.repojson", SearchOption.TopDirectoryOnly);
                foreach (var repoRegistration in repoRegistrations)
                {
                    var repo = new Repo(repoCollection)
                    {
                        GemName = Path.GetFileNameWithoutExtension(repoRegistration.Name)
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
                    var repoDirectory = repo.GetDirectoryInfo();
                    if (repoDirectory == null)
                    {
                        continue;
                    }

                    foreach (var bucketDirectory in repoDirectory.GetDirectories())
                    {
                        // Don't show the archive directory in the UI
                        if (bucketDirectory.Name.Equals("archive", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }
                        // Don't show .vs directories made by Visual Studio
                        if (bucketDirectory.Name.Equals(".vs", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        var bucket = new Bucket(repo)
                        {
                            GemName = bucketDirectory.Name
                        };
                        repo.Buckets.Add(bucket);

                        foreach (var enlistmentDirectory in bucketDirectory.GetDirectories())
                        {
                            // Don't show .vs directory made by Visual Studio
                            if (enlistmentDirectory.Name.Equals(".vs", StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }

                            var enlistment = new Enlistment(bucket)
                            {
                                GemName = enlistmentDirectory.Name
                            };
                            bucket.Enlistments.Add(enlistment);
                        }
                    }
                }
            }
            // Restore the state of IsExpanded, IsSelected to how it was before the reload
            gem.ApplyRepoCollectionsMetaClone(repoMetaClone);

            WriteDefaultCommandSets(gem);

            // Look for commands in the command set directory
            gem.CommandSets.Clear();

            // Inject the default command set directory at the beginning, but it's not exposed in the UI
            var commandSetDirectories = new List<string>();
            commandSetDirectories.Add(gem.GetDefaultCommandSetsDirectory().FullName);
            commandSetDirectories.AddRange(gem.LocalAppData.CommandSetDirectories);

            try
            {
                foreach (var commandSetDirectory in commandSetDirectories)
                {
                    var commandSetDirectoryInfo = new DirectoryInfo(commandSetDirectory);
                    if (!commandSetDirectoryInfo.Exists)
                    {
                        Gem.LoadingErrors.Add($"Command set directory {commandSetDirectoryInfo.FullName} was not found");
                        continue;
                    }

                    // Loop through all the command set definitions that are present here
                    var commandDefinitions = commandSetDirectoryInfo.GetFiles("*.cmdjson", SearchOption.TopDirectoryOnly);
                    foreach (var commandDefinition in commandDefinitions)
                    {
                        var readResult = CommandSet.ReadCommandSet(commandDefinition.FullName);
                        if (readResult.CommandSet == null)
                        {
                            Gem.LoadingErrors.Add(readResult.LoadingError);
                            continue;
                        }

                        // We always load all command sets. It's when we run them that we decide how to pick which one overrides the others
                        // The important part here is that they are loaded in the same order that the command set directories are defined by the user.
                        gem.CommandSets.Add(readResult.CommandSet);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading Command Sets: {ex.Message}");
            }

            // Commands are a hardcoded list from within the program
            gem.Commands.Clear();
            gem.Commands.Add(typeof(ArchiveEnlistmentCommand));
            gem.Commands.Add(typeof(CompareSelectLeftSideCommand));
            gem.Commands.Add(typeof(CompareToLeftSideCommand));
            gem.Commands.Add(typeof(CreateBucketCommand));
            gem.Commands.Add(typeof(CreateEnlistmentAboveCommand));
            gem.Commands.Add(typeof(CreateEnlistmentCommand));
            gem.Commands.Add(typeof(CreateRepoCommand));
            gem.Commands.Add(typeof(DeleteBucketCommand));
            gem.Commands.Add(typeof(EditGemSettingsCommand));
            gem.Commands.Add(typeof(EditRepoSettingsCommand));
            gem.Commands.Add(typeof(GitCloneCommand));
            gem.Commands.Add(typeof(GitCreateBranchCommand));
            gem.Commands.Add(typeof(GitSetPullDetailsCommand));
            gem.Commands.Add(typeof(GitSetPushDetailsCommand));
            gem.Commands.Add(typeof(GitSetUserDetailsCommand));
            gem.Commands.Add(typeof(ListTokensCommand));
            gem.Commands.Add(typeof(ManageRemoteBranchesCommand));
            gem.Commands.Add(typeof(OpenRootSolutionCommand));
            gem.Commands.Add(typeof(RefreshTreeviewCommand));
            gem.Commands.Add(typeof(RunProgramCommand));
            gem.Commands.Add(typeof(ShowHelpCommand));
            return Gem.LoadingErrors.Count == 0;
        }

        private static void WriteDefaultCommandSets(Gem gem)
        {
            // Currently there is no UI support for creating or editing command sets, so we give an example of what one looks like and write it out
            // Note that shell commands like 'echo' are not directly supported, but you could call cmd.exe and pass parameters to a .cmd and use them.
            var defaultCommandSets = new List<CommandSet>
            {
                new ArchiveEnlistmentCommandSet(CommandSetMode.UserInterface),
                new ArchiveEnlistmentCommandSet(CommandSetMode.CommandPrompt),
                new CreateBucketCommandSet(),
                new CreateEnlistmentAboveCommandSet(),
                new CreateEnlistmentCommandSet(),
                new CreateRepoCommandSet(),
                new CompareSelectLeftSideCommandSet(),
                new CompareToLeftSideCommandSet(),
                new DeleteBucketCommandSet(),
                new EditGemSettingsCommandSet(),
                new EditRepoSettingsCommandSet(),
                new GemStatusCommandSet(),
                new ManageRemoteBranchesCommandSet(),
                new OpenDevVS2022CommandSet(),
                new OpenRootSolutionCommandSet(),
                new PRCommandSet(),
                new RefreshTreeviewCommandSet(),
                new ShowHelpCommandSet(),
            };

            foreach (var placement in new List<CommandSetPlacement>()
            {
                CommandSetPlacement.Enlistment,
                CommandSetPlacement.Bucket,
                CommandSetPlacement.Repo,
                CommandSetPlacement.RepoCollection
            })
            {
                defaultCommandSets.Add(new ListTokensCommandSet(placement));
            }

            var defaultCommandSetsDirectory = gem.GetDefaultCommandSetsDirectory();
            foreach (var commandSet in defaultCommandSets)
            {
                CommandSet.WriteCommandSet(commandSet, defaultCommandSetsDirectory.FullName, overwrite: true);
            }
        }

        public static DirectoryInfo GetAppDataDirectory(this Gem _)
        {
            var gemAppDataDirectory = new DirectoryInfo(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Gem"));
            if (!gemAppDataDirectory.Exists)
            {
                gemAppDataDirectory.Create();
            }
            return gemAppDataDirectory;
        }

        public static DirectoryInfo GetDefaultCommandSetsDirectory(this Gem gem)
        {
            var gemAppDataDirectory = gem.GetAppDataDirectory();

            var defaultCommandSetsDirectory = new DirectoryInfo(Path.Combine(gemAppDataDirectory.FullName, "DefaultCommandSets"));
            if (!defaultCommandSetsDirectory.Exists)
            {
                defaultCommandSetsDirectory.Create();
            }

            return defaultCommandSetsDirectory;
        }

        public static bool WriteLocalAppData(this Gem gem)
        {
            var gemAppDataDirectory = gem.GetAppDataDirectory();

            // Write the metadata
            try
            {
                var gemLocalAppDataFile = new FileInfo(Path.Combine(gemAppDataDirectory.FullName, gemLocalAppDataFilename));
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
            var gemAppDataDirectory = gem.GetAppDataDirectory();

            try
            {
                var gemLocalAppDataFile = new FileInfo(Path.Combine(gemAppDataDirectory.FullName, gemLocalAppDataFilename));
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
            if (gem.LocalAppData.ReposDirectory != null)
            {
                tokens["ReposDirectory"] = gem.LocalAppData.ReposDirectory;
            }
            return tokens;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gem"></param>
        /// <param name="placement">Where in the program the command set is associated with</param>
        /// <param name="mode">If the command set is being ran from the UI or from a command prompt or both</param>
        /// <param name="repoCollection"></param>
        /// <param name="repo"></param>
        /// <param name="bucket"></param>
        /// <param name="enlistment"></param>
        /// <returns></returns>
        public static List<CommandSet> GetCommandSets(this Gem gem, CommandSetPlacement placement, CommandSetMode mode, RepoCollection? repoCollection = null, Repo? repo = null, Bucket? bucket = null, Enlistment? enlistment = null)
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
            // Process in reverse so it's easier to add overridden command sets. They are overridden by the override key.
            foreach (var acs in allCommandSets.Reverse())
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
