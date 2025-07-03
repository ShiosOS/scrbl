namespace scrbl
{
    internal class TemplateManager
    {
        public static Dictionary<string, Template> Templates { get; set; } = new()
        {
            {
                "Daily", new Template("## {date:yyyy.MM.dd}", [ "Daily Summary" ])
                
            }
        };
        
        public static string GenerateTemplate(string templateName)
        {
            if (!Templates.TryGetValue(templateName, out var template))
                throw new Exception("Template not found");

            var output = template.Sections
                .Aggregate(
                    template.GetFormattedHeader(templateName, DateTime.Now),
                    (current, section) => current + ("\n" + "###" + section));
            
            return output + "---\n\n";
        }
    }
    
    public class Template(string header, string[] sections)
    {
        public string Header { get; set; } = header;
        public string[] Sections { get; set; } = sections;
        
        public string GetFormattedHeader(string templateName, DateTime date)
        {
            return templateName == "Daily" ? Header.Replace("{date:yyyy.MM.dd}", date.ToString("yyyy.MM.dd")) : Header;
        }
    }
    
}