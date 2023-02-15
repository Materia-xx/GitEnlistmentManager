using GitEnlistmentManager.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitEnlistmentManager.Commands
{
    public abstract class Command
    {
        public bool Executed { get; private set; }

        public bool OpenNewWindow { get; set; }

        public string? Documentation { get; set; }

        public GemNodeContext NodeContext { get; } = new GemNodeContext();

        public abstract Task<bool> Execute();

        public void MarkAsExecuted()
        {
            this.Executed = true;
        }

        /// <summary>
        /// Parses any command line arguments
        /// </summary>
        /// <param name="arguments"></param>
        public virtual void ParseArgs(Stack<string> arguments)
        {
        }
    }
}
