using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace scrbl
{
    internal class ConfigManager
    {
        private static readonly string ConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "scrbl",
            "config.json");

        public static void SaveNotesPath(string path)
        {
            var fullNotesPath = Path.Combine(path, "scrbl.md");

            var config = new { NotesFilePath = fullNotesPath };
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });

            Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath));

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
            return config["NotesFilePath"];
        }
    }
}
