using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;

namespace GitEnlistmentManager.Commands
{
    public class ListTokensCommand : ICommand
    {
        public bool OpenNewWindow { get; set; } = false;

        public string CommandDocumentation { get; set; } = "Lists the tokens.";

        public void ParseArgs(GemNodeContext nodeContext, Stack<string> arguments)
        {
        }

        public async Task<bool> Execute(GemNodeContext nodeContext, MainWindow mainWindow)
        {
            var tokens = await nodeContext.GetTokens().ConfigureAwait(false);
            await mainWindow.ClearCommandWindow().ConfigureAwait(false);

            var tokenKeys = tokens.Keys.ToList();
            tokenKeys.Sort();

            foreach (var tokenKey in tokenKeys)
            {
                await mainWindow.AppendCommandLine($"{tokenKey} = {tokens[tokenKey]}", Brushes.AliceBlue).ConfigureAwait(false);
            }
            return true;
        }
    }
}
