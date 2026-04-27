using GitEnlistmentManager.Globals;
using System.Collections.Generic;

namespace GitEnlistmentManager.DTOs
{
    public class Bucket : GemTreeViewItem
    {
        public TargetBranch TargetBranch { get; }

        /// <summary>
        /// Convenience property to access the parent Repo through TargetBranch.
        /// </summary>
        public Repo Repo => TargetBranch.Repo;

        public List<Enlistment> Enlistments { get; set; } = new List<Enlistment>();

        public Bucket(TargetBranch targetBranch)
        {
            this.TargetBranch = targetBranch;
            this.Icon = Icons.GetBitMapImage(@"bucket.png");
        }
    }
}
