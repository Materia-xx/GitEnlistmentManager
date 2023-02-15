using GitEnlistmentManager.CommandSets;
using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Extensions;
using GitEnlistmentManager.Globals;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GitEnlistmentManager.Commands
{
    public class ArchiveEnlistmentCommand : Command
    {
        public ArchiveEnlistmentCommand() 
        {
            this.Documentation = "Archives an enlistment in an archive bucket in the same repo.";
        }

        public override void ParseArgs(Stack<string> arguments)
        {
            if (this.NodeContext.Bucket == null || arguments.Count == 0)
            {
                return;
            }

            var enlistmentName = arguments.Peek();
            var enlistment = this.NodeContext.Bucket.Enlistments.FirstOrDefault(e => e.GemName == enlistmentName);
            if (enlistment != null)
            {
                this.NodeContext.BaseNodeContext.Enlistment = enlistment;
                arguments.Pop();
            }
        }

        public async override Task<bool> Execute()
        {
            if (this.NodeContext.Enlistment == null)
            {
                return false;
            }

            var repoDirectory = this.NodeContext.Enlistment.Bucket.Repo.GetDirectoryInfo()?.FullName;
            if (repoDirectory == null)
            {
                return false;
            }

            var enlistmentDirectory = this.NodeContext.Enlistment.GetDirectoryInfo()?.FullName;
            if (enlistmentDirectory == null)
            {
                return false;
            }
            var enlistmentDirectoryInfo = new DirectoryInfo(enlistmentDirectory);
            var archiveDirectoryInfo = new DirectoryInfo(Path.Combine(repoDirectory, "archive"));

            if (!archiveDirectoryInfo.Exists)
            {
                archiveDirectoryInfo.Create();
            }

            // Find a spot to store the archive
            var archiveSlots = Gem.Instance.LocalAppData.ArchiveSlots;
            var archiveDirs = archiveDirectoryInfo.GetDirectories().ToList().OrderByDescending(d => d.CreationTime);
            var usedSlots = 0;
            // Recycle directories so we have at-least 1 spot free
            foreach (var archiveDir in archiveDirs)
            {
                usedSlots++;
                if (usedSlots >= archiveSlots)
                {
                    FileSystem.DeleteDirectory(archiveDir.FullName, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                }
            }

            // Figure out the next slot to use
            DirectoryInfo? archiveSlotDirectoryInfo = null;
            for (int i = 0; i < archiveSlots; i++)
            {
                archiveSlotDirectoryInfo = new DirectoryInfo(Path.Combine(archiveDirectoryInfo.FullName, i.ToString()));
                if (!archiveSlotDirectoryInfo.Exists)
                {
                    break;
                }
            }

            if (archiveSlotDirectoryInfo == null)
            {
                MessageBox.Show("Unable to find a free archive slot");
                return false;
            }

            try
            {
                var childEnlistment = this.NodeContext.Enlistment.GetChildEnlistment();
                if (!archiveSlotDirectoryInfo.Exists)
                {
                    archiveSlotDirectoryInfo.Create();
                }

                var archiveToInfo = new DirectoryInfo(Path.Combine(archiveSlotDirectoryInfo.FullName, enlistmentDirectoryInfo.Name));
                enlistmentDirectoryInfo.MoveTo(archiveToInfo.FullName);

                // Remove the enlistment from GEM config, otherwise re-parenting will just re-parent it back to the thing we just moved.
                this.NodeContext.Enlistment.Bucket.Enlistments.Remove(this.NodeContext.Enlistment);
                if (childEnlistment != null)
                {
                    // This sets the *branch* and *URL* that the enlistment will pull from
                    var setPullDetailsCommandSet = new CommandSet();
                    var setPullCommand = new GitSetPullDetailsCommand()
                    {
                        ScopeToBranch = false
                    };
                    // Change NodeContext.Enlistment focus to the child before re-parenting so it knows what enlistment needs to be re-parented
                    setPullCommand.NodeContext.BaseNodeContext.Enlistment = childEnlistment;
                    setPullDetailsCommandSet.Commands.Add(setPullCommand);
                    if (!await setPullDetailsCommandSet.RunCommandSet(this.NodeContext.BaseNodeContext).ConfigureAwait(false))
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Encountered an error archiving the enlistment: {ex}");
                return false;
            }

            return true;
        }
    }
}
