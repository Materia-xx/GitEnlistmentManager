using GitEnlistmentManager.DTOs;
using System.Collections.Generic;
using System.IO;

namespace GitEnlistmentManager.Extensions
{
    public static class TargetBranchExtensions
    {
        /// <summary>
        /// Gets the directory for this TargetBranch. If FolderName is set, returns RepoDir/FolderName.
        /// If FolderName is not set (backward compat), returns RepoDir directly.
        /// </summary>
        public static DirectoryInfo? GetDirectoryInfo(this TargetBranch targetBranch)
        {
            var repoDirectory = targetBranch.Repo.GetDirectoryInfo();
            if (repoDirectory == null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(targetBranch.BranchDefinition.FolderName))
            {
                return repoDirectory;
            }

            var targetBranchDirectory = new DirectoryInfo(Path.Combine(repoDirectory.FullName, targetBranch.BranchDefinition.FolderName));
            if (!targetBranchDirectory.Exists)
            {
                targetBranchDirectory.Create();
            }
            return targetBranchDirectory;
        }

        public static Dictionary<string, string> GetTokens(this TargetBranch targetBranch)
        {
            var tokens = targetBranch.Repo.GetTokens();

            if (targetBranch.BranchDefinition.BranchFrom != null)
            {
                tokens["TargetBranchFrom"] = targetBranch.BranchDefinition.BranchFrom;
            }
            if (targetBranch.BranchDefinition.BranchPrefix != null)
            {
                tokens["TargetBranchPrefix"] = targetBranch.BranchDefinition.BranchPrefix;
            }
            if (targetBranch.BranchDefinition.FolderName != null)
            {
                tokens["TargetBranchFolderName"] = targetBranch.BranchDefinition.FolderName;
            }

            var targetBranchDirectory = targetBranch.GetDirectoryInfo();
            if (targetBranchDirectory != null)
            {
                tokens["TargetBranchDirectory"] = targetBranchDirectory.FullName;
            }

            return tokens;
        }
    }
}
