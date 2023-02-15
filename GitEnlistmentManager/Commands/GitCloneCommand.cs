using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Extensions;
using GitEnlistmentManager.Globals;
using System.Threading.Tasks;
using System.Windows;

namespace GitEnlistmentManager.Commands
{
    public class GitCloneCommand : Command
    {
        public GitCloneCommand()
        {
            this.Documentation = "Clones a branch of choice.";
        }

        public string? CloneUrl { get; set; }

        /// <summary>
        /// When cloning this is the branch that will be cloned from
        /// </summary>
        public string? BranchFrom { get; set; }

        public bool ScopeToBranch { get; set; }

        public bool GitAutoCrlf { get; set; }

        public override async Task<bool> Execute()
        {
            if (this.NodeContext.Enlistment == null || this.NodeContext.Bucket == null || this.NodeContext.Repo == null || this.NodeContext.RepoCollection == null)
            {
                return false;
            }
            if (string.IsNullOrWhiteSpace(CloneUrl))
            {
                return false;
            }

            var enlistmentDirectory = this.NodeContext.Enlistment.GetDirectoryInfo();
            if (enlistmentDirectory == null)
            {
                MessageBox.Show("Encountered an error getting the enlistment directory.");
                return false;
            }

            var bucketDirectory = this.NodeContext.Bucket.GetDirectoryInfo();
            if (bucketDirectory == null)
            {
                MessageBox.Show("Encountered an error getting the bucket directory.");
                return false;
            }

            // The intention is that a branch will never be changed to a different branch in these enlistments
            // So we set --depth 1 to save some time/space. But this only works when cloning from the
            // remote repo and not a local parent directory.
            var gitShallowOption = CloneUrl == this.NodeContext.Repo.Metadata.CloneUrl ? "--depth 1" : string.Empty;

            var branchFrom = string.IsNullOrWhiteSpace(BranchFrom)
                ? this.NodeContext.Repo.Metadata.BranchFrom
                : BranchFrom;
            branchFrom = this.ScopeToBranch ? $"--branch {branchFrom}" : string.Empty;

            var gitAutoCrlfOption = $"--config core.autocrlf={this.GitAutoCrlf}";

            // Note: Git on the commandline adds progress lines that are not included in redirected output.
            //       It is possible to add --progress to see them, but because this program is not a true
            //       command terminal it spams many lines instead of keeping the progress on one line.
            //       I've left the option out for that reason.
            if (!await Global.Instance.MainWindow.RunProgram(
                programPath: Gem.Instance.LocalAppData.GitExePath,
                arguments: $"clone {gitShallowOption} {branchFrom} {gitAutoCrlfOption} {CloneUrl} \"{enlistmentDirectory.FullName}\"",
                tokens: null, // There are no tokens in the above programPath/arguments - If we did supply tokens here it would supply an invalid enlistmentBranch because it's not made yet.
                workingDirectory: bucketDirectory.FullName
                ).ConfigureAwait(false))
            {
                return false;
            }

            return true;
        }
    }
}
