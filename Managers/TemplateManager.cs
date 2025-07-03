namespace scrbl
{
    internal class TemplateManager
    {
        public static Dictionary<string, TemplateInfo> Templates { get; set; } = new()
        {
            ["daily"] = new TemplateInfo
            {
                Template = new Template
                {
                    Header = "## {date:yyyy.MM.dd}",
                    Sections = new[] 
                    { 
                        "### Daily Summary"
                    }
                },
                Description = "Daily planning template",
                Flags = new[] { "-d", "--daily" }
            }
        };

        public static string GenerateTemplate(string templateName, DateTime date)
        {
            if (!Templates.TryGetValue(templateName, out var templateInfo))
            {
                throw new ArgumentException($"Template '{templateName}' not found");
            }

            var template = templateInfo.Template;
            var header = FormatDatePlaceholders(template.Header, date);
            var output = new List<string> { header };

            output.AddRange(template.Sections);
            output.Add("");

            return string.Join("\n", output) + "\n";
        }

        public static IEnumerable<string> GetAvailableTemplateNames()
        {
            return Templates.Keys;
        }

        public static IEnumerable<(string flags, string name, string description)> GetTemplateDisplayInfo()
        {
            return Templates.Select(kvp => (
                flags: string.Join(", ", kvp.Value.Flags),
                name: kvp.Key,
                description: kvp.Value.Description
            ));
        }

        public static bool TemplateExists(string templateName)
        {
            return Templates.ContainsKey(templateName);
        }

        private static string FormatDatePlaceholders(string input, DateTime date)
        {
            return input.Replace("{date:yyyy.MM.dd}", date.ToString("yyyy.MM.dd"));
        }

        public class Template
        {
            public string Header { get; set; } = string.Empty;
            public string[] Sections { get; set; } = Array.Empty<string>();
        }

        public class TemplateInfo
        {
            public Template Template { get; set; } = new();
            public string Description { get; set; } = string.Empty;
            public string[] Flags { get; set; } = Array.Empty<string>();
        }
    }
}