using System.Collections.Generic;

namespace GitEnlistmentManager.DTOs
{
    public class GemLocalAppData
    {
        public string? ReposDirectory { get; set; }

        public List<string> RepoCollectionDefinitionDirectories { get; } = new List<string>();

        public List<string> CommandSetDirectories { get; } = new List<string>();

        public string GitExePath { get; set; } = @"C:\Program Files\Git\cmd\git.exe";

        public int ArchiveSlots { get; set; } = 10;

        public int EnlistmentIncrement { get; set; } = 2000;

        public int ServerPort { get; set; } = 8397;

        public string? CompareProgram { get; set; }

        public string? CompareArguments { get; set; }
    }
}
