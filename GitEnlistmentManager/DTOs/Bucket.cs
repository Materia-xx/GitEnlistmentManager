using System.Collections.Generic;

namespace GitEnlistmentManager.DTOs
{
    public class Bucket
    {
        public Repo Repo { get; }

        public string? Name { get; set; }
        public List<Enlistment> Enlistments { get; set; } = new List<Enlistment>();

        public Bucket(Repo repo)
        {
            this.Repo = repo;
        }
    }
}
