using GitEnlistmentManager.DTOs;
using GitEnlistmentManager.Globals;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace GitEnlistmentManager.Extensions
{
    public static class RepoExtensions
    {
        public static DirectoryInfo? GetDirectoryInfo(this Repo repo)
        {
            if (string.IsNullOrWhiteSpace(repo.Name))
            {
                MessageBox.Show("Repo must have a name");
                return null;
            }

            if (string.IsNullOrWhiteSpace(repo.RepoCollection.Name))
            {
                MessageBox.Show("Metadata folder must have a name");
                return null;
            }

            if (string.IsNullOrWhiteSpace(repo.RepoCollection.Gem.LocalAppData.ReposFolder))
            {
                MessageBox.Show("Gem metadata does not have the ReposFolder set correctly");
                return null;
            }

            // Create the repos folder if it doesn't exist yet.
            var targetRepoFolder = new DirectoryInfo(Path.Combine(repo.RepoCollection.Gem.LocalAppData.ReposFolder, repo.RepoCollection.Name, repo.Name));
            if (!targetRepoFolder.Exists)
            {
                try
                {
                    targetRepoFolder.Create();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error creating repo folder: {ex.Message}");
                    return null;
                }
            }
            return targetRepoFolder;
        }

        public static Dictionary<string, string> GetTokens(this Repo repo)
        {
            var tokens = repo.RepoCollection.GetTokens();
            if (repo.Name != null)
            {
                tokens["RepoName"] = repo.Name;
            }
            if (repo.Metadata.BranchFrom != null)
            {
                tokens["RepoBranchFrom"] = repo.Metadata.BranchFrom;
            }
            if (repo.Metadata.BranchPrefix != null)
            {
                tokens["RepoBranchPrefix"] = repo.Metadata.BranchPrefix;
            }
            if (repo.Metadata.CloneUrl != null)
            {
                tokens["RepoCloneUrl"] = repo.Metadata.CloneUrl;
            }
            if (repo.Metadata.GitHostingPlatformName != null)
            {
                tokens["RepoGitHostingPlatformName"] = repo.Metadata.GitHostingPlatformName;
            }
            if (repo.Metadata.UserEmail != null)
            {
                tokens["RepoUserEmail"] = repo.Metadata.UserEmail;
            }
            if (repo.Metadata.UserName != null)
            {
                tokens["RepoUserName"] = repo.Metadata.UserName;
            }
            var repoDirectory = repo.GetDirectoryInfo();
            if (repoDirectory != null)
            {
                tokens["RepoDirectory"] = repoDirectory.FullName;
            }

            return tokens;
        }

        public static bool WriteMetadata(this Repo repo)
        {
            var targetRepoCollectionFolder = new DirectoryInfo(repo.RepoCollection.RepoCollectionFolderPath);

            // Write the metadata for the repo folder
            try
            {
                var repoMetadataFile = new FileInfo(Path.Combine(targetRepoCollectionFolder.FullName, $"{repo.Name}.repojson"));
                var repoMetadataJson = JsonConvert.SerializeObject(repo.Metadata, GemJsonSerializer.Settings);
                File.WriteAllText(repoMetadataFile.FullName, repoMetadataJson);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error writing repo metadata: {ex.Message}");
                return false;
            }
            return true;
        }

        public static bool ReadMetadata(this Repo repo, string metadataFilePath)
        { 
            try
            {
                var repoMetadataJson = File.ReadAllText(metadataFilePath);
                var repoMetadata = JsonConvert.DeserializeObject<RepoMetadata>(repoMetadataJson, GemJsonSerializer.Settings);
                if (repoMetadata == null)
                {
                    MessageBox.Show($"Unable to deserialize Repo metadata from {metadataFilePath}");
                    return false;
                }
                repo.Metadata = repoMetadata;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading repo metadata: {ex.Message}");
                return false;
            }
            return true;
        }
    }
}
