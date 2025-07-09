using System.Text.Json;

namespace scrbl.Managers
{
    internal static class ConfigManager
    {
        private static readonly JsonSerializerOptions SWriteOptions = new() { WriteIndented = true };

        private static readonly string ConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "scrbl",
            "config.json");

        public static void SaveNotesPath(string path)
        {
            var fullNotesPath = Path.Combine(path, "scrbl.md");

            var config = new
            {
                NotesFilePath = fullNotesPath,
                Templates = GetDefaultTemplates()
            };

            var json = JsonSerializer.Serialize(config, SWriteOptions);
            var configDir = Path.GetDirectoryName(ConfigPath) ??
                            throw new InvalidOperationException("Could not determine config directory.");

            Directory.CreateDirectory(configDir);
            Directory.CreateDirectory(path);

            if (!File.Exists(fullNotesPath))
            {
                const string initialContent = "# 📝 My Notes\n\nWelcome to your notes! Use `scrbl write` to add new entries.\n\n";
                File.WriteAllText(fullNotesPath, initialContent);
            }

            File.WriteAllText(ConfigPath, json);
        }

        public static string LoadNotesPath()
        {
            var config = LoadConfig();
            return config.NotesFilePath;
        }

        public static Config LoadConfig()
        {
            if (!File.Exists(ConfigPath))
            {
                throw new InvalidOperationException("No config file configured.");
            }

            var json = File.ReadAllText(ConfigPath);
            var config = JsonSerializer.Deserialize<Config>(json);

            return config ?? throw new InvalidOperationException("Invalid configuration file.");
        }
        public static void SaveConfig(Config config)
        {
            var json = JsonSerializer.Serialize(config, SWriteOptions);
            var configDir = Path.GetDirectoryName(ConfigPath) ?? throw new InvalidOperationException("Could not determine config directory.");

            Directory.CreateDirectory(configDir);
            File.WriteAllText(ConfigPath, json);
        }

        public static bool IsConfigured()
        {
            try
            {
                LoadNotesPath();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static Dictionary<string, Template> GetDefaultTemplates()
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

    public class Config
    {
        public string NotesFilePath { get; set; } = string.Empty;
        public Dictionary<string, Template> Templates { get; set; } = new();
    }

    public class Template
    {
        public string Name { get; set; } = string.Empty;
        public string Header { get; set; } = string.Empty;
        public List<string> Sections { get; set; } = [];
    }
}