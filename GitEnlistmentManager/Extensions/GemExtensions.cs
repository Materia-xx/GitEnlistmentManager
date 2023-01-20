using GitEnlistmentManager.DTOs;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Windows;

namespace GitEnlistmentManager.Extensions
{
    public static class GemExtensions
    {
        private const string gemLocalAppDataFilename = "GemSettings.json";

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

        public static bool WriteLocalAppData(this Gem gem)
        {
            var gemAppDataFolder = gem.GetAppDataFolder();

            // Write the metadata
            try
            {
                var gemLocalAppDataFile = new FileInfo(Path.Combine(gemAppDataFolder.FullName, gemLocalAppDataFilename));
                var gemLocalAppDataJson = JsonConvert.SerializeObject(gem.LocalAppData, Formatting.Indented);
                File.WriteAllText(gemLocalAppDataFile.FullName, gemLocalAppDataJson);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error writing Gem local application data: {ex.Message}");
                return false;
            }
            return true;
        }

        public static bool ReadLocalAppData(this Gem gem)
        {
            var gemAppDataFolder = gem.GetAppDataFolder();

            try
            {
                var gemLocalAppDataFile = new FileInfo(Path.Combine(gemAppDataFolder.FullName, gemLocalAppDataFilename));
                if (!gemLocalAppDataFile.Exists)
                {
                    // If the local app data doesn't exist then don't default it. The user needs to update the settings.
                    return false;
                }
                else
                {
                    var gemLocalAppDataJson = File.ReadAllText(gemLocalAppDataFile.FullName);
                    var gemLocalAppData = JsonConvert.DeserializeObject<GemLocalAppData>(gemLocalAppDataJson);
                    if (gemLocalAppData == null)
                    {
                        MessageBox.Show($"Unable to deserialize Gem local application data from {gemLocalAppDataFile.FullName}");
                        return false;
                    }
                    gem.LocalAppData = gemLocalAppData;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading Gem local application data: {ex.Message}");
                return false;
            }
            return true;
        }
    }
}
