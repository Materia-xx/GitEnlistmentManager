using System.Collections.Generic;

namespace GitEnlistmentManager.DTOs
{
    public class GemMetadata
    {
        public string? ReposFolder { get; set; }

        public List<string> MetadataFolders { get; } = new List<string>();

        public string GitExePath { get; set; } = @"C:\Program Files\Git\cmd\git.exe";
    }
}
