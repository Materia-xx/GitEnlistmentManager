using GitEnlistmentManager.CommandSets;
using GitEnlistmentManager.Extensions;
using System;
using System.Collections.Generic;

namespace GitEnlistmentManager.DTOs
{
    public class Gem
    {
        private static readonly Lazy<Gem> gem = new(() =>
        {
            var newGem = new Gem();
            newGem.ReloadSettings();
            return newGem;
        });

        public static Gem Instance => gem.Value;

        private Gem()
        {
        }

        public static List<string> LoadingErrors { get; set; } = new List<string>();

        public GemLocalAppData LocalAppData { get; set; } = new GemLocalAppData();

        public List<RepoCollection> RepoCollections { get; } = new List<RepoCollection>();

        public List<CommandSet> CommandSets { get; } = new List<CommandSet>();

        public List<Type> Commands { get; } = new List<Type>();
    }
}
