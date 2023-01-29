using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitEnlistmentManager.DTOs.Commands
{
    public interface ICommand
    {
        public bool OpenNewWindow { get; set; }

        public Task<bool> Execute(GemNodeContext nodeContext, MainWindow mainWindow);

        public void ParseArgs(GemNodeContext nodeContext, Stack<string> arguments);
    }
}
