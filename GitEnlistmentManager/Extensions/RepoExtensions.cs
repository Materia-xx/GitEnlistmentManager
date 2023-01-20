using GitEnlistmentManager.DTOs;
using Newtonsoft.Json;
using System;
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

        public static bool WriteMetadata(this Repo repo)
        {
            var targetRepoCollectionFolder = new DirectoryInfo(repo.RepoCollection.RepoCollectionFolderPath);

            // Write the metadata for the repo folder
            try
            {
                var repoMetadataFile = new FileInfo(Path.Combine(targetRepoCollectionFolder.FullName, $"{repo.Name}.repojson"));
                var repoMetadataJson = JsonConvert.SerializeObject(repo.Metadata, Formatting.Indented);
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
                var repoMetadata = JsonConvert.DeserializeObject<RepoMetadata>(repoMetadataJson);
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
