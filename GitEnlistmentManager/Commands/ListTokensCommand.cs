using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Globals;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;

namespace GitEnlistmentManager.Commands
{
    public class ListTokensCommand : Command
    {
        public ListTokensCommand()
        {
            this.CommandDocumentation = "Lists the tokens.";
        }

        public override async Task<bool> Execute()
        {
            var tokens = await this.NodeContext.GetTokens().ConfigureAwait(false);
            await Global.Instance.MainWindow.ClearCommandWindow().ConfigureAwait(false);

            var tokenKeys = tokens.Keys.ToList();
            tokenKeys.Sort();

            foreach (var tokenKey in tokenKeys)
            {
                await Global.Instance.MainWindow.AppendCommandLine($"{tokenKey} = {tokens[tokenKey]}", Brushes.AliceBlue).ConfigureAwait(false);
            }
            return true;
        }
    }
}
