using GitEnlistmentManager.DTOs;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Windows;

namespace GitEnlistmentManager.Extensions
{
    public static class RepoExtensions
    {
        private const string repoMetadataFilename = "repoMetadata.json";

        public static DirectoryInfo? GetDirectoryInfo(this Repo repo)
        {
            if (string.IsNullOrWhiteSpace(repo.Name))
            {
                MessageBox.Show("Repo must have a name");
                return null;
            }

            if (string.IsNullOrWhiteSpace(repo.Gem.Metadata.ReposFolder))
            {
                MessageBox.Show("Gem metadata does not have the ReposFolder set correctly");
                return null;
            }

            // Create the repos folder if it doesn't exist yet.
            var targetRepoFolder = new DirectoryInfo(Path.Combine(repo.Gem.Metadata.ReposFolder, repo.Name));
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
            var targetRepoFolder = repo.GetDirectoryInfo();
            if (targetRepoFolder == null)
            {
                return false;
            }

            // Write the metadata for the repo folder
            try
            {
                var repoMetadataFile = new FileInfo(Path.Combine(targetRepoFolder.FullName, repoMetadataFilename));
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

        public static bool ReadMetadata(this Repo repo)
        {
            var targetRepoFolder = repo.GetDirectoryInfo();
            if (targetRepoFolder == null)
            {
                return false;
            }

            // Read the metadata for the repo folder
            try
            {
                var repoMetadataFile = new FileInfo(Path.Combine(targetRepoFolder.FullName, repoMetadataFilename));
                if (!repoMetadataFile.Exists)
                {
                    repo.Metadata = new();
                }
                else
                {
                    var repoMetadataJson = File.ReadAllText(repoMetadataFile.FullName);
                    var repoMetadata = JsonConvert.DeserializeObject<RepoMetadata>(repoMetadataJson);
                    if (repoMetadata == null)
                    {
                        MessageBox.Show($"Unable to deserialize Repo metadata from {repoMetadataFile.FullName}");
                        return false;
                    }
                    repo.Metadata = repoMetadata;
                }
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
