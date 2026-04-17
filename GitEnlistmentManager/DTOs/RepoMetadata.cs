using System.Collections.Generic;

namespace GitEnlistmentManager.DTOs
{
    public class RepoMetadata
    {
        public string? CloneUrl { get; set; }
        public string? UserEmail { get; set; }
        public string? UserName { get; set; }
        public string? GitHostingPlatformName { get; set; }
        public string? ShortName { get; set; }
        public List<BranchDefinition> Branches { get; set; } = new List<BranchDefinition>();
    }
}
