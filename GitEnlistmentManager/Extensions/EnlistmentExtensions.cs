using GitEnlistmentManager.DTOs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
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

            if (string.IsNullOrWhiteSpace(enlistment.Name))
            {
                MessageBox.Show("Enlistment name must be set.");
                return null;
            }

            var enlistmentDirectory = new DirectoryInfo(Path.Combine(bucketDirectory.FullName, enlistment.Name));
            if (!enlistmentDirectory.Exists)
            {
                try
                {
                    enlistmentDirectory.Create();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error creating enlistment folder: {ex.Message}");
                    return null;
                }
            }

            return enlistmentDirectory;
        }

        public static int GetNumberPrefix(this Enlistment enlistment)
        {
            if (enlistment.Name != null)
            {
                var enlistmentNameParts = enlistment.Name.Split(dotCharArray);
                if (int.TryParse(enlistmentNameParts[0], out int enlistmentNumberPrefix))
                {
                    return enlistmentNumberPrefix;
                }
            }
            return -1;
        }

        public static string GetFullGitBranch(this Enlistment enlistment)
        {
            return $"{enlistment.Bucket.Repo.Metadata.BranchPrefix}/{enlistment.Bucket.Repo.RepoCollection.Name}/{enlistment.Bucket.Repo.Name}/{enlistment.Bucket.Name}/{enlistment.Name}";
        }

        public static Enlistment? GetParentEnlistment(this Enlistment enlistment)
        {
            Enlistment? parentEnlistment = null;
            foreach (var examineEnlistment in enlistment.Bucket.Enlistments)
            {
                if (examineEnlistment.Name == enlistment.Name)
                {
                    break;
                }
                parentEnlistment = examineEnlistment;
            }
            return parentEnlistment;
        }

        public static async Task<bool> SetBranchOriginUrl(this Enlistment enlistment, MainWindow mainWindow, string originUrl)
        {
            var enlistmentDirectory = enlistment?.GetDirectoryInfo();
            if (enlistmentDirectory == null || enlistment == null)
            {
                return false;
            }

            var enlistmentTokens = enlistment.GetTokens();

            // This will set the "URL" that the enlistment pulls from.
            if (!await mainWindow.RunCommand(
                programPath: enlistment.Bucket.Repo.RepoCollection.Gem.LocalAppData.GitExePath,
                arguments: $@"remote set-url origin {originUrl}",
                tokens: enlistmentTokens,
                workingFolder: enlistmentDirectory.FullName
                ).ConfigureAwait(true))
            {
                return false;
            }

            // Git pull so this workspace becomes aware of the branch in the parent repo
            if (!await mainWindow.RunCommand(
                programPath: enlistment.Bucket.Repo.RepoCollection.Gem.LocalAppData.GitExePath,
                arguments: $@"pull",
                tokens: enlistmentTokens,
                workingFolder: enlistmentDirectory.FullName
                ).ConfigureAwait(true))
            {
                return false;
            }

            return true;
        }

        public static async Task<bool> SetPullFromBranch(this Enlistment enlistment, MainWindow mainWindow, string? pullFromBranch)
        {
            var enlistmentDirectory = enlistment?.GetDirectoryInfo();
            if (enlistmentDirectory == null || enlistment == null || pullFromBranch == null)
            {
                return false;
            }

            if (!await mainWindow.RunCommand(
                programPath: enlistment.Bucket.Repo.RepoCollection.Gem.LocalAppData.GitExePath,
                arguments: $@"branch --set-upstream-to=origin/{pullFromBranch} {enlistment.GetFullGitBranch()}",
                tokens: enlistment.GetTokens(),
                workingFolder: enlistmentDirectory.FullName
                ).ConfigureAwait(true))
            {
                return false;
            }

            return true;
        }

        public static async Task<bool> CreateEnlistment(this Enlistment enlistment, MainWindow mainWindow, EnlistmentPlacement enlistmentPlacement, Enlistment? referenceEnlistment = null)
        {
            if (string.IsNullOrWhiteSpace(enlistment.Name))
            {
                MessageBox.Show("Enlistment name must be set.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(enlistment.Bucket.Repo.Metadata.BranchPrefix))
            {
                MessageBox.Show("Branch prefix must be set in the repo settings before creating an enlistment.");
                return false;
            }

            // TODO: if enlistment name already has a number prefix, then fail

            var bucketDirectory = enlistment.Bucket.GetDirectoryInfo();
            if (bucketDirectory == null)
            {
                MessageBox.Show("Unable to determine bucket directory");
                return false;
            }

            // If PlaceAbove is set, then referenceEnlistment should be set to the enlistment that we're placing a new enlistment above.
            if (enlistmentPlacement == EnlistmentPlacement.PlaceAbove && referenceEnlistment == null)
            {
                MessageBox.Show("Reference enlistment must be specified when using PlaceAbove mode");
                return false;
            }

            if (string.IsNullOrWhiteSpace(enlistment.Bucket.Repo.Metadata.UserName) || string.IsNullOrWhiteSpace(enlistment.Bucket.Repo.Metadata.UserEmail))
            {
                MessageBox.Show("User name and email must be set in the repo settings before creating an enlistment.");
                return false;
            }

            await mainWindow.ClearCommandWindow().ConfigureAwait(false);

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
                        var separationAmount = 2000; // How far apart to place new enlistments // TODO: move this to GEM settings
                        if (parentEnlistment != null)
                        {
                            newNumberPrefix = parentEnlistment.GetNumberPrefix() + separationAmount;
                        }
                    }
                    break;
                case EnlistmentPlacement.PlaceAbove:
                    foreach (var examineEnlistment in enlistment.Bucket.Enlistments)
                    {
                        if (referenceEnlistment != null && examineEnlistment.Name == referenceEnlistment.Name)
                        {
                            var parentEnlistmentNumberPrefix = parentEnlistment == null ? 0 : parentEnlistment.GetNumberPrefix();
                            var childEnlimentNumberPrefix = examineEnlistment.GetNumberPrefix();
                            newNumberPrefix = parentEnlistmentNumberPrefix + (childEnlimentNumberPrefix - parentEnlistmentNumberPrefix) / 2;
                            if (parentEnlistmentNumberPrefix == newNumberPrefix || childEnlimentNumberPrefix == newNumberPrefix)
                            {
                                MessageBox.Show("Unable to determine an appropriate numbering prefix for the new enlistment.");
                                return false;
                            }
                            break;
                        }
                        parentEnlistment = examineEnlistment;
                    }
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

            enlistment.Name = $"{newNumberPrefix:000000}.{enlistment.Name}";
            var enlistmentDirectory = enlistment.GetDirectoryInfo();
            if (enlistmentDirectory == null)
            {
                MessageBox.Show("Encountered an error getting the enlistment directory.");
                return false;
            }

            var gitAutoCrlfOption = "--config core.autocrlf=false";

            // If we know about a parent enlistment (a local repo/folder) then use that as the place we clone from. Otherwise use the remote clone URL.
            var gitCloneSource = parentEnlistment?.GetDirectoryInfo()?.FullName ?? enlistment.Bucket.Repo.Metadata.CloneUrl;

            // The intention is that a branch will never be changed to a different branch in these enlistments
            // So we set --depth 1 to save some time/space. But this only works when cloning from the remote repo and not a local parent folder.
            var gitShallowOption = gitCloneSource == enlistment.Bucket.Repo.Metadata.CloneUrl ? "--depth 1" : string.Empty;

            var enlistmentTokens = enlistment.GetTokens();

            // Clone the enlistment
            // Note: Git on the commandline adds progress lines that are not included in redirected output.
            //       It is possible to add --progress to see them, but because this program is not a true
            //       command terminal it spams many lines instead of keeping the progress on one line.
            //       I've left the option out for that reason.
            if (!await mainWindow.RunCommand(
                programPath: enlistment.Bucket.Repo.RepoCollection.Gem.LocalAppData.GitExePath,
                arguments: $"clone {gitShallowOption} {gitAutoCrlfOption} {gitCloneSource} \"{enlistmentDirectory.FullName}\"",
                tokens: enlistmentTokens,
                workingFolder: bucketDirectory.FullName
                ).ConfigureAwait(true))
            {
                return false;
            }

            // Make sure this branch has knowledge of the branch we are branching from, otherwise it will error out
            // If we are branching from a local repo/folder then use the branch there as the branch to pull from
            // Otherwise use the branch from the remote repo
            if (!await mainWindow.RunCommand(
                programPath: enlistment.Bucket.Repo.RepoCollection.Gem.LocalAppData.GitExePath,
                arguments: $"checkout {parentEnlistment?.GetFullGitBranch() ?? enlistment.Bucket.Repo.Metadata.BranchFrom}",
                tokens: enlistmentTokens,
                workingFolder: enlistmentDirectory.FullName
                ).ConfigureAwait(true))
            {
                return false;
            }

            // Create the new branch that this folder will represent
            if (!await mainWindow.RunCommand(
                programPath: enlistment.Bucket.Repo.RepoCollection.Gem.LocalAppData.GitExePath,
                arguments: $@"checkout -b ""{enlistment.GetFullGitBranch()}""",
                tokens: enlistmentTokens,
                workingFolder: enlistmentDirectory.FullName
                ).ConfigureAwait(true))
            {
                return false;
            }

            // This sets the *branch* that the enlistment will pull from
            await enlistment.SetPullFromBranch(mainWindow, parentEnlistment?.GetFullGitBranch() ?? enlistment.Bucket.Repo.Metadata.BranchFrom).ConfigureAwait(false);

            // This will make it so 'git push' always pushes to a branch in the main repo
            // i.e. child branch e3 will not push to child branch e2, but rather to the original repo
            if (!await mainWindow.RunCommand(
                programPath: enlistment.Bucket.Repo.RepoCollection.Gem.LocalAppData.GitExePath,
                arguments: $@"remote set-url --push origin {enlistment.Bucket.Repo.Metadata.CloneUrl}",
                tokens: enlistmentTokens,
                workingFolder: enlistmentDirectory.FullName
                ).ConfigureAwait(true))
            {
                return false;
            }

            // This is the branch that 'git push' will publish to
            // It is set to publish a branch with the same name on the remote
            if (!await mainWindow.RunCommand(
                programPath: enlistment.Bucket.Repo.RepoCollection.Gem.LocalAppData.GitExePath,
                arguments: $@"config push.default current",
                tokens: enlistmentTokens,
                workingFolder: enlistmentDirectory.FullName
                ).ConfigureAwait(true))
            {
                return false;
            }

            // Set the user name
            if (!await mainWindow.RunCommand(
                programPath: enlistment.Bucket.Repo.RepoCollection.Gem.LocalAppData.GitExePath,
                arguments: $@"config --local user.name ""{enlistment.Bucket.Repo.Metadata.UserName}""",
                tokens: enlistmentTokens,
                workingFolder: enlistmentDirectory.FullName
                ).ConfigureAwait(true))
            {
                return false;
            }

            // Set the user email
            if (!await mainWindow.RunCommand(
                programPath: enlistment.Bucket.Repo.RepoCollection.Gem.LocalAppData.GitExePath,
                arguments: $@"config --local user.email ""{enlistment.Bucket.Repo.Metadata.UserEmail}""",
                tokens: enlistmentTokens,
                workingFolder: enlistmentDirectory.FullName
                ).ConfigureAwait(true))
            {
                return false;
            }

            // If we injected an enlistment then the child most enlistment needs to be re-parented for pulls to the newly injected enlistment
            if (enlistmentPlacement == EnlistmentPlacement.PlaceAbove)
            {
                var referenceEnlistmentDirectory = referenceEnlistment?.GetDirectoryInfo();
                if (referenceEnlistment != null && referenceEnlistmentDirectory != null && enlistmentDirectory != null)
                {
                    // Set the *URL* that the reference enlistment will pull from
                    await referenceEnlistment.SetBranchOriginUrl(mainWindow, enlistmentDirectory.FullName).ConfigureAwait(false);

                    // This sets the *branch* that the reference enlistment will pull from
                    await referenceEnlistment.SetPullFromBranch(mainWindow, enlistment.GetFullGitBranch()).ConfigureAwait(false);
                }
            }

            // Run any "After Clone" command sets attached to this enlistment
            var afterCloneCommandSets = enlistment.Bucket.Repo.RepoCollection.Gem.GetCommandSets(
                placement: CommandSetPlacement.AfterEnlistmentClone,
                repoCollection: enlistment.Bucket.Repo.RepoCollection,
                repo: enlistment.Bucket.Repo,
                bucket: enlistment.Bucket,
                enlistment: enlistment);
            return await mainWindow.RunCommandSets(afterCloneCommandSets, enlistment.GetTokens(), enlistment.GetDirectoryInfo()?.FullName).ConfigureAwait(false);
        }

        public static Dictionary<string, string> GetTokens(this Enlistment enlistment)
        {
            var tokens = enlistment.Bucket.GetTokens();

            if (enlistment.Name != null)
            {
                tokens["EnlistmentName"] = enlistment.Name;
            }

            tokens["EnlistmentBranch"] = enlistment.GetFullGitBranch();

            var enlistmentDirectory = enlistment.GetDirectoryInfo();
            if (enlistmentDirectory != null)
            {
                tokens["EnlistmentDirectory"] = enlistmentDirectory.FullName;
            }

            var pullRequestUrl = enlistment.GetPullRequestUrl();
            if (pullRequestUrl != null)
            {
                tokens["EnlistmentPullRequestUrl"] = pullRequestUrl;
            }

            return tokens;
        }

        public static string? GetPullRequestUrl(this Enlistment enlistment)
        {
            var hostingPlatform = GitHostingPlatforms.Instance.Platforms.FirstOrDefault(p => p.Name == enlistment.Bucket.Repo.Metadata.GitHostingPlatformName);
            return hostingPlatform?.CalculatePullRequestUrl(enlistment);
        }
    }
}
