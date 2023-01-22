using System.Collections.Generic;

namespace GitEnlistmentManager.DTOs
{
    public class GemLocalAppData
    {
        public string? ReposFolder { get; set; }

        public List<string> RepoCollectionDefinitionFolders { get; } = new List<string>();

        public List<string> CommandSetFolders { get; } = new List<string>();

        public string GitExePath { get; set; } = @"C:\Program Files\Git\cmd\git.exe";
    }
}
