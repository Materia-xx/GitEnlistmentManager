using GitEnlistmentManager.ClientServer;
using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Extensions;
using GitEnlistmentManager.Globals;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
            this.Icon = Icons.GemIcon;
            this.gemServer = new GemServer(this.ProcessCSCommand);
            this.Loaded += MainWindow_Loaded;
            treeRepos.PreviewMouseRightButtonDown += TreeRepos_PreviewMouseRightButtonDown;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (!await this.ReloadTreeview().ConfigureAwait(false))
            {
                this.Close();
            }

            this.gemServer.Start();
        }

        protected override void OnClosed(EventArgs e)
        {
            this.gemServer?.Stop();
        }

        private async Task<bool> ReloadTreeview()
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                treeRepos.ItemsSource = null;
            });
            if (!this.gem.ReloadSettings())
            {
                return false;
            }
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                treeRepos.ItemsSource = gem.RepoCollections;
            });

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

                    var remainingArgsStack = new Stack<string>();
                    for (int i = command.CommandArgs.Length - 1; i >= 0; i--)
                    {
                        var commandArg = command.CommandArgs[i].ToString();
                        if (commandArg != null)
                        {
                            remainingArgsStack.Push(commandArg);
                        }
                    }

                    // If the user is asking for help
                    var firstArgument = remainingArgsStack.Pop();
                    if (firstArgument != null && firstArgument.Equals("--help", StringComparison.OrdinalIgnoreCase))
                    {
                        await this.ShowHelp().ConfigureAwait(false);
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

                    var verb = command.CommandArgs[0].ToString();

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
                        repoCollection = gem.RepoCollections.FirstOrDefault(rc => rc.GemName != null && rc.GemName.Equals(workingDirParts[0], StringComparison.OrdinalIgnoreCase));
                    }
                    if (repoCollection != null && workingDirParts.Length > 1 && !string.IsNullOrWhiteSpace(workingDirParts[1]))
                    {
                        repo = repoCollection.Repos.FirstOrDefault(r => r.Metadata.ShortName != null && r.Metadata.ShortName.Equals(workingDirParts[1], StringComparison.OrdinalIgnoreCase));
                    }
                    if (repo != null && workingDirParts.Length > 2 && !string.IsNullOrWhiteSpace(workingDirParts[2]))
                    {
                        bucket = repo.Buckets.FirstOrDefault(b => b.GemName != null && b.GemName.Equals(workingDirParts[2], StringComparison.OrdinalIgnoreCase));
                    }
                    if (bucket != null && workingDirParts.Length > 3 && !string.IsNullOrWhiteSpace(workingDirParts[3]))
                    {
                        enlistment = bucket.Enlistments.FirstOrDefault(e => e.GemName != null && e.GemName.Equals(workingDirParts[3], StringComparison.OrdinalIgnoreCase));
                    }

                    var nodeContext = new GemNodeContext()
                    {
                        RepoCollection = repoCollection,
                        Repo = repo,
                        Bucket = bucket,
                        Enlistment = enlistment
                    };

                    // We always have to have a repo collection
                    if (repoCollection == null)
                    {
                        return;
                    }

                    // All command sets that are available for this placement
                    var commandSets = gem.GetCommandSets(nodeContext.GetPlacement(), CommandSetMode.CommandPrompt, repoCollection, repo, bucket, enlistment);

                    // Grab the last one that matches the verb being specified
                    var commandSet = commandSets.LastOrDefault(cs => cs.Verb != null && cs.Verb.Equals(verb, StringComparison.OrdinalIgnoreCase));

                    // If no command set with that verb was found then write out something in the UI
                    if (commandSet == null)
                    {
                        await this.AppendCommandLine($"No commands with verb '{verb}' were found.", Brushes.LightSalmon).ConfigureAwait(false);
                    }
                    else
                    {
                        foreach (var commandSetCommand in commandSet.Commands)
                        {
                            commandSetCommand.ParseArgs(nodeContext, remainingArgsStack);
                        }
                        await this.RunCommandSet(
                            commandSet: commandSet,
                            nodeContext: nodeContext
                            ).ConfigureAwait(false);
                    }
                    break;
            }
        }

        private async Task ShowHelp()
        {
            var helpText = @"GEM - Git Enlistment Manager
Command Sets
  Command sets can include tokens in the form of {token}. The program and arguments are the only 2 fields that allow token replacement.
  The tokens that are available depend on where the command set is being run from. To get a list of currently available tokens use
  the lt (list tokens) default command set. e.g. ""gem lt"" within a specific gem directory.
            ";

            await this.ClearCommandWindow().ConfigureAwait(false);
            await txtCommandPrompt.AppendLine(helpText, Brushes.AliceBlue).ConfigureAwait(false);
            await txtCommandPrompt.Dispatcher.BeginInvoke(() =>
            {
                txtCommandPrompt.ScrollToEnd();
            });
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
            // Clear the selected item first when right-clicking. If the user is clicking on a node then
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
                menuEditGemSettings.Click += async (s, e) =>
                {
                    var gemSettingsEditor = new GemSettings(this.gem);
                    gemSettingsEditor.ShowDialog();
                    // After the editor closes, reload the UI so we pick up any changes made
                    await this.ReloadTreeview().ConfigureAwait(false);
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
                    menuAddNewRepo.Click += async (s, e) =>
                    {
                        var repoSettingsEditor = new RepoSettings(new Repo(repoCollection), isNew: true);
                        repoSettingsEditor.ShowDialog();
                        // After the editor closes, reload the UI so we pick up any changes made
                        await this.ReloadTreeview().ConfigureAwait(false);
                    };
                    menu.Items.Add(menuAddNewRepo);

                    // Attach "RepoCollection" type command sets to the menu
                    var repoCollectionCommandSets = gem.GetCommandSets(CommandSetPlacement.RepoCollection, CommandSetMode.UserInterface, repoCollection);
                    foreach (var repoCollectionCommandSet in repoCollectionCommandSets)
                    {
                        var menuRepoCommandSet = new MenuItem()
                        {
                            Header = repoCollectionCommandSet.RightClickText
                        };
                        menuRepoCommandSet.Click += async (s, e) =>
                        {
                            await this.ClearCommandWindow().ConfigureAwait(false);
                            await this.RunCommandSet(repoCollectionCommandSet, GemNodeContext.GetNodeContext(repoCollection: repoCollection)).ConfigureAwait(false);
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
                        menuEditRepoSettings.Click += async (s, e) =>
                        {
                            var repoSettingsEditor = new RepoSettings(repo, isNew: false);
                            repoSettingsEditor.ShowDialog();
                            // After the editor closes, reload the UI so we pick up any changes made
                            await this.ReloadTreeview().ConfigureAwait(false);
                        };
                        menu.Items.Add(menuEditRepoSettings);

                        // Attach "Repo" type command sets to the menu
                        var repoCommandSets = gem.GetCommandSets(CommandSetPlacement.Repo, CommandSetMode.UserInterface, repo.RepoCollection, repo);
                        foreach (var repoCommandSet in repoCommandSets)
                        {
                            var menuRepoCommandSet = new MenuItem()
                            {
                                Header = repoCommandSet.RightClickText
                            };
                            menuRepoCommandSet.Click += async (s, e) =>
                            {
                                await this.ClearCommandWindow().ConfigureAwait(false);
                                await this.RunCommandSet(repoCommandSet, GemNodeContext.GetNodeContext(repo: repo)).ConfigureAwait(false);
                            };
                            menu.Items.Add(menuRepoCommandSet);
                        }
                    }
                }

                if (selectedItem is Bucket bucket)
                {
                    // Attach "Bucket" type command sets to the menu
                    var bucketCommandSets = gem.GetCommandSets(CommandSetPlacement.Bucket, CommandSetMode.UserInterface, bucket.Repo.RepoCollection, bucket.Repo, bucket);
                    foreach (var bucketCommandSet in bucketCommandSets)
                    {
                        var menuBucketCommandSet = new MenuItem()
                        {
                            Header = bucketCommandSet.RightClickText
                        };
                        menuBucketCommandSet.Click += async (s, e) =>
                        {
                            await this.ClearCommandWindow().ConfigureAwait(false);
                            await this.RunCommandSet(bucketCommandSet, GemNodeContext.GetNodeContext(bucket: bucket)).ConfigureAwait(false);
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
                        await newEnlistment.CreateEnlistment(this, EnlistmentPlacement.PlaceAbove, childEnlistment: enlistment).ConfigureAwait(false);
                        // Reload the UI so we pick up any changes made
                        await this.ReloadTreeview().ConfigureAwait(false);
                    };
                    menu.Items.Add(menuAddNewEnlistmentAbove);

                    // Attach "Enlistment" type command sets to the menu
                    var enlistmentCommandSets = gem.GetCommandSets(CommandSetPlacement.Enlistment, CommandSetMode.UserInterface, enlistment.Bucket.Repo.RepoCollection, enlistment.Bucket.Repo, enlistment.Bucket, enlistment);
                    foreach (var enlistmentCommandSet in enlistmentCommandSets)
                    {
                        var menuEnlistmentCommandSet = new MenuItem()
                        {
                            Header = enlistmentCommandSet.RightClickText
                        };
                        menuEnlistmentCommandSet.Click += async (s, e) =>
                        {
                            await this.ClearCommandWindow().ConfigureAwait(false);
                            await this.RunCommandSet(enlistmentCommandSet, GemNodeContext.GetNodeContext(enlistment: enlistment)).ConfigureAwait(false);
                        };
                        menu.Items.Add(menuEnlistmentCommandSet);
                    }
                }

                e.Handled = true;
            }
        }

        public async Task ClearCommandWindow()
        {
            await this.txtCommandPrompt.Clear().ConfigureAwait(false);
            await this.AppendCommandLine(string.Empty, Brushes.White).ConfigureAwait(false);
        }

        public async Task AppendCommandLine(string text, Brush brush)
        {
            await txtCommandPrompt.AppendLine(text, brush).ConfigureAwait(false);
            await txtCommandPrompt.Dispatcher.BeginInvoke(() =>
            {
                txtCommandPrompt.ScrollToEnd();
            });
        }

        public async Task<bool> RunCommandSets(List<CommandSet> commandSets, GemNodeContext nodeContext)
        {
            foreach (var commandSet in commandSets)
            {
                if (!await this.RunCommandSet(
                    commandSet: commandSet,
                    nodeContext: nodeContext
                    ).ConfigureAwait(false))
                {
                    return false;
                }
            }
            return true;
        }

        public async Task<bool> RunCommandSet(CommandSet commandSet, GemNodeContext nodeContext)
        {
            foreach (var command in commandSet.Commands)
            {
                await command.Execute(nodeContext, this).ConfigureAwait(false);

                // All commands have the potential to change the structure of the repos/enlistments etc.
                // A fully custom command wouldn't have the option to reload the UI.
                // So reload the UI here so we pick up any changes made
                await this.ReloadTreeview().ConfigureAwait(false);
            }
            return true;
        }

        public async Task<bool> RunProgram(string? programPath, string? arguments, Dictionary<string, string>? tokens, bool openNewWindow, string? workingFolder = null)
        {
            // Replace tokens that come from GEM. These are in the format of {token}
            if (tokens != null)
            {
                foreach (var token in tokens)
                {
                    programPath = programPath?.Replace($"{{{token.Key}}}", token.Value, StringComparison.OrdinalIgnoreCase);
                    arguments = arguments?.Replace($"{{{token.Key}}}", token.Value, StringComparison.OrdinalIgnoreCase);
                    workingFolder = workingFolder?.Replace($"{{{token.Key}}}", token.Value, StringComparison.OrdinalIgnoreCase);
                }
            }

            // Replace environment variables formatted like %comspec%
            var envVarsCaseSensitive = Environment.GetEnvironmentVariables();
            var envVarsCaseInsensitive = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var envVarName in envVarsCaseSensitive.Keys)
            {
                var name = envVarName.ToString();
                var value = envVarsCaseSensitive[envVarName]?.ToString();
                if (value != null && name != null)
                {
                    envVarsCaseInsensitive[name] = value;
                }
            }
            foreach (var envVarName in envVarsCaseInsensitive.Keys)
            {
                var find = $"%{envVarName}%";
                var replace = envVarsCaseInsensitive[envVarName];

                programPath = programPath?.Replace(find, replace, StringComparison.OrdinalIgnoreCase);
                arguments = arguments?.Replace(find, replace, StringComparison.OrdinalIgnoreCase);
            }

            if (workingFolder != null)
            {
                await txtCommandPrompt.AppendLine($"cd \"{workingFolder}\"", Brushes.White).ConfigureAwait(false);
            }
            await txtCommandPrompt.AppendLine($"\"{programPath}\" {arguments}", Brushes.White).ConfigureAwait(false);

            bool useShellExecute = false; // TODO: pass this in as a parameter from the command, it might not always be http, but something else the shell still needs to handle
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
                WindowStyle = openNewWindow ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden,
                RedirectStandardOutput = !useShellExecute,
                RedirectStandardError = !useShellExecute,
                CreateNoWindow = !openNewWindow,
                WorkingDirectory = workingFolder
            };

            if (!useShellExecute)
            {
                // UseShellExecute must be false in order to capture data
                process.OutputDataReceived += new DataReceivedEventHandler(RunCommand_Output);
                process.ErrorDataReceived += new DataReceivedEventHandler(RunCommand_Error);

                // UseShellExecute must be false in order to use environment variables
                // Inject the path to where GEM is running from into the environment path so it's callable from the commandline.
                var gemExe = Assembly.GetExecutingAssembly().FullName;
                string? gemExeDirectory = null;
                if (gemExe != null)
                {
                    gemExeDirectory = new FileInfo(gemExe)?.Directory?.FullName;
                }
                process.StartInfo.Environment["Path"] = $"{Environment.GetEnvironmentVariable("Path")};{gemExeDirectory}";
            }

            try
            {
                process.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return false;
            }

            if (!useShellExecute)
            {
                // UseShellExecute must be false in order to capture data
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