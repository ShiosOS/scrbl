using System.Text.Json;
using System.IO;
using System.Collections.Generic;

namespace scrbl.Managers
{
    public class Config
    {
        public string NotesFilePath { get; set; } = string.Empty;
        public string ServerUrl { get; set; } = string.Empty;
        public string ServerApiKey { get; set; } = string.Empty;
        public Dictionary<string, Template> Templates { get; set; } = new();
    }

    public class Template
    {
        public string Name { get; set; } = string.Empty;
        public string Header { get; set; } = string.Empty;
        public List<string> Sections { get; set; } = [];
    }

    internal static class ConfigManager
    {
        private static readonly JsonSerializerOptions SWriteOptions = new() { WriteIndented = true };
        private static readonly string ConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "scrbl", "config.json");

        public static Config LoadConfig()
        {
            if (!File.Exists(ConfigPath))
            {
                // If no config exists, return a fresh, empty one.
                return new Config();
            }
            var json = File.ReadAllText(ConfigPath);
            return JsonSerializer.Deserialize<Config>(json) ?? new Config();
        }

        public static void SaveConfig(Config config)
        {
            var json = JsonSerializer.Serialize(config, SWriteOptions);
            var configDir = Path.GetDirectoryName(ConfigPath);
            if(configDir != null)
            {
                 Directory.CreateDirectory(configDir);
            }
            File.WriteAllText(ConfigPath, json);
        }

        public static string LoadNotesPath() => LoadConfig().NotesFilePath;
        public static string LoadServerUrl() => LoadConfig().ServerUrl;
        public static bool IsConfigured() => !string.IsNullOrEmpty(LoadNotesPath());

        public static Dictionary<string, Template> GetDefaultTemplates()
        {
            return new Dictionary<string, Template>
            {
                ["daily"] = new Template
                {
                    Name = "daily",
                    Header = "## {date:yyyy.MM.dd}",
                    Sections = new List<string> { "### Daily Summary" }
                }
            };
        }
    }
}
