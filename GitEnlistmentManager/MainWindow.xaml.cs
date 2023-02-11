using GitEnlistmentManager.ClientServer;
using GitEnlistmentManager.CommandSets;
using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Extensions;
using GitEnlistmentManager.Globals;
using System;
using System.Collections.Generic;
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
        public event EventHandler? FullyLoaded;

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
            treeRepos.PreviewMouseRightButtonDown += TreeRepos_PreviewMouseRightButtonDown;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (!await this.ReloadTreeview().ConfigureAwait(false))
            {
                this.Close();
            }
            FullyLoaded?.Invoke(this, new EventArgs());
        }

        public async Task<bool> ReloadTreeview()
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                treeRepos.ItemsSource = null;
            });
            if (!Gem.Instance.ReloadSettings())
            {
                return false;
            }
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                treeRepos.ItemsSource = Gem.Instance.RepoCollections;
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

                    // Working directory is required and must be under the one specified in the settings
                    if (string.IsNullOrWhiteSpace(command.WorkingDirectory))
                    {
                        MessageBox.Show("Working directory is required for running a client/server command");
                        return;
                    }
                    if (Gem.Instance.LocalAppData.ReposDirectory == null)
                    {
                        MessageBox.Show("The LocalAppData ReposDirectory is not loaded or set properly.");
                        return;
                    }
                    if (!command.WorkingDirectory.StartsWith(Gem.Instance.LocalAppData.ReposDirectory, StringComparison.OrdinalIgnoreCase))
                    {
                        MessageBox.Show("The directory Gem starts in should be within the ReposDirectory.");
                        return;
                    }

                    // The first parameter has to be the verb of the command set to run
                    // Special handling if the verb is a help request
                    var verb = remainingArgsStack.Pop();
                    if (verb != null
                        && (verb.Equals("--help", StringComparison.OrdinalIgnoreCase) || verb.Equals("-?") || verb.Equals("/?")))
                    {
                        verb = "help";
                    }

                    // Figure out the context of where the command being run from.
                    // It has to be running from within the repos directory.
                    var relativeWorkingDirectory = command.WorkingDirectory.Substring(Gem.Instance.LocalAppData.ReposDirectory.Length);
                    var workingDirParts = relativeWorkingDirectory.Trim('\\').Split('\\');

                    RepoCollection? repoCollection = null;
                    Repo? repo = null;
                    Bucket? bucket = null;
                    Enlistment? enlistment = null;

                    if (workingDirParts.Length > 0 && !string.IsNullOrWhiteSpace(workingDirParts[0]))
                    {
                        repoCollection = Gem.Instance.RepoCollections.FirstOrDefault(rc => rc.GemName != null && rc.GemName.Equals(workingDirParts[0], StringComparison.OrdinalIgnoreCase));
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
                        MessageBox.Show("RepoCollection was not set properly");
                        return;
                    }

                    // All command sets that are available for this placement
                    var commandSets = Gem.Instance.GetCommandSets(nodeContext.GetPlacement(), CommandSetMode.CommandPrompt, repoCollection, repo, bucket, enlistment);

                    // Grab the last one that matches the verb being specified
                    var commandSet = commandSets.LastOrDefault(cs => cs.Verb != null && cs.Verb.Equals(verb, StringComparison.OrdinalIgnoreCase));

                    // If no command set with that verb was found then write out something in the UI
                    if (commandSet == null)
                    {
                        var message = $"No command sets with verb '{verb}' were found.";
                        MessageBox.Show(message);
                        await this.AppendCommandLine(message, Brushes.LightSalmon).ConfigureAwait(false);
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
            List<CommandSet>? commandSets = null;
            GemNodeContext? nodeContext = null;

            // If no items are selected then just show these basic choices
            if (treeViewItem == null)
            {
                commandSets = Gem.Instance.GetCommandSets(CommandSetPlacement.Gem, CommandSetMode.UserInterface);
                nodeContext = new GemNodeContext();
            }
            else
            {
                treeViewItem.Focus();
                treeViewItem.IsSelected = true;

                // Figure out what kind of treeviewItem we're clicking on (repo/bucket/enlistment) and create different menus for each type
                var selectedItem = treeViewItem.DataContext;
                if (selectedItem is RepoCollection repoCollection)
                {
                    commandSets = Gem.Instance.GetCommandSets(CommandSetPlacement.RepoCollection, CommandSetMode.UserInterface, repoCollection);
                    nodeContext = GemNodeContext.GetNodeContext(repoCollection: repoCollection);
                }
                else if (selectedItem is Repo repo)
                {
                    commandSets = Gem.Instance.GetCommandSets(CommandSetPlacement.Repo, CommandSetMode.UserInterface, repo.RepoCollection, repo);
                    nodeContext = GemNodeContext.GetNodeContext(repo: repo);
                }
                else if (selectedItem is Bucket bucket)
                {
                    commandSets = Gem.Instance.GetCommandSets(CommandSetPlacement.Bucket, CommandSetMode.UserInterface, bucket.Repo.RepoCollection, bucket.Repo, bucket);
                    nodeContext = GemNodeContext.GetNodeContext(bucket: bucket);
                }
                else if (selectedItem is Enlistment enlistment)
                {
                    commandSets = Gem.Instance.GetCommandSets(CommandSetPlacement.Enlistment, CommandSetMode.UserInterface, enlistment.Bucket.Repo.RepoCollection, enlistment.Bucket.Repo, enlistment.Bucket, enlistment);
                    nodeContext = GemNodeContext.GetNodeContext(enlistment: enlistment);
                }
            }

            // Attach command sets to the menu
            if (commandSets != null && nodeContext != null)
            {
                foreach (var gemCommandSet in commandSets)
                {
                    var menuGemCommandSet = new MenuItem()
                    {
                        Header = gemCommandSet.RightClickText
                    };
                    menuGemCommandSet.Click += async (s, e) =>
                    {
                        await this.ClearCommandWindow().ConfigureAwait(false);
                        await this.RunCommandSet(gemCommandSet, nodeContext).ConfigureAwait(false);
                    };
                    menu.Items.Add(menuGemCommandSet);
                }
            }

            e.Handled = true;
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
                // Execute the command and if the command was not successful then end now, returning false for the command set
                if (!await command.Execute(nodeContext, this).ConfigureAwait(false))
                {
                    return false;
                }
            }
            return true;
        }

        public async Task<bool> RunProgram(string? programPath, string? arguments, Dictionary<string, string>? tokens, string? workingDirectory = null)
        {
            return await ProgramHelper.RunProgram(
                programPath: programPath,
                arguments: arguments,
                tokens: tokens,
                useShellExecute: false,
                openNewWindow: false,
                workingDirectory: workingDirectory,
                metaOutputHandler: async (s) =>
                {
                    await txtCommandPrompt.AppendLine(s, Brushes.White).ConfigureAwait(false);
                    await txtCommandPrompt.Dispatcher.BeginInvoke(() =>
                    {
                        txtCommandPrompt.ScrollToEnd();
                    });
                },
                outputHandler: async (s) =>
                {
                    await txtCommandPrompt.AppendLine(s, Brushes.LightGray).ConfigureAwait(false);
                    await txtCommandPrompt.Dispatcher.BeginInvoke(() =>
                    {
                        txtCommandPrompt.ScrollToEnd();
                    });
                },
                errorHandler: async (s) =>
                {
                    await txtCommandPrompt.AppendLine(s, Brushes.DarkGray).ConfigureAwait(false);
                    await txtCommandPrompt.Dispatcher.BeginInvoke(() =>
                    {
                        txtCommandPrompt.ScrollToEnd();
                    });
                }
                ).ConfigureAwait(false);
        }
    }
}
