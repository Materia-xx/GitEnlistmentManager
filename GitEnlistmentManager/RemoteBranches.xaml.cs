using GitEnlistmentManager.Commands;
using GitEnlistmentManager.CommandSets;
using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Extensions;
using GitEnlistmentManager.Globals;
using System;
using System.Collections.Generic;
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
        private MainWindow mainWindow;

        public RemoteBranches(Repo repo, MainWindow mainWindow)
        {
            InitializeComponent();
            this.repo = repo;
            this.mainWindow = mainWindow;
        }

        private async Task RefreshRemoteBranches()
        {
            var remoteBranchDtos = new List<RemoteBranchDto>();

            if (this.repo == null || this.repo.Metadata.CloneUrl == null)
            {
                return;
            }

            // If the branch prefix doesn't start with refs/heads then add it as a prefix
            txtBranchPrefixFilter.Text?.TrimStart('/');
            if (string.IsNullOrWhiteSpace(txtBranchPrefixFilter.Text))
            {
                txtBranchPrefixFilter.Text = $"{refsHeads}{this.repo.Metadata.BranchPrefix}";
            }
            if (!txtBranchPrefixFilter.Text.StartsWith(refsHeads))
            {
                txtBranchPrefixFilter.Text = $"{refsHeads}{txtBranchPrefixFilter.Text}";
            }

            // Capture the output of this command
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

            Application.Current.Dispatcher.Invoke(() =>
            {
                // Pick out only branches that match the prefix
                var matchingBranches = remoteBranches.Where(b => b.StartsWith(txtBranchPrefixFilter.Text, StringComparison.OrdinalIgnoreCase)).ToList();

                // The branches should already be sorted, but just in case they aren't
                matchingBranches.Sort();

                foreach (var remoteBranch in matchingBranches)
                {
                    remoteBranchDtos.Add(new RemoteBranchDto()
                    {
                        BranchName = remoteBranch,
                        IsMerged = true,
                        LastCommitDate = DateTime.Now
                    });
                }

                gridRemoteBranches.ItemsSource = remoteBranchDtos;
            });
        }

        private class RemoteBranchDto
        {
            public string? BranchName { get; set; }
            public DateTime LastCommitDate { get; set; }
            public bool IsMerged { get; set; }
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

            // TODO: at this point, show a dialog that lets the user correct the enlistment name and bucket name if desired. In all cases, even if we read it above.
            if (string.IsNullOrWhiteSpace(enlistmentName) || string.IsNullOrWhiteSpace(bucketName))
            {
                await mainWindow.AppendCommandLine($"Skipping re-creation of enlistment because required fields are not provided", Brushes.White).ConfigureAwait(false);
                return;
            }

            await mainWindow.AppendCommandLine($"Re-creating branch '{dto.BranchName}'", Brushes.White).ConfigureAwait(false);

            // Look for an existing bucket with this name
            var bucket = this.repo.Buckets.FirstOrDefault(b => b.GemName != null && b.GemName.Equals(bucketName, StringComparison.OrdinalIgnoreCase));

            // TODO: document how to use the re-create option correctly
            // TODO: - Only branches that originally existed in a bucket should be recreated in that bucket.
            // TODO: - Branches should be re-created in the same order they originally existed in the bucket.

            // If the bucket doesn't exist yet, then create it
            if (bucket == null)
            {
                var createBucketCommand = new CreateBucketCommand()
                {
                    BucketName = bucketName
                };
                await mainWindow.AppendCommandLine($"Re-creating bucket '{bucketName}'", Brushes.White).ConfigureAwait(false);
                await createBucketCommand.Execute(GemNodeContext.GetNodeContext(repo: this.repo), mainWindow).ConfigureAwait(false);
                if (createBucketCommand.ResultBucket == null)
                {
                    MessageBox.Show($"Failed to create the bucket {bucketName}");
                    return;
                }
                bucket = createBucketCommand.ResultBucket;
                this.repo.Buckets.Add(createBucketCommand.ResultBucket);
            }

            var parentEnlistment = bucket.Enlistments.LastOrDefault();

            var cloneFromBranch = dto.BranchName;
            if (cloneFromBranch.StartsWith(refsHeads))
            {
                cloneFromBranch = cloneFromBranch[refsHeads.Length..];
            }

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
                PullFrom = (parentEnlistment == null ? null : await parentEnlistment.GetFullGitBranch().ConfigureAwait(false)) ?? bucket.Repo.Metadata.BranchFrom
            });

            // This sets the *branch* and *URL* that the enlistment will pull from
            recreateEnlistmentCommandSet.Commands.Add(new GitSetPullDetailsCommand());

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
            await mainWindow.RunCommandSet(recreateEnlistmentCommandSet, recreateNodeContext).ConfigureAwait(false);
        }

        public void BtnDelete_Click(object sender, RoutedEventArgs e)
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

            // TODO: this is the git command to run
            // git push origin --delete user/Materia/trim/010000.w1

            // TODO: write code
        }

        private async void RemoteBranches_Loaded(object sender, RoutedEventArgs e)
        {
            await this.RefreshRemoteBranches().ConfigureAwait(true);
        }

        private async void BtnBranchPrefixFilterApply_Click(object sender, RoutedEventArgs e)
        {
            await this.RefreshRemoteBranches().ConfigureAwait(true);
        }
    }
}
