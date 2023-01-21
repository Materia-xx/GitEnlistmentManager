using System.Collections.Generic;

namespace GitEnlistmentManager.DTOs
{
    public class GitHostingPlatforms
    {
        public List<GitHostingPlatform> Platforms { get; } = new List<GitHostingPlatform>()
        {
            new GitHubGitHostingPlatform(),
            new AzureDevOpsGitHostingPlatform()
        };

        public static GitHostingPlatforms Instance { get; } = new GitHostingPlatforms();

        private GitHostingPlatforms()
        {
        }
    }
}
