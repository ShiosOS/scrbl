using System.Text.Json;

namespace scrbl
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

            var config = new { NotesFilePath = fullNotesPath };
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

        private static string LoadNotesPath()
        {
            if (!File.Exists(ConfigPath))
            {
                throw new InvalidOperationException("No notes file configured. Run 'scrbl setup <path>' first.");
            }

            var json = File.ReadAllText(ConfigPath);
            var config = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

            return (config == null || !config.TryGetValue("NotesFilePath", out var value))
                ? throw new InvalidOperationException("Invalid configuration file")
                : value;
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
    }
}