using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Extensions;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace GitEnlistmentManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Gem gem = new();

        public MainWindow()
        {
            InitializeComponent();
            if(!this.ReloadTreeview())
            {
                this.Close();
            }

            treeRepos.PreviewMouseRightButtonDown += TreeRepos_PreviewMouseRightButtonDown;
        }

        private bool ReloadTreeview()
        {
            treeRepos.ItemsSource = null;
            var gem = GetGem();
            if (gem == null) 
            {
                return false;
            }

            this.gem = gem;
            treeRepos.ItemsSource = gem.RepoCollections;
            return true;
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
                }

                if (selectedItem is Repo repo)
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

                    if (repo.GetDirectoryInfo() != null)
                    { 
                        var menuAddNewBucket = new MenuItem()
                        {
                            Header = "Add New Bucket"
                        };
                        menuAddNewBucket.Click += (s, e) =>
                        {
                            var bucketSettingsEditor = new BucketSettings(new Bucket(repo));
                            bucketSettingsEditor.ShowDialog();
                            // After the editor closes, reload the UI so we pick up any changes made
                            this.ReloadTreeview();
                        };

                        menu.Items.Add(menuAddNewBucket);
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

                    var menuPullRequest = new MenuItem()
                    {
                        Header = "Pull Request"
                    };
                    menuPullRequest.Click += (s, e) =>
                    {
                        enlistment.PullRequest();
                    };
                    menu.Items.Add(menuPullRequest);
                }

                e.Handled = true;
            }
        }

        private Gem? GetGem()
        {
            var gem = new Gem();

            // If the Gem settings are not yet set, these need to be set first before
            // doing anything else
            while (!gem.ReadLocalAppData() || string.IsNullOrWhiteSpace(gem.LocalAppData.ReposFolder))
            {
                var gemSettingsEditor = new GemSettings(gem);
                var result = gemSettingsEditor.ShowDialog();
                if (result != null && !result.Value)
                {
                    return null;
                }
            }

            // For each Gem metadata folder we have, look through it for repo definitions
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

            var gemFolder = new DirectoryInfo(gem.LocalAppData.ReposFolder);
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
            return gem;
        }

        public async Task ClearCommandWindow()
        {
            await txtCommandPrompt.Clear().ConfigureAwait(false);
        }

        public async Task<bool> RunCommand(string programPath, string arguments, string? workingFolder = null)
        {
            if (workingFolder != null)
            {
                await txtCommandPrompt.AppendLine($"cd \"{workingFolder}\"", Brushes.White).ConfigureAwait(false);
            }
            await txtCommandPrompt.AppendLine($"\"{programPath}\" {arguments}", Brushes.White).ConfigureAwait(false);

            using Process process = new();
            process.StartInfo = new()
            {
                FileName = programPath,
                Arguments = arguments,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = workingFolder
            };

            process.OutputDataReceived += new DataReceivedEventHandler(RunCommand_Output);
            process.ErrorDataReceived += new DataReceivedEventHandler(RunCommand_Error);
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
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
