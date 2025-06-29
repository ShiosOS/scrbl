using System.Text.Json;

namespace scrbl
{
    internal class ConfigManager
    {
        private static readonly JsonSerializerOptions s_writeOptions = new() { WriteIndented = true };

        private static readonly string ConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "scrbl",
            "config.json");

        public static void SaveNotesPath(string path)
        {
            var fullNotesPath = Path.Combine(path, "scrbl.md");

            var config = new { NotesFilePath = fullNotesPath };
            var json = JsonSerializer.Serialize(config, s_writeOptions);
            var configDir = Path.GetDirectoryName(ConfigPath) ?? throw new InvalidOperationException("Could not determine config directory.");
            Directory.CreateDirectory(configDir);

            Directory.CreateDirectory(path);

            if (!File.Exists(fullNotesPath))
            {
                var initialContent = "# Notes\n\n";
                File.WriteAllText(fullNotesPath, initialContent);
            }

            File.WriteAllText(ConfigPath, json);
        }

        public static string LoadNotesPath()
        {
            if (!File.Exists(ConfigPath))
            {
                throw new InvalidOperationException("No file configued, Run setup command");
            }

            var json = File.ReadAllText(ConfigPath);
            var config = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            return  (config == null || !config.ContainsKey("NotesFile"))
                ? throw new InvalidOperationException("Invalid configuration file")
                : config["NotesFilePath"];
        }
    }
}
