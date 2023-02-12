using GitEnlistmentManager.Extensions;
using GitEnlistmentManager.Globals;
using System.Threading.Tasks;
using System.Windows;

namespace GitEnlistmentManager.DTOs
{
    public class Enlistment : GemTreeViewItem
    {
        public Bucket Bucket { get; }

        private string? branch;
        // This isn't in extensions because it caches the branch name privately in the instance
        public async Task<string?> GetFullGitBranch()
        {
            if (this.branch == null)
            {
                var enlistmentDirectory = this.GetDirectoryInfo()?.FullName;
                if (enlistmentDirectory == null)
                {
                    MessageBox.Show("Unable to get enlistment directory information");
                    return null;
                }

                await ProgramHelper.RunProgram(
                    programPath: this.Bucket.Repo.RepoCollection?.Gem.LocalAppData.GitExePath,
                    arguments: $"branch --show-current",
                    tokens: null,
                    openNewWindow: false,
                    useShellExecute: false,
                    workingDirectory: enlistmentDirectory,
                    outputHandler: (s) =>
                    {
                        this.branch = s;
                        return Task.CompletedTask;
                    }).ConfigureAwait(false);
            }
            return this.branch;
        }

        public Enlistment(Bucket bucket)
        {
            this.Bucket = bucket;
            this.Icon = Icons.GetBitMapImage(@"enlistment.png");
        }
    }
}
