using GitEnlistmentManager.ClientServer;
using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace GitEnlistmentManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Gem gem = new();
        private readonly GemServer gemServer;

        public MainWindow()
        {
            InitializeComponent();
            if(!this.ReloadTreeview())
            {
                this.Close();
            }

            this.gemServer = new GemServer(this.ProcessCSCommand);
            treeRepos.PreviewMouseRightButtonDown += TreeRepos_PreviewMouseRightButtonDown;
        }

        protected override async void OnActivated(EventArgs e)
        {
            await this.gemServer.Start().ConfigureAwait(false);
            base.OnActivated(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            this.gemServer?.Stop();
        }

        private bool ReloadTreeview()
        {
            treeRepos.ItemsSource = null;
            if (!this.gem.ReloadSettings())
            {
                return false;
            }
            treeRepos.ItemsSource = gem.RepoCollections;
            return true;
        }

        public async Task ProcessCSCommand(GemCSCommand command)
        {
            switch (command.CommandType)
            {
                case GemCSCommandType.InterpretCommandLine:
                    // There has to be at least 1 argument coming in
                    if (command.CommandArgs == null || command.CommandArgs.Length == 0)
                    {
                        return;
                    }

                    // Working directory is required and must be under the one specified in the settings
                    if (string.IsNullOrWhiteSpace(command.WorkingDirectory))
                    {
                        return;
                    }
                    if (this.gem.LocalAppData.ReposFolder == null || !command.WorkingDirectory.StartsWith(this.gem.LocalAppData.ReposFolder, StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }

                    // The first parameter has to be the verb of the command set to run

                    var verb = command.CommandArgs[0];

                    // Figure out the context of where the command being run from.
                    // It has to be running from within the repos folder.
                    var relativeWorkingDirectory = command.WorkingDirectory.Substring(this.gem.LocalAppData.ReposFolder.Length);
                    var workingDirParts = relativeWorkingDirectory.Trim('\\').Split('\\');

                    RepoCollection? repoCollection = null;
                    Repo? repo = null;
                    Bucket? bucket = null;
                    Enlistment? enlistment = null;

                    if (workingDirParts.Length > 0 && !string.IsNullOrWhiteSpace(workingDirParts[0]))
                    {
                        repoCollection = gem.RepoCollections.FirstOrDefault(rc => rc.Name != null && rc.Name.Equals(workingDirParts[0], StringComparison.OrdinalIgnoreCase));
                    }
                    if (repoCollection != null && workingDirParts.Length > 1 && !string.IsNullOrWhiteSpace(workingDirParts[1]))
                    {
                        repo = repoCollection.Repos.FirstOrDefault(r => r.Name != null && r.Name.Equals(workingDirParts[1], StringComparison.OrdinalIgnoreCase));
                    }
                    if (repo != null && workingDirParts.Length > 2 && !string.IsNullOrWhiteSpace(workingDirParts[2]))
                    {
                        bucket = repo.Buckets.FirstOrDefault(b => b.Name != null && b.Name.Equals(workingDirParts[2], StringComparison.OrdinalIgnoreCase));
                    }
                    if (bucket != null && workingDirParts.Length > 3 && !string.IsNullOrWhiteSpace(workingDirParts[3]))
                    {
                        enlistment = bucket.Enlistments.FirstOrDefault(e => e.Name != null && e.Name.Equals(workingDirParts[3], StringComparison.OrdinalIgnoreCase));
                    }

                    var placement = enlistment != null ? CommandSetPlacement.Enlistment :
                        bucket != null ? CommandSetPlacement.Bucket :
                        repo != null ? CommandSetPlacement.Repo :
                        repoCollection != null ? CommandSetPlacement.RepoCollection : CommandSetPlacement.RepoCollection;

                    // We always have to have a repo collection
                    if (repoCollection == null)
                    {
                        return;
                    }

                    // All command sets that are available for this placement
                    var commandSets = gem.GetCommandSets(placement, repoCollection, repo, bucket, enlistment);

                    // Grab the last one that matches the verb being specified
                    var commandSet = commandSets.LastOrDefault(cmdSet => cmdSet.Verb != null && cmdSet.Verb.Equals(verb, StringComparison.OrdinalIgnoreCase));

                    // If no command set with that verb was found then write out something in the UI
                    if (commandSet == null)
                    {
                        // TODO: write out error
                    }
                    else
                    {
                        await this.RunCommandSet(
                            commandSet: commandSet,
                            tokens: enlistment?.GetTokens() ?? bucket?.GetTokens() ?? repo?.GetTokens() ?? repoCollection.GetTokens(),
                            workingFolder: enlistment?.GetDirectoryInfo()?.FullName ?? bucket?.GetDirectoryInfo()?.FullName ?? repo?.GetDirectoryInfo()?.FullName ?? repoCollection.RepoCollectionFolderPath
                            ).ConfigureAwait(false);
                    }
                    break;
            }
        }


        private static TreeViewItem? VisualUpwardSearch(DependencyObject? source)
        {
            while (source != null && !(source is TreeViewItem))
                source = VisualTreeHelper.GetParent(source);

            return source as TreeViewItem;
        }

        public static void ClearTreeViewSelection(TreeView tview)
        {
            if (tview != null)
            {
                ClearTreeViewItemsControlSelection(tview.Items, tview.ItemContainerGenerator);
            }
        }

        private static void ClearTreeViewItemsControlSelection(ItemCollection ic, ItemContainerGenerator icg)
        {
            if (ic == null || icg == null)
            {
                return;
            }

            for (int i = 0; i < ic.Count; i++)
            {
                // Get the TreeViewItem
                TreeViewItem? tvi = icg.ContainerFromIndex(i) as TreeViewItem;
                if (tvi != null)
                {
                    // Recursive call to traverse deeper levels
                    ClearTreeViewItemsControlSelection(tvi.Items, tvi.ItemContainerGenerator);
                    // Deselect the TreeViewItem 
                    tvi.IsSelected = false;
                }
            }
        }

        private void TreeRepos_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Clear the selected item first when rightclicking. If the user is clicking on a node then
            // it will re-select it.
            ClearTreeViewSelection(treeRepos);

            var treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);

            var menu = new ContextMenu();
            treeRepos.ContextMenu = menu;

            // If no items are selected then just show these basic choices
            if (treeViewItem == null)
            {
                var menuEditGemSettings = new MenuItem
                {
                    Header = "Edit Gem Settings"
                };
                menuEditGemSettings.Click += (s, e) =>
                {
                    var gemSettingsEditor = new GemSettings(this.gem);
                    gemSettingsEditor.ShowDialog();
                    // After the editor closes, reload the UI so we pick up any changes made
                    this.ReloadTreeview();
                };
                menu.Items.Add(menuEditGemSettings);
            }
            else
            {
                treeViewItem.Focus();
                treeViewItem.IsSelected = true;

                // Figure out what kind of treeviewItem we're clicking on (repo/bucket/enlistment) and create different menus for each type
                var selectedItem = treeViewItem.DataContext;

                if (selectedItem is RepoCollection repoCollection)
                {
                    var menuAddNewRepo = new MenuItem
                    {
                        Header = "Add New Repo"
                    };
                    menuAddNewRepo.Click += (s, e) =>
                    {
                        var repoSettingsEditor = new RepoSettings(new Repo(repoCollection), isNew: true);
                        repoSettingsEditor.ShowDialog();
                        // After the editor closes, reload the UI so we pick up any changes made
                        this.ReloadTreeview();
                    };
                    menu.Items.Add(menuAddNewRepo);

                    // Attach "RepoCollection" type command sets to the menu
                    var repoCollectionCommandSets = gem.GetCommandSets(CommandSetPlacement.RepoCollection, repoCollection);
                    foreach (var repoCollectionCommandSet in repoCollectionCommandSets)
                    {
                        var menuRepoCommandSet = new MenuItem()
                        {
                            Header = repoCollectionCommandSet.RightClickText
                        };
                        menuRepoCommandSet.Click += async (s, e) =>
                        {
                            await this.ClearCommandWindow().ConfigureAwait(false);
                            await this.RunCommandSet(repoCollectionCommandSet, repoCollection.GetTokens(), repoCollection.RepoCollectionFolderPath).ConfigureAwait(false);
                        };
                        menu.Items.Add(menuRepoCommandSet);
                    }
                }

                if (selectedItem is Repo repo)
                {
                    if (repo.GetDirectoryInfo() != null)
                    {
                        var menuEditRepoSettings = new MenuItem()
                        {
                            Header = "Edit Repo Settings"
                        };
                        menuEditRepoSettings.Click += (s, e) =>
                        {
                            var repoSettingsEditor = new RepoSettings(repo, isNew: false);
                            repoSettingsEditor.ShowDialog();
                            // After the editor closes, reload the UI so we pick up any changes made
                            this.ReloadTreeview();
                        };
                        menu.Items.Add(menuEditRepoSettings);

                        var menuAddNewBucket = new MenuItem()
                        {
                            Header = "Add New Bucket"
                        };
                        menuAddNewBucket.Click += (s, e) =>
                        {
                            var bucketSettingsEditor = new BucketSettings(new Bucket(repo), this);
                            bucketSettingsEditor.ShowDialog();

                            // After the editor closes, reload the UI so we pick up any changes made
                            this.ReloadTreeview();
                        };
                        menu.Items.Add(menuAddNewBucket);

                        // Attach "Repo" type command sets to the menu
                        var repoCommandSets = gem.GetCommandSets(CommandSetPlacement.Repo, repo.RepoCollection, repo);
                        foreach (var repoCommandSet in repoCommandSets)
                        {
                            var menuRepoCommandSet = new MenuItem()
                            {
                                Header = repoCommandSet.RightClickText
                            };
                            menuRepoCommandSet.Click += async (s, e) =>
                            {
                                await this.ClearCommandWindow().ConfigureAwait(false);
                                await this.RunCommandSet(repoCommandSet, repo.GetTokens(), repo.GetDirectoryInfo()?.FullName).ConfigureAwait(false);
                            };
                            menu.Items.Add(menuRepoCommandSet);
                        }
                    }
                }

                if (selectedItem is Bucket bucket)
                {
                    var menuAddNewEnlistment = new MenuItem() // TODO: all enlistment options should only be there if all the settings needed to create an enlistment are set up properly
                    {
                        Header = "Add New Enlistment"
                    };
                    menuAddNewEnlistment.Click += async (s, e) =>
                    {
                        var enlistment = new Enlistment(bucket);
                        var enlistmentSettingsEditor = new EnlistmentSettings(enlistment);
                        enlistmentSettingsEditor.ShowDialog();
                        // After the editor closes, create the enlistment
                        await enlistment.CreateEnlistment(this, EnlistmentPlacement.PlaceAtEnd).ConfigureAwait(true);
                        // Reload the UI so we pick up any changes made
                        this.ReloadTreeview();
                    };
                    menu.Items.Add(menuAddNewEnlistment);

                    // Attach "Bucket" type command sets to the menu
                    var bucketCommandSets = gem.GetCommandSets(CommandSetPlacement.Bucket, bucket.Repo.RepoCollection, bucket.Repo, bucket);
                    foreach (var bucketCommandSet in bucketCommandSets)
                    {
                        var menuBucketCommandSet = new MenuItem()
                        {
                            Header = bucketCommandSet.RightClickText
                        };
                        menuBucketCommandSet.Click += async (s, e) =>
                        {
                            await this.ClearCommandWindow().ConfigureAwait(false);
                            await this.RunCommandSet(bucketCommandSet, bucket.GetTokens(), bucket.GetDirectoryInfo()?.FullName).ConfigureAwait(false);
                        };
                        menu.Items.Add(menuBucketCommandSet);
                    }
                }

                if (selectedItem is Enlistment enlistment)
                {
                    var menuAddNewEnlistmentAbove = new MenuItem()
                    {
                        Header = "Add New Enlistment Above"
                    };
                    menuAddNewEnlistmentAbove.Click += async (s, e) =>
                    {
                        var newEnlistment = new Enlistment(enlistment.Bucket);
                        var enlistmentSettingsEditor = new EnlistmentSettings(newEnlistment);
                        enlistmentSettingsEditor.ShowDialog();
                        // After the editor closes, create the enlistment
                        await newEnlistment.CreateEnlistment(this, EnlistmentPlacement.PlaceAbove, referenceEnlistment: enlistment).ConfigureAwait(true);
                        // Reload the UI so we pick up any changes made
                        this.ReloadTreeview();
                    };
                    menu.Items.Add(menuAddNewEnlistmentAbove);

                    // Attach "Enlistment" type command sets to the menu
                    var enlistmentCommandSets = gem.GetCommandSets(CommandSetPlacement.Enlistment, enlistment.Bucket.Repo.RepoCollection, enlistment.Bucket.Repo, enlistment.Bucket, enlistment);
                    foreach (var enlistmentCommandSet in enlistmentCommandSets)
                    {
                        var menuEnlistmentCommandSet = new MenuItem()
                        {
                            Header = enlistmentCommandSet.RightClickText
                        };
                        menuEnlistmentCommandSet.Click += async (s, e) =>
                        {
                            await this.ClearCommandWindow().ConfigureAwait(false);
                            await this.RunCommandSet(enlistmentCommandSet, enlistment.GetTokens(), enlistment.GetDirectoryInfo()?.FullName).ConfigureAwait(false);
                        };
                        menu.Items.Add(menuEnlistmentCommandSet);
                    }
                }

                e.Handled = true;
            }
        }

        public async Task ClearCommandWindow()
        {
            await txtCommandPrompt.Clear().ConfigureAwait(false);
        }

        public async Task<bool> RunCommandSets(List<CommandSet> commandSets, Dictionary<string, string> tokens, string? workingFolder = null)
        {
            foreach (var commandSet in commandSets)
            {
                if (!await this.RunCommandSet(
                    commandSet: commandSet,
                    tokens: tokens,
                    workingFolder: workingFolder
                    ).ConfigureAwait(false))
                {
                    return false;
                }
            }
            return true;
        }

        public async Task<bool> RunCommandSet(CommandSet commandSet, Dictionary<string, string> tokens, string? workingFolder = null)
        {
            foreach (var command in commandSet.Commands)
            {
                if (!await this.RunCommand(
                    programPath: command.Program,
                    arguments: command.Arguments,
                    tokens: tokens,
                    workingFolder: workingFolder
                    ))
                {
                    return false;
                }
            }
            return true;
        }

        public async Task<bool> RunCommand(string? programPath, string? arguments, Dictionary<string, string> tokens, string? workingFolder = null)
        {
            foreach (var token in tokens)
            {
                programPath = programPath?.Replace($"{{{token.Key}}}", token.Value);
                arguments = arguments?.Replace($"{{{token.Key}}}", token.Value);
            }

            if (workingFolder != null)
            {
                // I have no idea why this line needs an extra Environment.NewLine added but the others don't
                await txtCommandPrompt.AppendLine($"cd \"{workingFolder}\"{Environment.NewLine}", Brushes.White).ConfigureAwait(false);
            }
            await txtCommandPrompt.AppendLine($"\"{programPath}\" {arguments}", Brushes.White).ConfigureAwait(false);

            bool useShellExecute = false;
            // We need shell execute to open urls
            if (programPath != null && programPath.StartsWith("http"))
            {
                useShellExecute = true;
            }

            using Process process = new();
            process.StartInfo = new()
            {
                FileName = programPath,
                Arguments = arguments,
                UseShellExecute = useShellExecute,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = !useShellExecute,
                RedirectStandardError = !useShellExecute,
                CreateNoWindow = true,
                WorkingDirectory = workingFolder
            };

            if (!useShellExecute)
            {
                process.OutputDataReceived += new DataReceivedEventHandler(RunCommand_Output);
                process.ErrorDataReceived += new DataReceivedEventHandler(RunCommand_Error);
            }
            process.Start();
            if (!useShellExecute)
            {
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            }
            await process.WaitForExitAsync().ConfigureAwait(false);
            process.Refresh();

            var exitCode = -1;
            if (process.HasExited)
            {
                exitCode = process.ExitCode;
            }
            process.Close();

            await txtCommandPrompt.AppendLine(string.Empty, Brushes.White).ConfigureAwait(false);
            // Exit code 0 is success. This works for git, but won't work for things like RoboCopy.
            return exitCode == 0;
        }

        private async void RunCommand_Output(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                await txtCommandPrompt.AppendLine(e.Data, Brushes.LightGray).ConfigureAwait(false);
                await txtCommandPrompt.Dispatcher.BeginInvoke(() =>
                {
                    txtCommandPrompt.ScrollToEnd();
                });
            }
        }

        private async void RunCommand_Error(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                await txtCommandPrompt.AppendLine(e.Data, Brushes.DarkGray).ConfigureAwait(false);
                await txtCommandPrompt.Dispatcher.BeginInvoke(() =>
                {
                    txtCommandPrompt.ScrollToEnd();
                });
            }
        }
    }
}
