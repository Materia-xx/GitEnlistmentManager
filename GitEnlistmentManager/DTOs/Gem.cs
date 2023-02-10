using GitEnlistmentManager.Extensions;
using System;
using System.Collections.Generic;

namespace GitEnlistmentManager.DTOs
{
    public class Gem
    {
        private static readonly Lazy<Gem> gem = new(() =>
        {
            var gem = new Gem();
            gem.ReloadSettings();
            return gem;
        });

        public static Gem Instance => gem.Value;

        private Gem()
        {
        }

        public GemLocalAppData LocalAppData { get; set; } = new GemLocalAppData();

        public List<RepoCollection> RepoCollections { get; } = new List<RepoCollection>();

        public List<CommandSet> CommandSets { get; } = new List<CommandSet>();
    }
}
