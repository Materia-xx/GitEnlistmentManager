﻿using GitEnlistmentManager.Commands;
using GitEnlistmentManager.CommandSets;
using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Globals;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GitEnlistmentManager.Extensions
{
    public static class EnlistmentExtensions
    {
        private static readonly char[] dotCharArray = { '.' };

        public static DirectoryInfo? GetDirectoryInfo(this Enlistment enlistment)
        {
            var bucketDirectory = enlistment.Bucket.GetDirectoryInfo();
            if (bucketDirectory == null)
            {
                MessageBox.Show("Unable to determine bucket directory");
                return null;
            }

            if (string.IsNullOrWhiteSpace(enlistment.GemName))
            {
                MessageBox.Show("Enlistment name must be set.");
                return null;
            }

            var enlistmentDirectory = new DirectoryInfo(Path.Combine(bucketDirectory.FullName, enlistment.GemName));
            if (!enlistmentDirectory.Exists)
            {
                try
                {
                    enlistmentDirectory.Create();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error creating enlistment directory: {ex.Message}");
                    return null;
                }
            }

            return enlistmentDirectory;
        }

        public static int GetNumberPrefix(this Enlistment enlistment)
        {
            if (enlistment.GemName != null)
            {
                var enlistmentNameParts = enlistment.GemName.Split(dotCharArray);
                if (enlistmentNameParts.Length > 1)
                {
                    if (int.TryParse(enlistmentNameParts[0], out int enlistmentNumberPrefix))
                    {
                        return enlistmentNumberPrefix;
                    }
                }
            }
            return -1;
        }

        public static Enlistment? GetParentEnlistment(this Enlistment enlistment)
        {
            Enlistment? parentEnlistment = null;
            foreach (var examineEnlistment in enlistment.Bucket.Enlistments)
            {
                if (examineEnlistment.GemName == enlistment.GemName)
                {
                    break;
                }
                parentEnlistment = examineEnlistment;
            }
            return parentEnlistment;
        }

        public static Enlistment? GetChildEnlistment(this Enlistment enlistment)
        {
            for (int i = 0; i < enlistment.Bucket.Enlistments.Count -1; i++)
            {
                if (enlistment.Bucket.Enlistments[i].GemName == enlistment.GemName)
                {
                    return enlistment.Bucket.Enlistments[i + 1];
                }
            }
            return null;
        }

        public static async Task<bool> CreateEnlistment(this Enlistment enlistment, EnlistmentPlacement enlistmentPlacement, Enlistment? childEnlistment, bool scopeToBranch, bool gitAutoCrlf)
        {
            if (string.IsNullOrWhiteSpace(enlistment.GemName))
            {
                MessageBox.Show("Enlistment name must be set.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(enlistment.Bucket.Repo.Metadata.BranchPrefix))
            {
                MessageBox.Show("Branch prefix must be set in the repo settings before creating an enlistment.");
                return false;
            }

            var bucketDirectory = enlistment.Bucket.GetDirectoryInfo();
            if (bucketDirectory == null)
            {
                MessageBox.Show("Unable to determine bucket directory");
                return false;
            }

            // If PlaceAbove is set, then childEnlistment should be set to the enlistment that we're placing a new enlistment above.
            if (enlistmentPlacement == EnlistmentPlacement.PlaceAbove && childEnlistment == null)
            {
                MessageBox.Show("Child enlistment must be specified when using PlaceAbove mode");
                return false;
            }

            if (string.IsNullOrWhiteSpace(enlistment.Bucket.Repo.Metadata.UserName) || string.IsNullOrWhiteSpace(enlistment.Bucket.Repo.Metadata.UserEmail))
            {
                MessageBox.Show("User name and email must be set in the repo settings before creating an enlistment.");
                return false;
            }

            await Global.Instance.MainWindow.ClearCommandWindow().ConfigureAwait(false);

            Enlistment? parentEnlistment = null;
            int newNumberPrefix = -1;
            switch (enlistmentPlacement)
            {
                case EnlistmentPlacement.PlaceAtEnd:
                    // If we found any enlistments already present in the bucket, then set the parent enlistment to the last one.
                    // Otherwise it will remain null and the handler below will treat this as a top level enlistment connected to the remote repo
                    if (enlistment.Bucket.Enlistments.Count > 0)
                    {
                        var lastEnlistmentNumberPrefix = enlistment.Bucket.Enlistments.Max(e => e.GetNumberPrefix());
                        parentEnlistment = enlistment.Bucket.Enlistments.FirstOrDefault(e => e.GetNumberPrefix() == lastEnlistmentNumberPrefix);
                        if (parentEnlistment != null)
                        {
                            newNumberPrefix = parentEnlistment.GetNumberPrefix() + Gem.Instance.LocalAppData.EnlistmentIncrement;
                        }

                        // Add the enlistment to the end of the enlistment settings
                        enlistment.Bucket.Enlistments.Add(enlistment);
                    }
                    break;
                case EnlistmentPlacement.PlaceAbove:
                    if (childEnlistment != null)
                    {
                        parentEnlistment = childEnlistment.GetParentEnlistment();

                        // The number prefix of the (grand)parent (if there is one) e.g. 010000
                        var parentEnlistmentNumberPrefix = parentEnlistment == null ? 0 : parentEnlistment.GetNumberPrefix();

                        // The number prefix of the child e.g. 012000
                        var childEnlimentNumberPrefix = childEnlistment.GetNumberPrefix();

                        // The middle of the 2 numbers above e.g. 011000
                        newNumberPrefix = parentEnlistmentNumberPrefix + (childEnlimentNumberPrefix - parentEnlistmentNumberPrefix) / 2;

                        // If there wasn't any space left between the first 2 numbers e.g. grandparent was 015555 and child was 015554
                        if (parentEnlistmentNumberPrefix == newNumberPrefix || childEnlimentNumberPrefix == newNumberPrefix)
                        {
                            MessageBox.Show("Unable to determine an appropriate numbering prefix for the new enlistment.");
                            return false;
                        }

                        // Insert the enlistment into the collection at the right spot
                        var childIndex = enlistment.Bucket.Enlistments.IndexOf(childEnlistment);
                        enlistment.Bucket.Enlistments.Insert(childIndex, enlistment);
                    }
                    break;
                case EnlistmentPlacement.Direct:
                    break;
                default:
                    MessageBox.Show("Enlistment placement mode not handled");
                    return false;
            }

            // Default number prefix if not otherwise specified
            if (newNumberPrefix == -1)
            {
                newNumberPrefix = 10000;
            }

            // We pad numbers with 0s so they sort correctly in file explorer, numbers over 999999 won't work
            if (newNumberPrefix > 999999)
            {
                MessageBox.Show("The program does not support enlistments with a prefix number over 999999.");
                return false;
            }

            // Direct enlistments don't add a number prefix
            if (enlistmentPlacement != EnlistmentPlacement.Direct)
            {
                enlistment.GemName = $"{newNumberPrefix:000000}.{enlistment.GemName}";
            }

            var enlistmentDirectory = enlistment.GetDirectoryInfo();
            if (enlistmentDirectory == null)
            {
                MessageBox.Show("Encountered an error getting the enlistment directory.");
                return false;
            }

            var nodeContext = GemNodeContext.GetNodeContext(enlistment: enlistment);

            // Clone the enlistment
            var createEnlistmentCommandSet = new CommandSet();
            createEnlistmentCommandSet.Commands.Add(new GitCloneCommand()
            {
                // If we know about a parent enlistment (a local repo/directory) then use that as the place we clone from.
                // Otherwise use the remote clone URL.
                CloneUrl = parentEnlistment?.GetDirectoryInfo()?.FullName ?? enlistment.Bucket.Repo.Metadata.CloneUrl,
                BranchFrom = (parentEnlistment == null ? null : await parentEnlistment.GetFullGitBranch().ConfigureAwait(false)) ?? enlistment.Bucket.Repo.Metadata.BranchFrom,
                ScopeToBranch = scopeToBranch,
                GitAutoCrlf = gitAutoCrlf
            });

            // Direct enlistments don't create a branch, they just keep using the main repo branch
            if (enlistmentPlacement != EnlistmentPlacement.Direct)
            {
                // Create the branch that this enlistment will be working in
                createEnlistmentCommandSet.Commands.Add(new GitCreateBranchCommand()
                {
                    Branch = $"{enlistment.Bucket.Repo.Metadata.BranchPrefix}/{enlistment.Bucket.GemName}/{enlistment.GemName}",
                });
            }

            // This sets the *branch* and *URL* that the enlistment will pull from
            createEnlistmentCommandSet.Commands.Add(new GitSetPullDetailsCommand()
            {
                FetchFilterBranch = (parentEnlistment == null ? null : await parentEnlistment.GetFullGitBranch().ConfigureAwait(false)) ?? enlistment.Bucket.Repo.Metadata.BranchFrom,
                ScopeToBranch = scopeToBranch
            });

            // Always push to a branch in the main repo and always push to a branch with the same name as the current one
            createEnlistmentCommandSet.Commands.Add(new GitSetPushDetailsCommand());

            // Set the user name and email
            createEnlistmentCommandSet.Commands.Add(new GitSetUserDetailsCommand());

            // If not scoping to the branch, then pull again to fetch all branches
            if (!scopeToBranch)
            {
                createEnlistmentCommandSet.Commands.Add(new GitPullCommand());
            }

            // Run all the commands
            if (!await createEnlistmentCommandSet.RunCommandSet(nodeContext).ConfigureAwait(false))
            {
                return false;
            }

            // If placing an enlistment above, the child (focused in the UI) enlistment needs to be now re-parented
            // This needs to be ran after the above command set is complete, otherwise enlistment.GetFullGitBranch won't work correctly
            if (enlistmentPlacement == EnlistmentPlacement.PlaceAbove && childEnlistment != null)
            {
                var reparentCommandSet = new CommandSet();
                var setPullCommand = new GitSetPullDetailsCommand()
                {
                    FetchFilterBranch = await enlistment.GetFullGitBranch().ConfigureAwait(false),
                    ScopeToBranch = scopeToBranch
                };
                setPullCommand.NodeContext.Enlistment = childEnlistment;
                reparentCommandSet.Commands.Add(setPullCommand);
                if (!await reparentCommandSet.RunCommandSet(nodeContext).ConfigureAwait(false))
                {
                    return false;
                }
            }

            // Run any "After Enlistment Create" command sets attached to this enlistment
            var afterEnlistmentCreateCommandSet = enlistment.Bucket.Repo.RepoCollection.Gem.GetCommandSets(
                placement: CommandSetPlacement.AfterEnlistmentCreate,
                mode: CommandSetMode.Any,
                repoCollection: enlistment.Bucket.Repo.RepoCollection,
                repo: enlistment.Bucket.Repo,
                bucket: enlistment.Bucket,
                enlistment: enlistment);
            return await afterEnlistmentCreateCommandSet.RunCommandSets(GemNodeContext.GetNodeContext(enlistment: enlistment)).ConfigureAwait(false);
        }

        public static async Task<Dictionary<string, string>> GetTokens(this Enlistment enlistment)
        {
            var tokens = enlistment.Bucket.GetTokens();
            var parent = enlistment.GetParentEnlistment();
            var child = enlistment.GetChildEnlistment();

            if (enlistment.GemName != null)
            {
                tokens["EnlistmentName"] = enlistment.GemName;
            }

            tokens["EnlistmentBranch"] = await enlistment.GetFullGitBranch().ConfigureAwait(false) ?? string.Empty;

            var enlistmentDirectory = enlistment.GetDirectoryInfo();
            if (enlistmentDirectory != null)
            {
                tokens["EnlistmentDirectory"] = enlistmentDirectory.FullName;
            }

            if (parent!= null)
            {
                if (parent.GemName != null)
                {
                    tokens["ParentEnlistmentName"] = parent.GemName;
                }

                tokens["ParentEnlistmentBranch"] = await parent.GetFullGitBranch().ConfigureAwait(false) ?? string.Empty;

                var parentEnlistmentDirectory = parent?.GetDirectoryInfo();
                if (parentEnlistmentDirectory != null)
                {
                    tokens["ParentEnlistmentDirectory"] = parentEnlistmentDirectory.FullName;
                }
            }

            if (child!= null)
            {
                if (child.GemName != null)
                {
                    tokens["ChildEnlistmentName"] = child.GemName;
                }

                tokens["ChildEnlistmentBranch"] = await child.GetFullGitBranch().ConfigureAwait(false) ?? string.Empty;
                
                var childEnlistmentDirectory = child?.GetDirectoryInfo();
                if (childEnlistmentDirectory != null)
                {
                    tokens["ChildEnlistmentDirectory"] = childEnlistmentDirectory.FullName;
                }
            }

            var pullRequestUrl = await enlistment.GetPullRequestUrl().ConfigureAwait(false);
            if (pullRequestUrl != null)
            {
                tokens["EnlistmentPullRequestUrl"] = pullRequestUrl;
            }

            return tokens;
        }

        public static async Task<string?> GetPullRequestUrl(this Enlistment enlistment)
        {
            var hostingPlatform = GitHostingPlatforms.Instance.Platforms.FirstOrDefault(p => p.Name == enlistment.Bucket.Repo.Metadata.GitHostingPlatformName);
            return hostingPlatform == null ? null : await hostingPlatform.CalculatePullRequestUrl(enlistment).ConfigureAwait(false);
        }
    }
}
