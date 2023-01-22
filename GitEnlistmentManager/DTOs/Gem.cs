using System.Collections.Generic;

namespace GitEnlistmentManager.DTOs
{
    public class Gem
    {
        public GemLocalAppData LocalAppData { get; set; } = new GemLocalAppData();

        public List<RepoCollection> RepoCollections { get; } = new List<RepoCollection>();

        public List<CommandSet> CommandSets { get; } = new List<CommandSet>();
    }
}
