using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Extensions;
using System.Windows;

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
            this.DtoToForm();
            this.txtName.IsEnabled = isNew;
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
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
        }

        private void DtoToForm()
        {
            this.txtName.Text = this.repoSettings.Name;
            this.txtCloneUrl.Text = this.repoSettings.Metadata.CloneUrl;
            this.txtBranchFrom.Text = this.repoSettings.Metadata.BranchFrom;
        }
    }
}
