using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Extensions;
using GitEnlistmentManager.Globals;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
            if (!FormToDto())
            {
                return;
            }

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

        private bool FormToDto()
        {
            this.gem.LocalAppData.ReposFolder = this.txtReposFolder.Text;
            this.gem.LocalAppData.GitExePath = this.txtGitExePath.Text;

            if (int.TryParse(this.txtArchiveSlots.Text, out int resultArchiveSlots))
            {
                this.gem.LocalAppData.ArchiveSlots = resultArchiveSlots;
            }
            else
            {
                MessageBox.Show("Unable to convert the number of archive slots into a number!");
                return false;
            }

            if (int.TryParse(this.txtEnlistmentIncrement.Text, out int resultEnlistmentIncrement))
            {
                this.gem.LocalAppData.EnlistmentIncrement = resultEnlistmentIncrement;
            }
            else
            {
                MessageBox.Show("Unable to convert the enlistment increment amount into a number!");
                return false;
            }

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

            this.gem.LocalAppData.CompareProgram = this.txtCompareProgram.Text;
            this.gem.LocalAppData.CompareArguments = this.txtCompareArguments.Text;

            return true;
        }

        private void DtoToForm()
        {
            this.txtReposFolder.Text = this.gem.LocalAppData.ReposFolder;
            this.txtGitExePath.Text = this.gem.LocalAppData.GitExePath;
            this.txtArchiveSlots.Text = this.gem.LocalAppData.ArchiveSlots.ToString();
            this.txtEnlistmentIncrement.Text = this.gem.LocalAppData.EnlistmentIncrement.ToString();

            this.txtRepoCollectionDefinitionFolders.Text = string.Join(Environment.NewLine, this.gem.LocalAppData.RepoCollectionDefinitionFolders);
            this.txtCommandSetFolders.Text = string.Join(Environment.NewLine, this.gem.LocalAppData.CommandSetFolders);

            this.txtCompareProgram.Text = this.gem.LocalAppData.CompareProgram;
            this.txtCompareArguments.Text = this.gem.LocalAppData.CompareArguments;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.txtReposFolder.Focus();
        }

        private async void btnOpenReposFolder_Click(object sender, RoutedEventArgs e)
        {
            await ProgramHelper.OpenFolder(txtReposFolder.Text).ConfigureAwait(false);
        }       

        private async void btnOpenCommandSetFolders_Click(object sender, RoutedEventArgs e)
        {
            var paths = txtCommandSetFolders.Text.Split(StringExtensions.LineReturnCharArray,StringSplitOptions.RemoveEmptyEntries);

            foreach (var path in paths)
            {
                await ProgramHelper.OpenFolder(path).ConfigureAwait(false);
            }
            await ProgramHelper.OpenFolder(this.gem.GetDefaultCommandSetsFolder().FullName).ConfigureAwait(false);
        }

        private async void btnOpenRepoCollectionDefinitionFolders_Click(object sender, RoutedEventArgs e)
        {
            var paths = txtRepoCollectionDefinitionFolders.Text.Split(StringExtensions.LineReturnCharArray, StringSplitOptions.RemoveEmptyEntries);

            foreach (var path in paths)
            {
                await ProgramHelper.OpenFolder(path).ConfigureAwait(false);
            }
        }
    }
}
