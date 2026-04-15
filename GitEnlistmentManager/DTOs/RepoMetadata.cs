using System.Collections.Generic;

namespace GitEnlistmentManager.DTOs
{
    public class RepoMetadata
    {
        public string? CloneUrl { get; set; }
        public string? BranchFrom { get; set; }
        public string? BranchPrefix { get; set;}
        public string? UserEmail { get; set; }
        public string? UserName { get; set; }
        public string? GitHostingPlatformName { get; set; }
        public string? ShortName { get; set; }
        public List<BranchDefinition>? Branches { get; set; }

        /// <summary>
        /// Ensures the Branches list is populated. If Branches is null/empty but the legacy
        /// BranchFrom/BranchPrefix are set, creates a single BranchDefinition from them.
        /// </summary>
        public void NormalizeBranches()
        {
            if (Branches != null && Branches.Count > 0)
            {
                return;
            }

            Branches = new List<BranchDefinition>();
            if (!string.IsNullOrWhiteSpace(BranchFrom) || !string.IsNullOrWhiteSpace(BranchPrefix))
            {
                Branches.Add(new BranchDefinition
                {
                    BranchFrom = BranchFrom,
                    BranchPrefix = BranchPrefix,
                    FolderName = null
                });
            }
        }
    }
}
