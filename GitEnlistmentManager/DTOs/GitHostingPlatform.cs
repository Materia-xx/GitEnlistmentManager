using System.Threading.Tasks;

namespace GitEnlistmentManager.DTOs
{
    public abstract class GitHostingPlatform
    {
        /// <summary>
        /// The name of the Hosting platform
        /// </summary>
        public abstract string? Name { get; }

        public abstract Task<string?> CalculatePullRequestUrl(Enlistment enlistment);
    }
}
