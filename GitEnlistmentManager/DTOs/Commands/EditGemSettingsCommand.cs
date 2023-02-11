using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitEnlistmentManager.DTOs.Commands
{
    public class EditGemSettingsCommand : ICommand
    {
        public bool OpenNewWindow { get; set; } = false;

        public string CommandDocumentation { get; set; } = "Opens the Gem Settings editor.";

        public void ParseArgs(GemNodeContext nodeContext, Stack<string> arguments)
        {
        }

        public async Task<bool> Execute(GemNodeContext nodeContext, MainWindow mainWindow)
        {
            bool? result = null;
            await mainWindow.Dispatcher.InvokeAsync(() =>
            {
                var gemSettingsEditor = new GemSettings(Gem.Instance);
                result = gemSettingsEditor.ShowDialog();
            });
            return (result.HasValue && result.Value);
        }
    }
}
