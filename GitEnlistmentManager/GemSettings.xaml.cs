using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Extensions;
using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace GitEnlistmentManager
{
    /// <summary>
    /// Interaction logic for GemSettings.xaml
    /// </summary>
    public partial class GemSettings : Window
    {
        private readonly Gem gem;

        public GemSettings(Gem gem)
        {
            InitializeComponent();
            this.gem = gem;
            this.DtoToForm();
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            // Transfer data from form to DTO
            FormToDto();

            if (!Path.Exists(this.gem.LocalAppData.GitExePath))
            {
                MessageBox.Show($"Git path not found: {this.gem.LocalAppData.GitExePath}");
                return;
            }

            if (string.IsNullOrWhiteSpace(this.gem.LocalAppData.ReposFolder))
            {
                MessageBox.Show($"Please specify the repos folder");
                return;
            }

            // Write the metadata.json.
            if (this.gem.WriteLocalAppData())
            {
                this.DialogResult = true;
                this.Close();
            }
        }

        private void FormToDto()
        {
            this.gem.LocalAppData.ReposFolder = this.txtReposFolder.Text;
            this.gem.LocalAppData.GitExePath = this.txtGitExePath.Text;

            var repoCollectionDefinitionFolders = this.txtRepoCollectionDefinitionFolders.Text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            this.gem.LocalAppData.RepoCollectionDefinitionFolders.Clear();
            foreach (var repoCollectionDefinitionFolder in repoCollectionDefinitionFolders)
            {
                if (!Directory.Exists(repoCollectionDefinitionFolder))
                {
                    MessageBox.Show($"{repoCollectionDefinitionFolder} doesn't exist, skipping.");
                    continue;
                }
                this.gem.LocalAppData.RepoCollectionDefinitionFolders.Add(repoCollectionDefinitionFolder);
            }

            var commandSetFolders = this.txtCommandSetFolders.Text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            this.gem.LocalAppData.CommandSetFolders.Clear();
            foreach (var commandSetFolder in commandSetFolders)
            {
                if (!Directory.Exists(commandSetFolder))
                {
                    MessageBox.Show($"{commandSetFolder} doesn't exist, skipping.");
                    continue;
                }
                this.gem.LocalAppData.CommandSetFolders.Add(commandSetFolder);
            }
        }

        private void DtoToForm()
        {
            this.txtReposFolder.Text = this.gem.LocalAppData.ReposFolder;
            this.txtGitExePath.Text = this.gem.LocalAppData.GitExePath;
            this.txtRepoCollectionDefinitionFolders.Text = string.Join(Environment.NewLine, this.gem.LocalAppData.RepoCollectionDefinitionFolders);
            this.txtCommandSetFolders.Text = string.Join(Environment.NewLine, this.gem.LocalAppData.CommandSetFolders);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.txtReposFolder.Focus();
        }
    }
}
