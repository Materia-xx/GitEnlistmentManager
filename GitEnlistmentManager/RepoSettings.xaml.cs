using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Extensions;
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

            // Transfer data from form to DTO
            FormToDto();

            // Write the metadata.json. This also creates the folder if it doesn't exist
            if (this.repoSettings.WriteMetadata())
            {
                this.Close();
            }
        }

        private void FormToDto()
        {
            this.repoSettings.Name = this.txtName.Text;
            this.repoSettings.Metadata.CloneUrl = this.txtCloneUrl.Text;
            this.repoSettings.Metadata.BranchFrom = this.txtBranchFrom.Text;
            this.repoSettings.Metadata.BranchPrefix = this.txtBranchPrefix.Text;
            this.repoSettings.Metadata.UserName = this.txtUserName.Text;
            this.repoSettings.Metadata.UserEmail = this.txtUserEmail.Text;
            this.repoSettings.Metadata.GitHostingPlatformName = this.cboGitHostingPlatformName.SelectedValue.ToString();
        }

        private void DtoToForm()
        {
            this.txtName.Text = this.repoSettings.Name;
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
    }
}
