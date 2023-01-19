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

            if (!Path.Exists(this.gem.Metadata.GitExePath))
            {
                MessageBox.Show($"Git path not found: {this.gem.Metadata.GitExePath}");
                return;
            }

            if (string.IsNullOrWhiteSpace(this.gem.Metadata.ReposFolder))
            {
                MessageBox.Show($"Please specify the repos folder");
                return;
            }

            // Write the metadata.json.
            if (this.gem.WriteMetadata())
            {
                this.DialogResult = true;
                this.Close();
            }
        }

        private void FormToDto()
        {
            this.gem.Metadata.ReposFolder = this.txtReposFolder.Text;
            this.gem.Metadata.GitExePath = this.txtGitExePath.Text;

            var metadataFolders = this.txtMetadataFolders.Text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            this.gem.Metadata.MetadataFolders.Clear();
            foreach (var metadataFolder in metadataFolders)
            {
                if (!Directory.Exists(metadataFolder))
                {
                    MessageBox.Show($"{metadataFolder} doesn't exist, skipping.");
                    continue;
                }
                this.gem.Metadata.MetadataFolders.Add(metadataFolder);
            }
        }

        private void DtoToForm()
        {
            this.txtReposFolder.Text = this.gem.Metadata.ReposFolder;
            this.txtGitExePath.Text = this.gem.Metadata.GitExePath;
            this.txtMetadataFolders.Text = string.Join(Environment.NewLine, this.gem.Metadata.MetadataFolders);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.txtReposFolder.Focus();
        }
    }
}
