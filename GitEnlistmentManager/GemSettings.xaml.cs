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

            if (string.IsNullOrWhiteSpace(this.gem.LocalAppData.ReposDirectory))
            {
                MessageBox.Show($"Please specify the repos directory");
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
            this.gem.LocalAppData.ReposDirectory = this.txtReposDirectory.Text;
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

            if (int.TryParse(this.txtServerPort.Text, out int resultServerPort))
            {
                this.gem.LocalAppData.ServerPort = resultServerPort;
            }
            else
            {
                MessageBox.Show("Unable to convert the server port into a number!");
                return false;
            }

            var repoCollectionDefinitionDirectories = this.txtRepoCollectionDefinitionDirectories.Text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            this.gem.LocalAppData.RepoCollectionDefinitionDirectories.Clear();
            foreach (var repoCollectionDefinitionDirectory in repoCollectionDefinitionDirectories)
            {
                if (!Directory.Exists(repoCollectionDefinitionDirectory))
                {
                    MessageBox.Show($"{repoCollectionDefinitionDirectory} doesn't exist, skipping.");
                    continue;
                }
                this.gem.LocalAppData.RepoCollectionDefinitionDirectories.Add(repoCollectionDefinitionDirectory);
            }

            var commandSetDirectories = this.txtCommandSetDirectories.Text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
            this.gem.LocalAppData.CommandSetDirectories.Clear();
            foreach (var commandSetDirectory in commandSetDirectories)
            {
                if (!Directory.Exists(commandSetDirectory))
                {
                    MessageBox.Show($"{commandSetDirectory} doesn't exist, skipping.");
                    continue;
                }
                this.gem.LocalAppData.CommandSetDirectories.Add(commandSetDirectory);
            }

            this.gem.LocalAppData.CompareProgram = this.txtCompareProgram.Text;
            this.gem.LocalAppData.CompareArguments = this.txtCompareArguments.Text;

            return true;
        }

        private void DtoToForm()
        {
            this.txtReposDirectory.Text = this.gem.LocalAppData.ReposDirectory;
            this.txtGitExePath.Text = this.gem.LocalAppData.GitExePath;
            this.txtArchiveSlots.Text = this.gem.LocalAppData.ArchiveSlots.ToString();
            this.txtEnlistmentIncrement.Text = this.gem.LocalAppData.EnlistmentIncrement.ToString();
            this.txtServerPort.Text = this.gem.LocalAppData.ServerPort.ToString();

            this.txtRepoCollectionDefinitionDirectories.Text = string.Join(Environment.NewLine, this.gem.LocalAppData.RepoCollectionDefinitionDirectories);
            this.txtCommandSetDirectories.Text = string.Join(Environment.NewLine, this.gem.LocalAppData.CommandSetDirectories);

            this.txtCompareProgram.Text = this.gem.LocalAppData.CompareProgram;
            this.txtCompareArguments.Text = this.gem.LocalAppData.CompareArguments;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.txtReposDirectory.Focus();
        }

        private async void btnOpenReposDirectory_Click(object sender, RoutedEventArgs e)
        {
            await ProgramHelper.OpenDirectory(txtReposDirectory.Text).ConfigureAwait(false);
        }

        private async void btnOpenCommandSetDirectories_Click(object sender, RoutedEventArgs e)
        {
            var paths = txtCommandSetDirectories.Text.Split(StringExtensions.LineReturnCharArray,StringSplitOptions.RemoveEmptyEntries);

            foreach (var path in paths)
            {
                await ProgramHelper.OpenDirectory(path).ConfigureAwait(false);
            }
            await ProgramHelper.OpenDirectory(this.gem.GetDefaultCommandSetsDirectory().FullName).ConfigureAwait(false);
        }

        private async void btnOpenRepoCollectionDefinitionDirectories_Click(object sender, RoutedEventArgs e)
        {
            var paths = txtRepoCollectionDefinitionDirectories.Text.Split(StringExtensions.LineReturnCharArray, StringSplitOptions.RemoveEmptyEntries);

            foreach (var path in paths)
            {
                await ProgramHelper.OpenDirectory(path).ConfigureAwait(false);
            }
        }
    }
}
