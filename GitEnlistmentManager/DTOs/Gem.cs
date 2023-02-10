using System.Collections.Generic;

namespace GitEnlistmentManager.DTOs
{
    public class Gem
    {
        private static readonly Gem gem = new();
        public static Gem Instance
        {
            get { return gem; }
        }

        private Gem()
        {
        }

        public GemLocalAppData LocalAppData { get; set; } = new GemLocalAppData();

        public List<RepoCollection> RepoCollections { get; } = new List<RepoCollection>();

        public List<CommandSet> CommandSets { get; } = new List<CommandSet>();
    }
}
