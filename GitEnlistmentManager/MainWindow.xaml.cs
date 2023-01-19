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

// Z, A: Make it so create new repo only appears when right clicking a blank spot

// Z, A: Split out data that can be serialized in the DTO classes. This will make it easier to read/write their data to disk.

// Z, A: Make it so creating a new repo displays a form that allows you to type in the repo information.

// Z, A: Make it so you can edit an existing repo information with the same repo settings form.

// Z, A: Don't allow renames of the repo folder, this would mess up the parent repo mapping for 2nd, 3rd etc level enlistments. Instead users will need to create a new repo folder for now.

// Z, A: Make a dialog that lets you edit the top level GEM data, such as the folder where all repos are stored
// Z, A: Make it so Gem data is stored in appdata for this app

// Z, A: Make sure that users are not allowed to create or edit any repos until they have first
// Z, A: set the Gem metadata repos folder

// Z, A: Add a right click menu to edit the main Gem settings

// Z, A: Add links to every child Dto so it can trace back up to its parent when needed.
// Z, A: make sure all links are loaded during load and when adding children.

// Z, A: Show a dialog to add a new bucket and create the bucket folder

// Z, A: Update RunCommand so you can pass in the working folder

// Z, A: Update the RunCommand so it returns a true/false indicating if the command was successful

// Z, A: Update the RunCommand so it also shows things written to the error channel

// Z, A: Show a dialog to create a new enlistment. This should run the clone commands to set up the enlistment.

// Z, A: Add an option to the main Gem config to specify the path to git.exe
// Z, A: then update enlistment extensions or anywhere else that has the path hardcoded.

// Z, A: Wrap the exe being called in RunCommand in quotes, or at-least display it that way so that a copy/pasted command will still run properly.

// Z, A: Update the new enlistment command to take into account being a child of another enlistment in the same bucket.

// Z, A: Implement "Injecting" an enlistment. 

// Z, A: Clear the "cmd screen" when creating a new enlistment.

// Z, A: design how setting default email and name will work, will it be required for each repo?
// Z, A: if so it would be more things to add in the repo settings dialog
// Z, A: add email and name to the repo settings and set the values for these when creating each enlistment
// Z, A:   git config --local user.email "you@example.com"
// Z, A:   git config --local user.name "Your Name"

// Z  Split repo metadata.json or other metadata currently in the gem repos folder into a new location "gem metadata"
// Z The initial Gem Config will ask for 2 paths. 1. where to store repos and 2. where to store metadata
// Z This will allow the metadata to be copied to another computer and used there, but on the other computer it uses a different folder for repos.


// TODO: Add a right click option to do a PR for an enlistment

// TODO: most/all commands done through the tool should also be available as command line arguments
// TODO: one example is that someone should be able to type "gem pr" when then are in a command prompt in an enlistment folder and the tool will know how to open a PR from there.


// TODO: add a right click option to archive an enlistment


// TODO: make it so the treeview remembers the open folders when reloading the UI

// TODO: add the ability to run extra commands specific to a given repo.. these commands would only run if you were using that repo.

// TODO: Make it so you can have different repo types GitHub vs TFS, each type would have
// TODO: different settings for the PR url format to use

// TODO: split the gem metadata files apart from the gem folder so that they are in a completely different place.
// TODO: Make it so you can specify where this folder is in the gem settings
// TODO: the idea is that you should be able to back up the settings metadata files in onedrive or another git repo and then easily restore all repos onto another machine using GEM there.

// TODO: support renaming a repo folder through the program. This will need to go through and re-parent enlistments that are bound to a file path.

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
            treeRepos.ItemsSource = gem.MetadataFolders;
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

                if (selectedItem is MetadataFolder metadataFolder)
                {
                    var menuAddNewRepo = new MenuItem
                    {
                        Header = "Add New Repo"
                    };
                    menuAddNewRepo.Click += (s, e) =>
                    {
                        var repoSettingsEditor = new RepoSettings(new Repo(metadataFolder), isNew: true);
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
                }

                e.Handled = true;
            }
        }

        private Gem? GetGem()
        {
            var gem = new Gem();

            // If the Gem settings are not yet set, these need to be set first before
            // doing anything else
            while (!gem.ReadMetadata() || string.IsNullOrWhiteSpace(gem.Metadata.ReposFolder))
            {
                var gemSettingsEditor = new GemSettings(gem);
                var result = gemSettingsEditor.ShowDialog();
                if (result != null && !result.Value)
                {
                    return null;
                }
            }

            // For each Gem metadata folder we have, look through it for repo definitions
            foreach (var metadataFolderPath in gem.Metadata.MetadataFolders)
            {
                var metadataFolderInfo = new DirectoryInfo(metadataFolderPath);
                if (!metadataFolderInfo.Exists)
                {
                    MessageBox.Show($"Metadata folder {metadataFolderInfo.FullName} was not found");
                    continue;
                }

                var metadataFolder = new MetadataFolder(gem, metadataFolderPath)
                {
                    Name = metadataFolderInfo.Name
                };
                gem.MetadataFolders.Add(metadataFolder);

                // Loop through all the repo registrations that are present here
                var repoRegistrations = metadataFolderInfo.GetFiles("*.repojson", SearchOption.TopDirectoryOnly);
                foreach (var repoRegistration in repoRegistrations)
                {
                    var repoName = Path.GetFileNameWithoutExtension(repoRegistration.Name);
                    var repo = new Repo(metadataFolder)
                    {
                        Name = Path.GetFileNameWithoutExtension(repoRegistration.Name)
                    };

                    // Best attempt to load repo metadata, but if it fails then still add it to the UI so the user can re-create it.
                    repo.ReadMetadata(repoRegistration.FullName);
                    metadataFolder.Repos.Add(repo);
                }
            }

            var gemFolder = new DirectoryInfo(gem.Metadata.ReposFolder);
            foreach (var metadataFolder in gem.MetadataFolders)
            {
                foreach (var repo in metadataFolder.Repos)
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
