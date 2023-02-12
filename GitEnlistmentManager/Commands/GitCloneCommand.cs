using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Extensions;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace GitEnlistmentManager.Commands
{
    public class GitCloneCommand : ICommand
    {
        public bool OpenNewWindow { get; set; } = false;

        public string CommandDocumentation { get; set; } = "Clones a branch of choice.";

        public string? CloneUrl { get; set; }

        /// <summary>
        /// When cloning this is the branch that will be cloned from
        /// </summary>
        public string? BranchFrom { get; set; }

        /// <summary>
        /// This is the branch that it will pull from when you type 'git pull'
        /// </summary>
        public string? PullFrom { get; set; }

        public void ParseArgs(GemNodeContext nodeContext, Stack<string> arguments)
        {
        }

        public async Task<bool> Execute(GemNodeContext nodeContext, MainWindow mainWindow)
        {
            if (nodeContext.Enlistment == null || nodeContext.Bucket == null || nodeContext.Repo == null || nodeContext.RepoCollection == null)
            {
                return false;
            }
            if (string.IsNullOrWhiteSpace(CloneUrl))
            {
                return false;
            }

            var enlistmentDirectory = nodeContext.Enlistment.GetDirectoryInfo();
            if (enlistmentDirectory == null)
            {
                MessageBox.Show("Encountered an error getting the enlistment directory.");
                return false;
            }

            var bucketDirectory = nodeContext.Bucket.GetDirectoryInfo();
            if (bucketDirectory == null)
            {
                MessageBox.Show("Encountered an error getting the bucket directory.");
                return false;
            }

            // The intention is that a branch will never be changed to a different branch in these enlistments
            // So we set --depth 1 to save some time/space. But this only works when cloning from the
            // remote repo and not a local parent directory.
            var gitShallowOption = CloneUrl == nodeContext.Repo.Metadata.CloneUrl ? "--depth 1" : string.Empty;

            var branchFrom = string.IsNullOrWhiteSpace(BranchFrom)
                ? nodeContext.Repo.Metadata.BranchFrom
                : BranchFrom;
            branchFrom = $"--branch {branchFrom}";

            var gitAutoCrlfOption = "--config core.autocrlf=false";

            // Note: Git on the commandline adds progress lines that are not included in redirected output.
            //       It is possible to add --progress to see them, but because this program is not a true
            //       command terminal it spams many lines instead of keeping the progress on one line.
            //       I've left the option out for that reason.
            if (!await mainWindow.RunProgram(
                programPath: nodeContext.RepoCollection.Gem.LocalAppData.GitExePath,
                arguments: $"clone {gitShallowOption} {branchFrom} {gitAutoCrlfOption} {CloneUrl} \"{enlistmentDirectory.FullName}\"",
                tokens: null, // There are no tokens in the above programPath/arguments - If we did supply tokens here it would supply an invalid enlistmentBranch because it's not made yet.
                workingDirectory: bucketDirectory.FullName
                ).ConfigureAwait(false))
            {
                return false;
            }

            // using --branch x will get you a local enlistment that is checked out to branch x, but it also has the downside of setting the config 
            // up in a way where it will only be able to fetch from the x branch too. This ends up so that later on when we tell the branch to pull
            // from the master branch instead of the x branch, it is unable to do so because it can only see x in the remote due to the fetch settings
            PullFrom ??= "*";
            var fetchSettings = $"+refs/heads/{PullFrom}:refs/remotes/origin/{PullFrom}";

            if (!await mainWindow.RunProgram(
                programPath: nodeContext.RepoCollection.Gem.LocalAppData.GitExePath,
                arguments: $"config remote.origin.fetch {fetchSettings}",
                tokens: null, // There are no tokens in the above programPath/arguments - If we did supply tokens here it would supply an invalid enlistmentBranch because it's not made yet.
                workingDirectory: enlistmentDirectory.FullName
                ).ConfigureAwait(false))
            {
                return false;
            }
            return true;
        }
    }
}
