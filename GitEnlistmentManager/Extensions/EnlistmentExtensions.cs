using GitEnlistmentManager.DTOs;
using System;
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

        public static async Task<bool> CreateEnlistment(this Enlistment enlistment, Enlistment? parentEnlistment, MainWindow mainWindow)
        {
            if (string.IsNullOrWhiteSpace(enlistment.Name))
            {
                MessageBox.Show("Enlistment name must be set.");
                return false;
            }

            var bucketDirectory = enlistment.Bucket.GetDirectoryInfo();
            if (bucketDirectory == null)
            {
                MessageBox.Show("Unable to determine bucket directory");
                return false;
            }

            // Look at existing exlistments to figure out the numbering prefix of this folder
            var highestNumberPrefix = 0; // Default starting prefix if no others found
            var lastEnlistmentName = string.Empty;
            foreach (var existingEnlistmentDirectory in bucketDirectory.GetDirectories().OrderBy(d => d.Name))
            {
                lastEnlistmentName = existingEnlistmentDirectory.Name;
                var directoryNameParts = existingEnlistmentDirectory.Name.Split(dotCharArray);
                // All directories created should have a prefix that is just a number
                if (directoryNameParts.Length > 1)
                {
                    if (int.TryParse(directoryNameParts[0], out int enlistmentNumberPrefix))
                    {
                        if (enlistmentNumberPrefix > highestNumberPrefix)
                        {
                            highestNumberPrefix = enlistmentNumberPrefix;
                        }
                    }
                }
            }

            // If the parent enlistmetnt was passed in then just keep that choice, but if not then
            // make the last enlistment the new parent enlistment
            if (parentEnlistment == null && !string.IsNullOrWhiteSpace(lastEnlistmentName))
            {
                parentEnlistment = enlistment.Bucket.Enlistments.Single(e => e.Name != null && e.Name.Equals(lastEnlistmentName, StringComparison.OrdinalIgnoreCase));
            }

            var separationAmount = 2000; // How far apart to place new enlistments
            var newNumberPrefix = highestNumberPrefix == 0 ? 10000 : highestNumberPrefix + separationAmount;
            if (newNumberPrefix > 999999)
            {
                MessageBox.Show("The program does not support enlistments with a prefix number over 999999.");
                return false;
            }

            // Instructions on how to inject an enlistment between 2 local repos
            // NOTE: when injecting, we need to create a number that is between the parent and child repos
            // 1. Create the enlistment
            // 2. Hook up the pull arrow to the parent enlistment as we would normally, it's just that we choose what the parent enlistment is instead of just using the last one
            //      There needs to be a way to tell the function that the main remote repo is the parent enlistment
            // 3. Figure out the enlistment that is "after" the injected one and adjust its pull location to now be the injected enlistment.
            // 4. Hook up the push of the injected enlistment in the same way as usual, nothing different here.

            // Note we don't call enlistment.GetDirectoryInfo here because we want the clone command to create the directory
            var enlistmentDirectory = new DirectoryInfo(Path.Combine(bucketDirectory.FullName, $"{newNumberPrefix:000000}.{enlistment.Name}"));

            // TODO: load this from repo settings
            var gitBranchPrefix = "user/materia";
            var gitBranchFqn = $"{gitBranchPrefix}/{bucketDirectory.Name}/{enlistmentDirectory.Name}";
            var parentEnlistmentBranchFqn = parentEnlistment == null ? null : $"{gitBranchPrefix}/{bucketDirectory.Name}/{parentEnlistment.Name}";

            // If we know about a parent enlistment (a local repo/folder) then use that as the place we clone from. Otherwuse use the remote clone url.
            var gitCloneSource = parentEnlistment?.GetDirectoryInfo()?.FullName ?? enlistment.Bucket.Repo.Metadata.CloneUrl;

            // The intention is that a branch will never be changed to a different branch in these enlistments
            // So we set --depth 1 to save some time/space. But this only works when cloning from the remote repo and not a local parent folder.
            var gitShallowOption = gitCloneSource == enlistment.Bucket.Repo.Metadata.CloneUrl ? "--depth 1" : string.Empty;
            var gitAutoCrlfOption = "--config core.autocrlf=false";

            // If we are branching from a local repo/folder then use the branch there as the branch to pull from
            // Otherwise use the branch from the remote repo
            var gitBranchFrom = parentEnlistment != null ? parentEnlistmentBranchFqn : enlistment.Bucket.Repo.Metadata.BranchFrom;
            var gitThisBranchFrom = gitBranchFrom;

            var gitPullFrom = $"origin/{gitBranchFrom}";

            // Clone the enlistment
            // Note: Git on the commandline adds progress lines that are not included in redirected output.
            //       It is possible to add --progress to see them, but because this program is not a true
            //       command terminal it spams many lines instead of keeping the progress on one line.
            //       I've left the option out for that reason.
            if (!await mainWindow.RunCommand(
                programPath: enlistment.Bucket.Repo.Gem.Metadata.GitExePath,
                arguments: $"clone {gitShallowOption} {gitAutoCrlfOption} {gitCloneSource} \"{enlistmentDirectory.FullName}\"",
                workingFolder: bucketDirectory.FullName
                ).ConfigureAwait(true))
            {
                return false;
            }

            // Make sure this branch has knowledge of the branch we are branching from, otherwise it will error out
            if (!await mainWindow.RunCommand(
                programPath: enlistment.Bucket.Repo.Gem.Metadata.GitExePath,
                arguments: $"checkout {gitThisBranchFrom}",
                workingFolder: enlistmentDirectory.FullName
                ).ConfigureAwait(true))
            {
                return false;
            }

            // Create the new branch that this folder will represent
            if (!await mainWindow.RunCommand(
                programPath: enlistment.Bucket.Repo.Gem.Metadata.GitExePath,
                arguments: $@"checkout -b ""{gitBranchFqn}""",
                workingFolder: enlistmentDirectory.FullName
                ).ConfigureAwait(true))
            {
                return false;
            }

            // This sets the branch 'git pull' will pull updates from
            // Each enlistment will pull from its parent enlistment, and the top level enlistment will pull from the original repo
            if (!await mainWindow.RunCommand(
                programPath: enlistment.Bucket.Repo.Gem.Metadata.GitExePath,
                arguments: $@"branch --set-upstream-to={gitPullFrom} {gitBranchFqn}",
                workingFolder: enlistmentDirectory.FullName
                ).ConfigureAwait(true))
            {
                return false;
            }

            // This will make it so 'git push' always pushes to a branch in the main repo
            // i.e. child branch e3 will not push to child branch e2, but rather to the original repo
            if (!await mainWindow.RunCommand(
                programPath: enlistment.Bucket.Repo.Gem.Metadata.GitExePath,
                arguments: $@"remote set-url --push origin {gitCloneSource}",
                workingFolder: enlistmentDirectory.FullName
                ).ConfigureAwait(true))
            {
                return false;
            }

            // This is the branch that 'git push' will publish to
            // It is set to publish a branch with the same name on the remote
            if (!await mainWindow.RunCommand(
                programPath: enlistment.Bucket.Repo.Gem.Metadata.GitExePath,
                arguments: $@"config push.default current",
                workingFolder: enlistmentDirectory.FullName
                ).ConfigureAwait(true))
            {
                return false;
            }

            return true;
        }
    }
}
