using GitEnlistmentManager.Commands;
using GitEnlistmentManager.CommandSets;
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
using System.Windows.Media;

namespace GitEnlistmentManager
{
    /// <summary>
    /// Interaction logic for RemoteBranches.xaml
    /// </summary>
    public partial class RemoteBranches : Window
    {
        private static readonly string refsHeads = "refs/heads/";

        private Repo repo;
        private DirectoryInfo? trackingRepoDirectory;

        public RemoteBranches(Repo repo)
        {
            InitializeComponent();
            this.repo = repo;

            // The default remote branch filter
            txtBranchPrefixFilter.Text = $"{refsHeads}{this.repo.Metadata.BranchPrefix}";
        }

        private async Task<bool> RefreshTrackingRepo()
        {
            var repoDirectory = this.repo.GetDirectoryInfo()?.FullName;
            if (repoDirectory == null)
            {
                return false;
            }
            this.trackingRepoDirectory = new DirectoryInfo(Path.Combine(repoDirectory, "archive", "gemref"));

            if (this.trackingRepoDirectory.Exists)
            {
                return await ProgramHelper.RunProgram(
                    programPath: Gem.Instance.LocalAppData.GitExePath,
                    arguments: $"fetch --all",
                    tokens: null, // There are no tokens in the above programPath/arguments
                    useShellExecute: false,
                    openNewWindow: false,
                    workingDirectory: this.trackingRepoDirectory.FullName
                    ).ConfigureAwait(false);
            }
            else
            {
                return await ProgramHelper.RunProgram(
                    programPath: Gem.Instance.LocalAppData.GitExePath,
                    arguments: $"clone --no-checkout {this.repo.Metadata.CloneUrl} \"{this.trackingRepoDirectory.FullName}\"",
                    tokens: null, // There are no tokens in the above programPath/arguments
                    useShellExecute: false,
                    openNewWindow: false,
                    workingDirectory: repoDirectory
                    ).ConfigureAwait(false);
            }
        }

        private async Task RefreshRemoteBranches()
        {
            if (!await this.RefreshTrackingRepo() || this.trackingRepoDirectory == null)
            {
                return;
            }
            if (this.repo == null || this.repo.Metadata.CloneUrl == null)
            {
                return;
            }

            var remoteBranchDtos = new List<RemoteBranchDto>();

            // If the branch prefix doesn't start with refs/heads then add it as a prefix
            var branchFilter = string.Empty;
            await this.Dispatcher.InvokeAsync(() =>
            {
                branchFilter = txtBranchPrefixFilter.Text?.TrimStart('/');
            });

            if (string.IsNullOrWhiteSpace(branchFilter))
            {
                branchFilter = $"{refsHeads}{this.repo.Metadata.BranchPrefix}";
            }
            if (!branchFilter.StartsWith(refsHeads))
            {
                branchFilter = $"{refsHeads}{branchFilter}";
            }

            // Run a git command to list the remote branches
            var remoteBranches = new List<string>();
            await ProgramHelper.RunProgram(
                programPath: this.repo.RepoCollection?.Gem.LocalAppData.GitExePath,
                arguments: $"ls-remote --heads {this.repo.Metadata.CloneUrl}",
                tokens: null,
                openNewWindow: false,
                useShellExecute: false,
                workingDirectory: null,
                outputHandler: (s) =>
                {
                    // Each branch listed will be a commit and then the branch which always starts with refs/heads
                    // 0e6ad61290228d5096c985563db5e81d57b0d4b6        refs/heads/user/Materia/Starspark/BookmarkManager/testing/010000.one
                    var refsHeadsPos = s.IndexOf(refsHeads);
                    if (refsHeadsPos > -1)
                    {
                        var branch = s.Substring(refsHeadsPos);
                        remoteBranches.Add(branch);
                    }
                    return Task.CompletedTask;
                }).ConfigureAwait(false);

            // Pick out only branches that match the prefix
            var matchingBranches = remoteBranches.Where(b => b.StartsWith(branchFilter, StringComparison.OrdinalIgnoreCase)).ToList();
            // The branches should already be sorted, but just in case they aren't
            matchingBranches.Sort();

            if (matchingBranches != null)
            {
                foreach (var remoteBranch in matchingBranches)
                {
                    var remoteBranchDto = new RemoteBranchDto()
                    { 
                        BranchName = remoteBranch
                    };

                    var originBranch = $"origin/{BranchWithoutRefsHeads(remoteBranch)}";
                    await ProgramHelper.RunProgram(
                        programPath: Gem.Instance.LocalAppData.GitExePath,
                        arguments: $"log -1 {originBranch} --pretty=format:\"%ci\"",
                        tokens: null,
                        openNewWindow: false,
                        useShellExecute: false,
                        workingDirectory: this.trackingRepoDirectory.FullName,
                        outputHandler: (s) =>
                        {
                            remoteBranchDto.LastCommitDate = s;
                            return Task.CompletedTask;
                        }).ConfigureAwait(false);
                    remoteBranchDtos.Add(remoteBranchDto);
                }
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                gridRemoteBranches.ItemsSource = remoteBranchDtos;
            });
        }

