using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.DTOs.Commands;
using GitEnlistmentManager.DTOs.CommandSetFilters;
using GitEnlistmentManager.DTOs.CommandSets;
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
                var rcClone = new RepoCollection(gem, rcBase.RepoCollectionFolderPath)
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

            // Remember the state of IsExpanded, IsSelected before reloading
            var repoMetaClone = gem.CloneRepoCollectionsMeta();
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
                        // Don't show .vs folders made by Visual Studio
                        if (bucketFolder.Name.Equals(".vs", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        var bucket = new Bucket(repo)
                        {
                            GemName = bucketFolder.Name
                        };
                        repo.Buckets.Add(bucket);

                        foreach (var enlistmentFolder in bucketFolder.GetDirectories())
                        {
                            // Don't show .vs folders made by Visual Studio
                            if (enlistmentFolder.Name.Equals(".vs", StringComparison.OrdinalIgnoreCase))
                            {
                                continue;
                            }

                            var enlistment = new Enlistment(bucket)
                            {
                                GemName = enlistmentFolder.Name
                            };
                            bucket.Enlistments.Add(enlistment);
                        }
                    }
                }
            }
            // Restore the state of IsExpanded, IsSelected to how it was before the reload
            gem.ApplyRepoCollectionsMetaClone(repoMetaClone);

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
            void writeCommandSetIfNotExist(CommandSet cs)
            {
                CommandSet.WriteCommandSet(cs, defaultCommandSetsFolder.FullName, overwrite: true);
            }

            // Currently there is no UI support for creating or editing command sets, so we give an example of what one looks like and write it out
            // Note that shell commands like 'echo' are not directly supported, but you could call cmd.exe and pass parameters to a .cmd and use them.
            {
                var exampleCommandSet = new GemStatusCommandSet();
                writeCommandSetIfNotExist(exampleCommandSet);
            }
            {
                var prCommandSet = new PRCommandSet();
                writeCommandSetIfNotExist(prCommandSet);
            }
            {
                var createEnlistmentCommandSet = new CreateEnlistmentCommandSet();
                writeCommandSetIfNotExist(createEnlistmentCommandSet);
            }
            {
                var openDevVS2022CommandSet = new OpenDevVS2022CommandSet();
                writeCommandSetIfNotExist(openDevVS2022CommandSet);
            }
            {
                var archiveEnlistmentCommandSet = new ArchiveEnlistmentCommandSet(CommandSetMode.UserInterface);
                writeCommandSetIfNotExist(archiveEnlistmentCommandSet);
            }
            {
                var archiveEnlistmentCommandSet = new ArchiveEnlistmentCommandSet(CommandSetMode.CommandPrompt);
                writeCommandSetIfNotExist(archiveEnlistmentCommandSet);
            }
            {
                var recreateFromRemoteCommandSet = new RecreateFromRemoteCommandSet();
                writeCommandSetIfNotExist(recreateFromRemoteCommandSet);
            }
            {
                var createBucketCommandSet = new CreateBucketCommandSet();
                writeCommandSetIfNotExist(createBucketCommandSet);
            }
            {
                var openRootSolutionCommandSet = new OpenRootSolutionCommandSet();
                writeCommandSetIfNotExist(openRootSolutionCommandSet);
            }
            {
                var deleteBucketCommandSet = new DeleteBucketCommandSet();
                writeCommandSetIfNotExist(deleteBucketCommandSet);
            }
            {
                var compareSelectLeftSideCommandSet = new CompareSelectLeftSideCommandSet();
                writeCommandSetIfNotExist(compareSelectLeftSideCommandSet);
            }
            {
                var compareToLeftSideCommandSet = new CompareToLeftSideCommandSet();
                writeCommandSetIfNotExist(compareToLeftSideCommandSet);
            }

            foreach (var placement in new List<CommandSetPlacement>()
            {
                CommandSetPlacement.Enlistment,
                CommandSetPlacement.Bucket,
                CommandSetPlacement.Repo,
                CommandSetPlacement.RepoCollection
            })
            {
                var listTokensCommandSet = new ListTokensCommandSet(placement);
                writeCommandSetIfNotExist(listTokensCommandSet);
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
        public static List<CommandSet> GetCommandSets(this Gem gem, CommandSetPlacement placement, CommandSetMode mode, RepoCollection repoCollection, Repo? repo = null, Bucket? bucket = null, Enlistment? enlistment = null) // TODO: why does repoCollection here need to be passed in, if it doesn't make it nullable too
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
