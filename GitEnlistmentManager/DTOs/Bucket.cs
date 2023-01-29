using GitEnlistmentManager.Globals;
using System.Collections.Generic;

namespace GitEnlistmentManager.DTOs
{
    public class Bucket : GemTreeViewItem
    {
        public Repo Repo { get; }

        public string? Name { get; set; }
        public List<Enlistment> Enlistments { get; set; } = new List<Enlistment>();

        public Bucket(Repo repo)
        {
            this.Repo = repo;
            this.Icon = Icons.GetBitMapImage(@"bucket.png");
        }
    }
}
