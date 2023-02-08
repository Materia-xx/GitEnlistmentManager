using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Extensions;
using GitEnlistmentManager.Globals;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GitEnlistmentManager
{
    /// <summary>
    /// Interaction logic for RepoSettings.xaml
    /// </summary>
    public partial class RepoSettings : Window
    {
        private readonly Repo repoSettings;

        public RepoSettings(Repo repo, bool isNew)
        {
            InitializeComponent();
            this.repoSettings = repo;

            // Load choices for combo boxes
            cboGitHostingPlatformName.Items.Clear();
            foreach (var platform in GitHostingPlatforms.Instance.Platforms)
            {
                cboGitHostingPlatformName.Items.Add(platform.Name);
            }

            // Load choices for "Pick defaults from" combo box
            cboChooseDefaultsFrom.Items.Clear();
            var otherRepoNames = new List<string>();
            var currentRepoCollectionName = this.repoSettings.RepoCollection.GemName;
            foreach (var repoCollection in this.repoSettings.RepoCollection.Gem.RepoCollections)
            {
                var repoNames = repoCollection.Repos.Where(r => r.GemName != null).Select(r => r.GemName).ToList();
                foreach (var repoName in repoNames)
                {
                    if (repoName != null)
                    {
                        if (currentRepoCollectionName == repoCollection.GemName && repoName == repoSettings.GemName)
                        {
                            continue;
                        }
                        otherRepoNames.Add($"{repoCollection.GemName}->{repoName}");
                    }
                }
            }
            foreach (var otherRepoName in otherRepoNames)
            {
                cboChooseDefaultsFrom.Items.Add(otherRepoName);
            }

            this.DtoToForm();
            this.txtName.IsEnabled = isNew;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            if (this.cboGitHostingPlatformName.SelectedValue == null)
            {
                MessageBox.Show("Please select a Git Hosting Platform");
                return;
            }

            if (string.IsNullOrWhiteSpace(this.txtShortName.Text))
            {
                MessageBox.Show("Short name must have a value");
                return;
            }

            // TODO: add validation for other fields too


            // Transfer data from form to DTO
            FormToDto();
            this.DialogResult = true;
            // Write the metadata.json. This also creates the folder if it doesn't exist
            if (this.repoSettings.WriteMetadata())
            {
                this.Close();
            }
        }

        private void FormToDto()
        {
            this.repoSettings.GemName = this.txtName.Text;
            this.repoSettings.Metadata.ShortName = this.txtShortName.Text;
            this.repoSettings.Metadata.CloneUrl = this.txtCloneUrl.Text;
            this.repoSettings.Metadata.BranchFrom = this.txtBranchFrom.Text;
            this.repoSettings.Metadata.BranchPrefix = this.txtBranchPrefix.Text;
            this.repoSettings.Metadata.UserName = this.txtUserName.Text;
            this.repoSettings.Metadata.UserEmail = this.txtUserEmail.Text;
            this.repoSettings.Metadata.GitHostingPlatformName = this.cboGitHostingPlatformName.SelectedValue.ToString();
        }

        private void DtoToForm()
        {
            this.txtName.Text = this.repoSettings.GemName;
            this.txtShortName.Text = this.repoSettings.Metadata.ShortName;
            this.txtCloneUrl.Text = this.repoSettings.Metadata.CloneUrl;
            this.txtBranchFrom.Text = this.repoSettings.Metadata.BranchFrom;
            this.txtBranchPrefix.Text = this.repoSettings.Metadata.BranchPrefix;
            this.txtUserName.Text = this.repoSettings.Metadata.UserName;
            this.txtUserEmail.Text = this.repoSettings.Metadata.UserEmail;
            this.cboGitHostingPlatformName.SelectedValue = this.repoSettings.Metadata.GitHostingPlatformName;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.txtName.Focus();
        }

        private void cboChooseDefaultsFrom_SelectedChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
            {
                return;
            }

            var chooseFromName = e.AddedItems[0]?.ToString();
            if (chooseFromName == null)
            {
                return;
            }

            var arrowIndex = chooseFromName.IndexOf("->");
            if (arrowIndex == -1)
            {
                return;
            }
            var repoCollectionName = chooseFromName.Substring(0, arrowIndex);
            var repoName = chooseFromName.Substring(arrowIndex + 2);

            var repoCollection = this.repoSettings.RepoCollection.Gem.RepoCollections.FirstOrDefault(rc => rc.GemName == repoCollectionName);
            if (repoCollection == null)
            {
                return;
            }

            var chooseFromRepo = repoCollection.Repos.FirstOrDefault(r => r.GemName != null && r.GemName == repoName);
            if (chooseFromRepo == null)
            {
                return;
            }

            this.txtBranchPrefix.Text = chooseFromRepo.Metadata.BranchPrefix;
            this.txtUserName.Text = chooseFromRepo.Metadata.UserName;
            this.txtUserEmail.Text = chooseFromRepo.Metadata.UserEmail;
        }
    }
}