        private string BranchWithoutRefsHeads(string branch)
        {
            return branch.StartsWith(refsHeads) ? branch[refsHeads.Length..] : branch;
        }

        public async void BtnReCreate_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button b)
            {
                return;
            }
            if (b.DataContext is not DataGridRow dc)
            {
                return;
            }
            if (dc.Item is not RemoteBranchDto dto)
            {
                return;
            }
            if (string.IsNullOrWhiteSpace(dto.BranchName))
            {
                return;
            }

            // Do our best to try and read the branch name and bucket
            var branchParts = dto.BranchName.Split(StringExtensions.FowardSlashCharArray).ToList();
            branchParts.Reverse();
            var enlistmentName = string.Empty;
            var bucketName = string.Empty;
            // If the number of segments in a branch is enough, we assume it's in the format that represents the enlistment name and bucket name
            // These will always have 5+ segments. "user/materia" is counted as 1 because the user has control of this in the repo settings.
            //
            //  1     2       3           4       5     
            // refs/heads/user/Materia/testing/010000.one
            if (branchParts.Count >= 5)
            {
                enlistmentName = branchParts[0];
                bucketName = branchParts[1];
            }

            EnlistmentSettings.EnlistmentSettingsDialogResult? result = null;
            await Global.Instance.MainWindow.Dispatcher.InvokeAsync(() =>
            {
                var enlistmentSettingsEditor = new EnlistmentSettings(bucketName, enlistmentName, true);
                var result = enlistmentSettingsEditor.ShowDialog();
                if (result != null)
                {
                    bucketName = result.BucketName;
                    enlistmentName = result.EnlistmentName;
                }
            });
            if (result == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(enlistmentName) || string.IsNullOrWhiteSpace(bucketName))
            {
                await Global.Instance.MainWindow.AppendCommandLine($"Skipping re-creation of enlistment because required fields are not provided", Brushes.White).ConfigureAwait(false);
                return;
            }

            await Global.Instance.MainWindow.AppendCommandLine($"Re-creating branch '{dto.BranchName}'", Brushes.White).ConfigureAwait(false);

            // Look for an existing bucket with this name
            var bucket = this.repo.Buckets.FirstOrDefault(b => b.GemName != null && b.GemName.Equals(bucketName, StringComparison.OrdinalIgnoreCase));

            // If the bucket doesn't exist yet, then create it
            if (bucket == null)
            {
                var thisRepoNodeContext = GemNodeContext.GetNodeContext(repo: this.repo);
                var createBucketCommandSet = new CommandSet();

                var createBucketCommand = new CreateBucketCommand()
                {
                    BucketName = bucketName
                };
                createBucketCommandSet.Commands.Add(createBucketCommand);
                await Global.Instance.MainWindow.AppendCommandLine($"Re-creating bucket '{bucketName}'", Brushes.White).ConfigureAwait(false);
                await Global.Instance.MainWindow.RunCommandSet(createBucketCommandSet, thisRepoNodeContext).ConfigureAwait(false);

                if (createBucketCommand.ResultBucket == null)
                {
                    MessageBox.Show($"Failed to create the bucket {bucketName}");
                    return;
                }
                bucket = createBucketCommand.ResultBucket;
                this.repo.Buckets.Add(createBucketCommand.ResultBucket);
            }

            var parentEnlistment = bucket.Enlistments.LastOrDefault();
            var cloneFromBranch = BranchWithoutRefsHeads(dto.BranchName);

            // All of the enlistment creation commands expect to see bucket and enlistment object set,
            // We need an enlistment object with the right name set at minimum.
            // This is enough for the commands to get the right directory to clone to.
            var enlistment = new Enlistment(bucket)
            {
                GemName = enlistmentName
            };
            bucket.Enlistments.Add(enlistment);

            var recreateEnlistmentCommandSet = new CommandSet();
            recreateEnlistmentCommandSet.Commands.Add(new GitCloneCommand()
            {
                // If we know about a parent enlistment (a local repo/directory) then use that as the place we clone from.
                // Otherwise use the remote clone URL.
                CloneUrl = bucket.Repo.Metadata.CloneUrl,
                BranchFrom = cloneFromBranch,
                ScopeToBranch = result.ScopeToBranch
            });

            // This sets the *branch* and *URL* that the enlistment will pull from
            recreateEnlistmentCommandSet.Commands.Add(new GitSetPullDetailsCommand()
            {
                FetchFilterBranch = (parentEnlistment == null ? null : await parentEnlistment.GetFullGitBranch().ConfigureAwait(false)) ?? bucket.Repo.Metadata.BranchFrom,
                ScopeToBranch = result.ScopeToBranch
            });

            // Always push to a branch in the main repo and always push to a branch with the same name as the current one
            recreateEnlistmentCommandSet.Commands.Add(new GitSetPushDetailsCommand());

            // Set the user name and email
            recreateEnlistmentCommandSet.Commands.Add(new GitSetUserDetailsCommand());

            // We need a node context that is correct for the creation of the enlistment.
            var recreateNodeContext = GemNodeContext.GetNodeContext(
                repoCollection: this.repo.RepoCollection,
                repo: this.repo,
                bucket: bucket,
                enlistment: enlistment);

            // Run the command set
            await Global.Instance.MainWindow.RunCommandSet(recreateEnlistmentCommandSet, recreateNodeContext).ConfigureAwait(false);
        }

        public async void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button b)
            {
                return;
            }
            if (b.DataContext is not DataGridRow dc)
            {
                return;
            }
            if (dc.Item is not RemoteBranchDto dto)
            {
                return;
            }
            if (string.IsNullOrWhiteSpace(dto.BranchName))
            {
                return;
            }

            await ProgramHelper.RunProgram(
                programPath: Gem.Instance.LocalAppData.GitExePath,
                arguments: $"push origin --delete {this.BranchWithoutRefsHeads(dto.BranchName)}",
                tokens: null, // There are no tokens in the above programPath/arguments
                useShellExecute: false,
                openNewWindow: false,
                workingDirectory: this.trackingRepoDirectory?.FullName
                ).ConfigureAwait(false);

            await this.RefreshRemoteBranches().ConfigureAwait(false);
        }

        private async void RemoteBranches_Loaded(object sender, RoutedEventArgs e)
        {
            await this.RefreshRemoteBranches().ConfigureAwait(true);
        }

        private async void BtnBranchPrefixFilterApply_Click(object sender, RoutedEventArgs e)
        {
            await this.RefreshRemoteBranches().ConfigureAwait(true);
        }

        private class RemoteBranchDto
        {
            public string? BranchName { get; set; }
            public string? LastCommitDate { get; set; }
        }
    }
}
