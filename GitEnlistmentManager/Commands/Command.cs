using GitEnlistmentManager.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitEnlistmentManager.Commands
{
    public abstract class Command
    {
        public bool OpenNewWindow { get; set; }

        public string? Documentation { get; set; }

        public GemNodeContextSet NodeContext { get; } = new GemNodeContextSet();

        public abstract Task<bool> Execute();

        /// <summary>
        /// Parses arguments passed in.
        /// If this function sets a node context, it should always set the base one.
        /// </summary>
        /// <param name="arguments"></param>
        public virtual void ParseArgs(Stack<string> arguments)
        {
        }
    }
}
