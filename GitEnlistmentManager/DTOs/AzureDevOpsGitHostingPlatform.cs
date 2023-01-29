using GitEnlistmentManager.Extensions;
using System.Threading.Tasks;

namespace GitEnlistmentManager.DTOs
{
    public class AzureDevOpsGitHostingPlatform : GitHostingPlatform
    {
        public override string? Name { get; } = "AzureDevOps";

        public override async Task<string?> CalculatePullRequestUrl(Enlistment enlistment)
        {
            // This is the final pull request URL, but you can leave out the Repository Ids and it will default them to the current repo
            // "https://dev.azure.com/(((OrgName)))/(((ProjectName)))/_git/(((RepoName)))/pullrequestcreate?sourceRef=(((ChildBranch)))&targetRef=(((ParentBranch)))&sourceRepositoryId=(((SourceRepoId)))&targetRepositoryId=(((TargetRepoId)))"
            var pullRequestUrl = enlistment.Bucket.Repo.Metadata.CloneUrl;
            if (pullRequestUrl != null)
            {
                pullRequestUrl += "/pullrequestcreate?sourceRef=(((ChildBranch)))&targetRef=(((ParentBranch)))";


                // Child branch
                pullRequestUrl = pullRequestUrl.Replace("(((ChildBranch)))", (await enlistment.GetFullGitBranch().ConfigureAwait(false)));

                // Parent branch
                var parentEnlistment = enlistment.GetParentEnlistment();
                if (parentEnlistment != null)
                {
                    var parentBranch = (await parentEnlistment.GetFullGitBranch().ConfigureAwait(false)) ?? enlistment.Bucket.Repo.Metadata.BranchFrom;
                    pullRequestUrl = pullRequestUrl.Replace("(((ParentBranch)))", parentBranch);
                }
            }

            return pullRequestUrl;
        }
    }
}
