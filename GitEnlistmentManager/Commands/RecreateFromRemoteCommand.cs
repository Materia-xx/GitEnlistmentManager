using GitEnlistmentManager.CommandSets;
using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Extensions;
using GitEnlistmentManager.Globals;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace GitEnlistmentManager.Commands
{
    public class RecreateFromRemoteCommand : ICommand
    {
        private static readonly string refsHeads = "refs/heads/";
        private static readonly char[] fowardSlashCharArray = { '/' };

        public bool OpenNewWindow { get; set; } = false;

        public string CommandDocumentation { get; set; } = "Recreates repositories from a git server.";

        public string? BranchPrefix { get; set; }

        public void ParseArgs(GemNodeContext nodeContext, Stack<string> arguments)
        {
            if (arguments.Count > 0)
            {
                BranchPrefix = arguments.Pop();
            }
        }

        public async Task<bool> Execute(GemNodeContext nodeContext, MainWindow mainWindow)
        {
            // TODO: add a confirm here before running this
            if (nodeContext.Repo == null || nodeContext.Repo.Metadata.CloneUrl == null)
            {
                return false;
            }

            // If the branch prefix doesn't start with refs/heads then add it as a prefix
            BranchPrefix?.TrimStart('/');
            if (string.IsNullOrWhiteSpace(BranchPrefix))
            {
                BranchPrefix = $"{refsHeads}{nodeContext.Repo.Metadata.BranchPrefix}";
            }
            if (!BranchPrefix.StartsWith(refsHeads))
            {
                BranchPrefix = $"{refsHeads}{BranchPrefix}";
            }

            // Capture the output of this command
            var remoteBranches = new List<string>();
            await ProgramHelper.RunProgram(
                programPath: nodeContext.RepoCollection?.Gem.LocalAppData.GitExePath,
                arguments: $"ls-remote --heads {nodeContext.Repo.Metadata.CloneUrl}",
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
            var matchingBranches = remoteBranches.Where(b => b.StartsWith(BranchPrefix, StringComparison.OrdinalIgnoreCase)).ToList();

            // The branches should already be sorted, but just in case they aren't
            matchingBranches.Sort();

            var validReCreateBuckets = new List<string>();

            // Re-create the buckets/enlistments
            foreach (var branch in matchingBranches)
            {
                var branchParts = branch.Split(fowardSlashCharArray).ToList();
                // The command really only re-creates enlistments created by this program
                // These will always have 7+ segments. "user/materia" is counted as 1 because the user has control of this in the repo settings.
                //
                //  1     2       3           4       5     
                // refs/heads/user/Materia/testing/010000.one
                if (branchParts.Count < 5)
                {
                    await mainWindow.AppendCommandLine($"Skipping re-creation of branch '{branch}' because it does not appear to have been created by this program.", Brushes.Salmon).ConfigureAwait(false);
                    continue;
                }
                await mainWindow.AppendCommandLine($"Re-creating branch '{branch}'", Brushes.White).ConfigureAwait(false);

                branchParts.Reverse();
                var enlistmentName = branchParts[0];
                var bucketName = branchParts[1];

                // Look for an existing bucket with this name
                var bucket = nodeContext.Repo.Buckets.FirstOrDefault(b => b.GemName != null && b.GemName.Equals(bucketName, StringComparison.OrdinalIgnoreCase));

                // Enlistment re-creation is only supported when we're re-creating everything in the bucket.
                // It won't fill in the gaps if some things already exist and others don't
                if (bucket != null)
                {
                    if (!validReCreateBuckets.Contains(bucketName))
                    {
                        await mainWindow.AppendCommandLine($"Skipping re-creation of branch '{branch}' because the bucket already exists.", Brushes.Salmon).ConfigureAwait(false);
                        continue;
                    }
                }
                // If the bucket doesn't exist yet, then create it
                else
                {
                    validReCreateBuckets.Add(bucketName);
                    var createBucketCommand = new CreateBucketCommand()
                    {
                        BucketName = bucketName
                    };
                    await mainWindow.AppendCommandLine($"Re-creating bucket '{bucketName}'", Brushes.White).ConfigureAwait(false);
                    await createBucketCommand.Execute(nodeContext, mainWindow).ConfigureAwait(false);
                    if (createBucketCommand.ResultBucket == null)
                    {
                        MessageBox.Show($"Failed to create the bucket {bucketName}");
                        return false;
                    }
                    bucket = createBucketCommand.ResultBucket;
                    nodeContext.Repo.Buckets.Add(createBucketCommand.ResultBucket);
                }

                var parentEnlistment = bucket.Enlistments.LastOrDefault();

                var cloneFromBranch = branch;
                if (cloneFromBranch.StartsWith(refsHeads))
                {
                    cloneFromBranch = cloneFromBranch[refsHeads.Length..];
                }

                // All of the commands expect to see bucket and enlistment object set, so we at-least need an enlistment object with the right name set
                // This should be enough for the commands to get the right directory to clone to.
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
                // This is different than the one passed in to this command in general. The repo and RepoCollection will be right though.
                var recreateNodeContext = nodeContext.Clone();
                recreateNodeContext.Bucket = bucket;
                recreateNodeContext.Enlistment = enlistment;

                // Run all the commands
                if (!await mainWindow.RunCommandSet(recreateEnlistmentCommandSet, recreateNodeContext).ConfigureAwait(false))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
