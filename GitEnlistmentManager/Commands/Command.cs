using GitEnlistmentManager.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitEnlistmentManager.Commands
{
    public abstract class Command
    {
        public bool OpenNewWindow { get; set; }

        public string? CommandDocumentation { get; set; }

        public GemNodeContext NodeContext { get; } = new GemNodeContext();

        public abstract Task<bool> Execute();

        public virtual void ParseArgs(Stack<string> arguments)
        {
        }
    }
}
