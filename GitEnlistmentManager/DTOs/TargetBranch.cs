using GitEnlistmentManager.Globals;
using System.Collections.Generic;

namespace GitEnlistmentManager.DTOs
{
    public class TargetBranch : GemTreeViewItem
    {
        public Repo Repo { get; }
        public BranchDefinition BranchDefinition { get; }
        public List<Bucket> Buckets { get; } = new List<Bucket>();

        public TargetBranch(Repo repo, BranchDefinition branchDefinition)
        {
            this.Repo = repo;
            this.BranchDefinition = branchDefinition;
            this.GemName = branchDefinition.BranchFrom ?? "(no branch)";
            this.Icon = Icons.GetBitMapImage(@"branch.png");
        }
    }
}
