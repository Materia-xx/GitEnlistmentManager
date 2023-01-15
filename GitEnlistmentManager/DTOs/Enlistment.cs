namespace GitEnlistmentManager.DTOs
{
    public class Enlistment
    {
        public Bucket Bucket { get; }
        public string? Name { get; set; }

        public Enlistment(Bucket bucket)
        {
            this.Bucket = bucket;
        }
    }
}
