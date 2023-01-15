using GitEnlistmentManager.DTOs;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Windows;

namespace GitEnlistmentManager.Extensions
{
    public static class GemExtensions
    {
        private const string gemMetadataFilename = "gemMetadata.json";

        public static DirectoryInfo GetAppDataFolder(this Gem _)
        {
            var gemAppDataFolder = new DirectoryInfo(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Gem"));
            if (!gemAppDataFolder.Exists)
            {
                gemAppDataFolder.Create();
            }
            return gemAppDataFolder;
        }

        public static bool WriteMetadata(this Gem gem)
        {
            var gemAppDataFolder = gem.GetAppDataFolder();

            // Write the metadata
            try
            {
                var gemMetadataFile = new FileInfo(Path.Combine(gemAppDataFolder.FullName, gemMetadataFilename));
                var gemMetadataJson = JsonConvert.SerializeObject(gem.Metadata, Formatting.Indented);
                File.WriteAllText(gemMetadataFile.FullName, gemMetadataJson);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error writing Gem metadata: {ex.Message}");
                return false;
            }
            return true;
        }

        public static bool ReadMetadata(this Gem gem)
        {
            var gemAppDataFolder = gem.GetAppDataFolder();

            // Read the metadata for the repo folder
            try
            {
                var gemMetadataFile = new FileInfo(Path.Combine(gemAppDataFolder.FullName, gemMetadataFilename));
                if (!gemMetadataFile.Exists)
                {
                    // If the metadata doesn't exist then don't default it. The user needs to update the settings
                    return false;
                }
                else
                {
                    var gemMetadataJson = File.ReadAllText(gemMetadataFile.FullName);
                    var gemMetadata = JsonConvert.DeserializeObject<GemMetadata>(gemMetadataJson);
                    if (gemMetadata == null)
                    {
                        MessageBox.Show($"Unable to deserialize Gem metadata from {gemMetadataFile.FullName}");
                        return false;
                    }
                    gem.Metadata = gemMetadata;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading Gem metadata: {ex.Message}");
                return false;
            }
            return true;
        }
    }
}
