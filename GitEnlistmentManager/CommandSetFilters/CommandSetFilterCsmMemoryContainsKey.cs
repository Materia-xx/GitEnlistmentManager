using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Globals;

namespace GitEnlistmentManager.CommandSetFilters
{
    public class CommandSetFilterCsmMemoryContainsKey : ICommandSetFilter
    {
        public string? Key { get; set; }

        public string Documentation => "True if a specific key is present in the global command set memory";

        public bool Matches(RepoCollection? repoCollection, Repo? repo, Bucket? bucket, Enlistment? enlistment)
        {
            if (Key == null)
            {
                return false;
            }

            return CommandSetMemory.Memory.ContainsKey(Key);
        }
    }
}
