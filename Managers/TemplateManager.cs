namespace scrbl.Managers
{
    internal static class TemplateManager
    {
        public static string GenerateTemplate(string templateName, DateTime date)
        {
            var config = ConfigManager.LoadConfig();
            
            if (!config.Templates.TryGetValue(templateName, out var template))
            {
                throw new ArgumentException($"Template '{templateName}' not found");
            }

            var header = FormatDatePlaceholders(template.Header, date);
            var output = new List<string> { header };

            output.AddRange(template.Sections);
            output.Add("");

            return string.Join(Environment.NewLine, output) + Environment.NewLine;
        }

        public static IEnumerable<string> GetAvailableTemplateNames()
        {
            var config = ConfigManager.LoadConfig();
            return config.Templates.Keys;
        }

        public static bool TemplateExists(string templateName)
        {
            var config = ConfigManager.LoadConfig();
            return config.Templates.ContainsKey(templateName);
        }
        
        public static void AddTemplate(string name, string header, List<string>? sections = null)
        {
            var config = ConfigManager.LoadConfig();
            
            config.Templates[name] = new Template
            {
                Name = name,
                Header = header,
                Sections = sections ?? new List<string>()
            };

            ConfigManager.SaveConfig(config);
        }

        public static void RemoveTemplate(string name)
        {
            var config = ConfigManager.LoadConfig();
            
            if (config.Templates.Remove(name))
            {
                ConfigManager.SaveConfig(config);
            }
        }

        public static Template? GetTemplate(string templateName)
        {
            var config = ConfigManager.LoadConfig();
            return config.Templates.GetValueOrDefault(templateName);
        }

        private static string FormatDatePlaceholders(string input, DateTime date)
        {
            return input.Replace("{date:yyyy.MM.dd}", date.ToString("yyyy.MM.dd"));
        }
    }
}