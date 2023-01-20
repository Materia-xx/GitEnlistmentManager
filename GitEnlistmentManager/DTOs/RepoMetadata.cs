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
    }
}
