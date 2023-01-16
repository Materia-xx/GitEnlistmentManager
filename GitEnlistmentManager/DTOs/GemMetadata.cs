namespace GitEnlistmentManager.DTOs
{
    public class GemMetadata
    {
        public string? ReposFolder { get; set; }

        public string GitExePath { get; set; } = @"C:\Program Files\Git\cmd\git.exe";
    }
}
