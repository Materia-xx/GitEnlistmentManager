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
                await this.Dispatcher.InvokeAsync(() =>
                {
                    try
                    {
                        if (Gem.LoadingErrors.Any())
                        {
                            MessageBox.Show(string.Join(Environment.NewLine, Gem.LoadingErrors));
                        }
                    }
                    catch { }
                    finally
                    {
                        this.Close();
                    }
                });
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

        /// <summary>
        /// Expands the tree view to the resolved nodes for the given working directory.
        /// Sets IsExpanded=true on each ancestor node from RepoCollection down to whatever
        /// level the path resolves to. Marshals to the UI dispatcher; safe to call from any
        /// thread. Silently no-ops if the path can't be resolved.
        /// </summary>
        public async Task ExpandToWorkingDirectory(string? workingDirectory)
        {
            if (string.IsNullOrWhiteSpace(workingDirectory) || Gem.Instance.LocalAppData.ReposDirectory == null)
            {
                return;
            }
            if (!workingDirectory.StartsWith(Gem.Instance.LocalAppData.ReposDirectory, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var relative = workingDirectory.Substring(Gem.Instance.LocalAppData.ReposDirectory.Length).Trim('\\');
            var parts = relative.Split('\\');

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                if (parts.Length == 0 || string.IsNullOrWhiteSpace(parts[0]))
                {
                    return;
                }

                var rc = Gem.Instance.RepoCollections.FirstOrDefault(c =>
                    c.GemName != null && c.GemName.Equals(parts[0], StringComparison.OrdinalIgnoreCase));
                if (rc == null)
                {
                    return;
                }
                rc.IsExpanded = true;

                if (parts.Length < 2 || string.IsNullOrWhiteSpace(parts[1]))
                {
                    return;
                }
                var repo = rc.Repos.FirstOrDefault(r =>
                    r.Metadata.ShortName != null && r.Metadata.ShortName.Equals(parts[1], StringComparison.OrdinalIgnoreCase));
                if (repo == null)
                {
                    return;
                }
                repo.IsExpanded = true;

                if (parts.Length < 3 || string.IsNullOrWhiteSpace(parts[2]))
                {
                    return;
                }
                var tb = repo.TargetBranches.FirstOrDefault(t =>
                    t.BranchDefinition.FolderName != null && t.BranchDefinition.FolderName.Equals(parts[2], StringComparison.OrdinalIgnoreCase));
                if (tb == null)
                {
                    return;
                }
                tb.IsExpanded = true;

                if (parts.Length < 4 || string.IsNullOrWhiteSpace(parts[3]))
                {
                    return;
                }
                var bucket = tb.Buckets.FirstOrDefault(b =>
                    b.GemName != null && b.GemName.Equals(parts[3], StringComparison.OrdinalIgnoreCase));
                if (bucket == null)
                {
                    return;
                }
                bucket.IsExpanded = true;

                if (parts.Length < 5 || string.IsNullOrWhiteSpace(parts[4]))
                {
                    return;
                }
                var enlistment = bucket.Enlistments.FirstOrDefault(e =>
                    e.GemName != null && e.GemName.Equals(parts[4], StringComparison.OrdinalIgnoreCase));
                if (enlistment == null)
                {
                    return;
                }
                enlistment.IsExpanded = true;
            });
        }

        public async Task<CommandDispatchResult> ProcessCSCommand(GemCSCommand command)
        {
            var result = new CommandDispatchResult();
            switch (command.CommandType)
            {
                case GemCSCommandType.InterpretCommandLine:
                    // There has to be at least 1 argument coming in
                    if (command.CommandArgs == null || command.CommandArgs.Length == 0)
                    {
                        result.AddError("No command verb was provided.");
                        return result;
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
                        result.AddError("Working directory is required for running a client/server command");
                        return result;
                    }
                    if (Gem.Instance.LocalAppData.ReposDirectory == null)
                    {
                        result.AddError("The LocalAppData ReposDirectory is not loaded or set properly.");
                        return result;
                    }
                    if (!command.WorkingDirectory.StartsWith(Gem.Instance.LocalAppData.ReposDirectory, StringComparison.OrdinalIgnoreCase))
                    {
                        result.AddError("The directory Gem starts in should be within the ReposDirectory.");
                        return result;
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
                    TargetBranch? targetBranch = null;
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
                        targetBranch = repo.TargetBranches.FirstOrDefault(tb =>
                            tb.BranchDefinition.FolderName != null &&
                            tb.BranchDefinition.FolderName.Equals(workingDirParts[2], StringComparison.OrdinalIgnoreCase));
                    }
                    // Path: [0]=RepoCollection, [1]=Repo, [2]=FolderName, [3]=Bucket, [4]=Enlistment
                    if (targetBranch != null && workingDirParts.Length > 3 && !string.IsNullOrWhiteSpace(workingDirParts[3]))
                    {
                        bucket = targetBranch.Buckets.FirstOrDefault(b => b.GemName != null && b.GemName.Equals(workingDirParts[3], StringComparison.OrdinalIgnoreCase));
                    }
                    if (bucket != null && workingDirParts.Length > 4 && !string.IsNullOrWhiteSpace(workingDirParts[4]))
                    {
                        enlistment = bucket.Enlistments.FirstOrDefault(e => e.GemName != null && e.GemName.Equals(workingDirParts[4], StringComparison.OrdinalIgnoreCase));
                    }

                    var nodeContext = new GemNodeContext()
                    {
                        RepoCollection = repoCollection,
                        Repo = repo,
                        TargetBranch = targetBranch,
                        Bucket = bucket,
                        Enlistment = enlistment
                    };

                    // We always have to have a repo collection
                    if (repoCollection == null)
                    {
                        result.AddError("RepoCollection was not set properly");
                        return result;
                    }

                    // All command sets that are available for this placement
                    var commandSets = Gem.Instance.GetCommandSets(nodeContext.GetPlacement(), CommandSetMode.CommandPrompt, repoCollection, repo, bucket, enlistment);

                    // Grab the last one that matches the verb being specified
                    var commandSet = commandSets.LastOrDefault(cs => cs.Verb != null && cs.Verb.Equals(verb, StringComparison.OrdinalIgnoreCase));

                    // If no command set with that verb was found then write out something in the UI
                    if (commandSet == null)
                    {
                        var message = $"No command sets with verb '{verb}' were found.";
                        result.AddError(message);
                        await this.AppendCommandLine(message, Brushes.LightSalmon).ConfigureAwait(false);
                    }
                    else
                    {
                        // The command sets loaded in Gem.CommandSets are never actually ran, they are just there to provide the verb for command line parsing and the right click menus
                        // We always run a freshly created command set each time. This is so that any state changes in the commands are starting fresh each time.
                        (commandSet, var loadingErrors) = CommandSet.ReadCommandSet(commandSet?.LoadedFromPath ?? string.Empty);
                        if (commandSet == null)
                        {
                            result.AddError(loadingErrors);
                            return result;
                        }

                        // Defense-in-depth: if this dispatch originated from MCP and the
                        // command set resolved at this placement is hidden from MCP, refuse
                        // to run it. RunCommandTool's pre-flight only confirms that some
                        // command set with the verb is exposed; placement-based resolution
                        // here could in principle land on a hidden variant if a verb is
                        // shared across placements with mixed ExposeToMcp values.
                        if (command.AutoExpandTreeView && !commandSet.ExposeToMcp)
                        {
                            result.AddError($"Command '{verb}' resolved to a command set that is not exposed to MCP at the current placement.");
                            return result;
                        }

                        foreach (var commandSetCommand in commandSet.Commands)
                        {
                            // Parse args first, this allows context to be initially set by command line args
                            commandSetCommand.ParseArgs(remainingArgsStack);

                            // If args don't set context, then fall back to the current node context
                            commandSetCommand.NodeContext.SetIfNotNullFrom(nodeContext);
                        }

                        // Auto-expand BEFORE running so the user sees where the AI is operating
                        // even for long-running commands. The after-pass below re-expands in
                        // case the command set wiped the tree via ReloadTreeview.
                        if (command.AutoExpandTreeView)
                        {
                            await ExpandToWorkingDirectory(command.WorkingDirectory).ConfigureAwait(false);
                        }

                        // Capture any UiMessages.ShowError calls raised by commands or the
                        // extension methods they invoke, so MCP callers receive them in the
                        // dispatch result instead of seeing modal dialogs popped on the GUI.
                        // CLI flows running concurrently install their own sink and are
                        // unaffected. Info messages flow only into the MCP response (they
                        // are no-ops when no sink is installed).
                        bool ranOk;
                        using (UiMessages.CaptureErrors(result.Errors))
                        using (UiMessages.CaptureInfo(result.InfoMessages))
                        {
                            ranOk = await commandSet.RunCommandSet(
                                nodeContext: nodeContext
                                ).ConfigureAwait(false);
                        }

                        // RunCommandSet returns false when a Command.Execute() returns false.
                        // Some commands return false silently (without calling
                        // UiMessages.ShowError); surface a generic dispatch-level error so
                        // MCP callers don't see Success when the command actually failed.
                        if (!ranOk && result.Errors.Count == 0)
                        {
                            result.AddError($"Command set '{verb}' reported failure but did not surface a specific error message.");
                        }

                        // After-pass: if the command set ended with ReloadTreeview the data
                        // objects were rebuilt from disk and IsExpanded was reset to false on
                        // every node. Re-expand to the working directory so the newly-created
                        // child nodes (createbucket, createenlistment, etc.) are visible.
                        if (command.AutoExpandTreeView)
                        {
                            await ExpandToWorkingDirectory(command.WorkingDirectory).ConfigureAwait(false);
                        }
                    }
                    break;
            }

            return result;
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
                else if (selectedItem is TargetBranch targetBranch)
                {
                    commandSets = Gem.Instance.GetCommandSets(CommandSetPlacement.TargetBranch, CommandSetMode.UserInterface, targetBranch.Repo.RepoCollection, targetBranch.Repo);
                    nodeContext = GemNodeContext.GetNodeContext(targetBranch: targetBranch);
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

                        // The command sets loaded in Gem.CommandSets are never actually ran, they are just there to provide the verb for command line parsing and the right click menus
                        // We always run a freshly created command set each time. This is so that any state changes in the commands are starting fresh each time.
                        (var commandSet, var loadingErrors) = CommandSet.ReadCommandSet(gemCommandSet?.LoadedFromPath ?? string.Empty);
                        if (commandSet == null)
                        {
                            MessageBox.Show(loadingErrors);
                            return;
                        }

                        await commandSet.RunCommandSet(nodeContext).ConfigureAwait(false);
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
