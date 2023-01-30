using GitEnlistmentManager.Extensions;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GitEnlistmentManager.DTOs.Commands
{
    public class ArchiveEnlistmentCommand : ICommand
    {
        public bool OpenNewWindow { get; set; } = false;

        public void ParseArgs(GemNodeContext nodeContext, Stack<string> arguments)
        {
            if (nodeContext.Bucket == null || arguments.Count == 0)
            {
                return;
            }

            var enlistmentName = arguments.Peek();
            var enlistment = nodeContext.Bucket.Enlistments.FirstOrDefault(e => e.GemName == enlistmentName);
            if (enlistment != null)
            {
                nodeContext.Enlistment = enlistment;
                arguments.Pop();
            }
        }

        public async Task<bool> Execute(GemNodeContext nodeContext, MainWindow mainWindow)
        {
            if (nodeContext.Enlistment == null)
            {
                return false;
            }

            var repoFolder = nodeContext.Enlistment.Bucket.Repo.GetDirectoryInfo()?.FullName;
            if (repoFolder == null)
            {
                return false;
            }

            var enlistmentFolder = nodeContext.Enlistment.GetDirectoryInfo()?.FullName;
            if (enlistmentFolder == null)
            {
                return false;
            }
            var enlistmentFolderInfo = new DirectoryInfo(enlistmentFolder);
            var archiveFolderInfo = new DirectoryInfo(Path.Combine(repoFolder, "archive"));

            if (!archiveFolderInfo.Exists)
            {
                archiveFolderInfo.Create();
            }

            // Find a spot to store the archive
            var archiveSlots = nodeContext.Enlistment.Bucket.Repo.RepoCollection.Gem.LocalAppData.ArchiveSlots;
            var archiveDirs = archiveFolderInfo.GetDirectories().ToList().OrderByDescending(d => d.CreationTime);
            var usedSlots = 0;
            // Recycle folders so we have at-least 1 spot free
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
                archiveSlotDirectoryInfo = new DirectoryInfo(Path.Combine(archiveFolderInfo.FullName, i.ToString()));
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
                var childEnlistment = nodeContext.Enlistment.GetChildEnlistment();
                if (!archiveSlotDirectoryInfo.Exists)
                {
                    archiveSlotDirectoryInfo.Create();
                }

                var archiveToInfo = new DirectoryInfo(Path.Combine(archiveSlotDirectoryInfo.FullName, enlistmentFolderInfo.Name));
                enlistmentFolderInfo.MoveTo(archiveToInfo.FullName);

                // Remove the enlistment from GEM config, otherwise re-parenting will just re-parent it back to the thing we just moved.
                nodeContext.Enlistment.Bucket.Enlistments.Remove(nodeContext.Enlistment);
                if (childEnlistment != null)
                {
                    // Change NodeContext.Enlistment focus to the child before re-parenting so it knows what enlistment needs to be re-parented
                    nodeContext.Enlistment = childEnlistment;
                    // This sets the *branch* and *URL* that the enlistment will pull from
                    var setPullDetailsCommand = new GitSetPullDetails();
                    if (!await setPullDetailsCommand.Execute(nodeContext, mainWindow).ConfigureAwait(false))
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
