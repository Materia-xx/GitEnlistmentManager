﻿using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Extensions;
using GitEnlistmentManager.Globals;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitEnlistmentManager.Commands
{
    public class CompareToLeftSideCommand : ICommand
    {
        public bool OpenNewWindow { get; set; } = false;

        public string CommandDocumentation { get; set; } = "Selects the other side to compare to the left.";

        public void ParseArgs(GemNodeContext nodeContext, Stack<string> arguments)
        {
        }

        public async Task<bool> Execute(GemNodeContext nodeContext, MainWindow mainWindow)
        {
            if (nodeContext.Enlistment == null)
            {
                return false;
            }

            if (!CommandSetMemory.Memory.ContainsKey("LeftDirectoryCompare"))
            {
                return false;
            }

            var tokens = new Dictionary<string, string>();
            tokens["LEFT"] = CommandSetMemory.Memory["LeftDirectoryCompare"];
            var rightDirectoryCompare = nodeContext.Enlistment.GetDirectoryInfo()?.FullName;
            if (rightDirectoryCompare != null)
            {
                tokens["RIGHT"] = rightDirectoryCompare;
                await ProgramHelper.RunProgram(
                    programPath: nodeContext.Enlistment.Bucket.Repo.RepoCollection.Gem.LocalAppData.CompareProgram,
                    arguments: nodeContext.Enlistment.Bucket.Repo.RepoCollection.Gem.LocalAppData.CompareArguments,
                    tokens: tokens,
                    useShellExecute: false,
                    openNewWindow: true,
                    workingDirectory: null
                    ).ConfigureAwait(false);
            }

            CommandSetMemory.Memory.Remove("LeftDirectoryCompare");
            return await Task.FromResult(true).ConfigureAwait(false);
        }
    }
}