using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Extensions;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

// Make it so create new repo only appears when right clicking a blank spot

// Split out data that can be serialized in the DTO classes. This will make it easier to read/write their data to disk.

// Make it so creating a new repo displays a form that allows you to type in the repo information.

// Make it so you can edit an existing repo information with the same repo settings form.

// Don't allow renames of the repo folder, this would mess up the parent repo mapping for 2nd, 3rd etc level enlistments. Instead users will need to create a new repo folder for now.

// Make a dialog that lets you edit the top level GEM data, such as the folder where all repos are stored
// Make it so Gem data is stored in localappdata for this app

// Make sure that users are not allowed to create or edit any repos until they have first
// set the Gem metadata repos folder

// Add a right click menu to edit the main Gem settings

// Add links to every child Dto so it can trace back up to its parent when needed.
// make sure all links are loaded during load and when adding children.

// Show a dialog to add a new bucket and create the bucket folder

// TODO: show a dialog to create a new enlistment. This should run the clone commands to set up the enlistment.
// TODO: the program should name the folders so they appear in file explorer the same way the inherit .. maybe.


// TODO: Create named snippets, a snippet would be a set of commands that run to accomplish a specific purpose
// TODO: Snippets should be set up at LCD for what the program needs.
// TODO: Performing various actions in the program is really running the right set of snippets.
// TODO: The snippets should be saved in an appdata folder so users could modify them if needed.

// TODO: make it so the treeview remembers the open folders when reloading the UI

// TODO: Make it so you can have different repo types GitHub vs TFS, each type would have
// TODO: different settings for the PR url format to use

// TODO: design how setting default email and name will work, will it be required for each repo?
// TODO: if so it would be more things to add in the repo settings dialog

// TODO: Add a right click option to do a PR for an enlistment

// TODO: add a right click option to archive an enlistment

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
            this.ReloadTreeview();
            treeRepos.PreviewMouseRightButtonDown += TreeRepos_PreviewMouseRightButtonDown;
        }

        private void ReloadTreeview()
        {
            treeRepos.ItemsSource = null;
            this.gem = GetGem();
            treeRepos.ItemsSource = gem.Repos;
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


                var menuAddNewRepo = new MenuItem
                {
                    Header = "Add New Repo"
                };
                menuAddNewRepo.Click += (s, e) =>
                {
                    var repoSettingsEditor = new RepoSettings(new Repo(this.gem), isNew: true);
                    repoSettingsEditor.ShowDialog();
                    // After the editor closes, reload the UI so we pick up any changes made
                    this.ReloadTreeview();
                };
                menu.Items.Add(menuAddNewRepo);
            }
            else
            {
                treeViewItem.Focus();
                treeViewItem.IsSelected = true;

                // Figure out what kind of treeviewItem we're clicking on (repo/bucket/enlistment) and create different menus for each type
                var selectedItem = treeViewItem.DataContext;
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
                if (selectedItem is Bucket)
                {
                    var menuAddNewEnlistment = new MenuItem() // TODO: all enlistment options should only be there if all the settings needed to create an enlistment are set up properly
                    {
                        Header = "Add New Enlistment"
                    };
                    menu.Items.Add(menuAddNewEnlistment);
                }
                if (selectedItem is Enlistment)
                {
                    var menuEditEnlistmentSettings = new MenuItem()
                    {
                        Header = "Edit Enlistment Settings"
                    };
                    menu.Items.Add(menuEditEnlistmentSettings);

                    var menuAddNewEnlistmentAbove = new MenuItem()
                    {
                        Header = "Add New Enlistment Above"
                    };
                    menu.Items.Add(menuAddNewEnlistmentAbove);
                }

                e.Handled = true;
            }
        }

        private Gem GetGem()
        {
            var gem = new Gem();

            // If the Gem settings are not yet set, these need to be set first before
            // doing anything else
            while (!gem.ReadMetadata() || string.IsNullOrWhiteSpace(gem.Metadata.ReposFolder))
            {
                var gemSettingsEditor = new GemSettings(gem);
                gemSettingsEditor.ShowDialog();
            }

            var gemFolder = new DirectoryInfo(gem.Metadata.ReposFolder);
            //This gets the repos, buckets, and enlistments from the configured repos folder
            foreach (var repoFolder in gemFolder.GetDirectories())
            {
                var repo = new Repo(gem)
                {
                    Name = repoFolder.Name
                };

                // Best attempt to load metadata, but if it fails then still add it to the UI so the user can re-create it.
                // This call will messagebox about ones that are bad so there is also the option to just shut the program off
                // and fix the json by hand too.
                // If the metadata file doesn't exist at all, then we just create silently create up a new object.
                repo.ReadMetadata();
                gem.Repos.Add(repo);

                foreach(var bucketFolder in repoFolder.GetDirectories())
                {
                    var bucket = new Bucket(repo);
                    bucket.Name = bucketFolder.Name;
                    repo.Buckets.Add(bucket);
                    
                    foreach(var enlistmentFolder in bucketFolder.GetDirectories())
                    {
                        var enlistment = new Enlistment(bucket)
                        {
                            Name = enlistmentFolder.Name
                        };
                        bucket.Enlistments.Add(enlistment);
                    }
                }
            }
            return gem;
        }



        //private async void btnRunCommand_Click(object sender, RoutedEventArgs e)
        //{
        //    await RunCommand(
        //        programPath: "C:\\Program Files\\Git\\cmd\\git.exe",
        //        arguments: "--help"
        //        ).ConfigureAwait(true);
        //    txtCommandPrompt.ScrollToEnd();
        //}


        private async Task RunCommand(string programPath, string arguments)
        {
            await txtCommandPrompt.FormatLinesWithoutExtraLineReturns().ConfigureAwait(false);
            await txtCommandPrompt.AppendLine($">{programPath} {arguments}", Brushes.White).ConfigureAwait(false);

            using (Process process = new Process())
            {
                process.StartInfo.FileName = programPath;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;

                process.OutputDataReceived += new DataReceivedEventHandler(RunCommand_Output);
                process.Start();
                process.BeginOutputReadLine();
                process.WaitForExit();
            }
        }

        private async void RunCommand_Output(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                await txtCommandPrompt.AppendLine(e.Data, Brushes.Gray).ConfigureAwait(false);
            }
        }
    }
}
