using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Extensions;
using GitEnlistmentManager.Globals;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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

            if (int.TryParse(this.txtMcpPort.Text, out int resultMcpPort))
            {
                this.gem.LocalAppData.McpPort = resultMcpPort;
            }
            else
            {
                MessageBox.Show("Unable to convert the MCP port into a number!");
                return false;
            }

            this.gem.LocalAppData.McpEnabled = this.chkMcpEnabled.IsChecked == true;

            // Save disabled MCP tools from checkboxes
            this.gem.LocalAppData.DisabledMcpTools.Clear();
            foreach (var child in this.mcpToolCheckboxes.Children)
            {
                if (child is CheckBox checkBox && checkBox.Tag is string toolName && checkBox.IsChecked != true)
                {
                    this.gem.LocalAppData.DisabledMcpTools.Add(toolName);
                }
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
            this.chkMcpEnabled.IsChecked = this.gem.LocalAppData.McpEnabled;
            this.txtMcpPort.Text = this.gem.LocalAppData.McpPort.ToString();

            this.txtRepoCollectionDefinitionDirectories.Text = string.Join(Environment.NewLine, this.gem.LocalAppData.RepoCollectionDefinitionDirectories);
            this.txtCommandSetDirectories.Text = string.Join(Environment.NewLine, this.gem.LocalAppData.CommandSetDirectories);

            this.txtCompareProgram.Text = this.gem.LocalAppData.CompareProgram;
            this.txtCompareArguments.Text = this.gem.LocalAppData.CompareArguments;

            // Populate MCP tool checkboxes
            this.mcpToolCheckboxes.Children.Clear();
            var mcpServer = Global.Instance.McpServer;
            if (mcpServer != null)
            {
                // Add checkboxes for registered MCP tools
                foreach (var toolName in mcpServer.GetToolNames())
                {
                    // Skip run_command itself — individual verbs are shown instead
                    if (toolName == "run_command")
                    {
                        continue;
                    }

                    var checkBox = new CheckBox
                    {
                        Content = toolName,
                        Tag = toolName,
                        IsChecked = !this.gem.LocalAppData.DisabledMcpTools.Contains(toolName),
                        Margin = new Thickness(5, 2, 5, 2),
                        ToolTip = mcpServer.GetToolDescription(toolName)
                    };
                    this.mcpToolCheckboxes.Children.Add(checkBox);
                }

                // Add checkboxes for each command verb available through run_command
                var seenVerbs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var commandSet in this.gem.CommandSets)
                {
                    if (string.IsNullOrWhiteSpace(commandSet.Verb))
                    {
                        continue;
                    }

                    if (!seenVerbs.Add(commandSet.Verb))
                    {
                        continue;
                    }

                    var tagValue = $"run_command:{commandSet.Verb}";
                    var tooltip = !string.IsNullOrWhiteSpace(commandSet.Documentation)
                        ? commandSet.Documentation
                        : commandSet.Verb;
                    var checkBox = new CheckBox
                    {
                        Content = $"cmd: {commandSet.Verb}",
                        Tag = tagValue,
                        IsChecked = !this.gem.LocalAppData.DisabledMcpTools.Contains(tagValue),
                        Margin = new Thickness(5, 2, 5, 2),
                        ToolTip = tooltip
                    };
                    this.mcpToolCheckboxes.Children.Add(checkBox);
                }
            }
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
