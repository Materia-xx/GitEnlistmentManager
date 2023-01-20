using GitEnlistmentManager.Extensions;

namespace GitEnlistmentManager.DTOs
{
    public class GitHubGitHostingPlatform : GitHostingPlatform
    {
        public override string? Name { get; } = "GitHub";

        public override string? CalculatePullRequestUrl(Enlistment enlistment)
        {
            // "https://github.com/Materia-xx/GitEnlistmentManager/compare/main...user/materia/b1000.init",;
            // "https://github.com/Materia-xx/GitEnlistmentManager.git",
            var pullRequestUrl = enlistment.Bucket.Repo.Metadata.CloneUrl;
            if (pullRequestUrl != null )
            {
                pullRequestUrl = pullRequestUrl[..^4];
                pullRequestUrl += "/compare/(((ParentBranch)))...(((ChildBranch)))";

                // Child branch
                pullRequestUrl = pullRequestUrl.Replace("(((ChildBranch)))", enlistment.GetFullGitBranch());

                // Parent branch
                var parentEnlistment = enlistment.GetParentEnlistment();
                var parentBranch = parentEnlistment?.GetFullGitBranch() ?? enlistment.Bucket.Repo.Metadata.BranchFrom;
                pullRequestUrl = pullRequestUrl.Replace("(((ParentBranch)))", parentBranch);
            }

            return pullRequestUrl;
        }
    }
}
