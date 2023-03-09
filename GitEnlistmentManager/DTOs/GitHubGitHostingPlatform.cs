using GitEnlistmentManager.Extensions;
using System.Threading.Tasks;

namespace GitEnlistmentManager.DTOs
{
    public class GitHubGitHostingPlatform : GitHostingPlatform
    {
        public override string? Name { get; } = "GitHub";

        public override async Task<string?> CalculatePullRequestUrl(Enlistment enlistment)
        {
            // "https://github.com/Materia-xx/GitEnlistmentManager/compare/main...user/materia/b1000.init",;
            // "https://github.com/Materia-xx/GitEnlistmentManager.git",
            var pullRequestUrl = enlistment.Bucket.Repo.Metadata.CloneUrl;
            if (pullRequestUrl != null )
            {
                pullRequestUrl += "/compare/(((ParentBranch)))...(((ChildBranch)))";

                // Child branch
                pullRequestUrl = pullRequestUrl.Replace("(((ChildBranch)))", (await enlistment.GetFullGitBranch().ConfigureAwait(false)));

                // Parent branch
                var parentEnlistment = enlistment.GetParentEnlistment();
                var parentBranch = (parentEnlistment == null ? null : await parentEnlistment.GetFullGitBranch().ConfigureAwait(false)) ?? enlistment.Bucket.Repo.Metadata.BranchFrom;
                pullRequestUrl = pullRequestUrl.Replace("(((ParentBranch)))", parentBranch);
            }

            return pullRequestUrl;
        }
    }
}
