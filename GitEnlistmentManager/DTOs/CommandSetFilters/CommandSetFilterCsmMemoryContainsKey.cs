using GitEnlistmentManager.Globals;
using System;

namespace GitEnlistmentManager.DTOs.CommandSetFilters
{
    public class CommandSetFilterCsmMemoryContainsKey : ICommandSetFilter
    {
        public string? Key { get; set; }

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
