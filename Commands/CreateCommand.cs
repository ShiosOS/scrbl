using System.ComponentModel;
using scrbl.Managers;
using Spectre.Console;
using Spectre.Console.Cli;

namespace scrbl.Commands
{
   
    public class CreateCommand : Command<CreateCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            [CommandOption("-d|--daily")]
            [Description("Create a daily section")]
            public bool Daily { get; set; }
            
            [CommandOption("-t|--template <TEMPLATE>")]
            [Description("Specify template name directly")]
            public string? Template { get; set; }
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            try
            {
                var templateName = GetTemplateName(settings);
                if (string.IsNullOrWhiteSpace(templateName))
                {
                    AnsiConsole.MarkupLine("[yellow]No template specified![/]\n");
                    ShowAllTemplates();
                    return 1;
                }
                
                var templateContent = TemplateManager.GenerateTemplate(templateName, DateTime.Now);
                var notesFile = new NotesFileManager(ConfigManager.LoadNotesPath());
                
                notesFile.AppendTemplate(templateContent);
                
                AnsiConsole.MarkupLine($"[green]✓[/] Template '[cyan]{templateName}[/]' added successfully!");
                
                var previewPanel = new Panel(templateContent.Trim())
                    .Header($"Added Template: {templateName}")
                    .Border(BoxBorder.Rounded)
                    .BorderColor(Color.Blue);
                
                AnsiConsole.Write(previewPanel);
                
                return 0;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]✗ Error:[/] {ex.Message}");
                return 1;
            }
        }

        private static string GetTemplateName(Settings settings)
        {
            if (settings.Daily) return "daily";
            
            if (string.IsNullOrEmpty(settings.Template)) return string.Empty;
            
            if (TemplateManager.TemplateExists(settings.Template))
            {
                return settings.Template;
            }

            AnsiConsole.MarkupLine($"[red]Template '{settings.Template}' not found![/]");
            ShowAllTemplates();
            return string.Empty;
        }

        private static void ShowAllTemplates()
        {
            AnsiConsole.MarkupLine("Available templates:");
            foreach (var templateName in TemplateManager.GetAvailableTemplateNames())
            {
                AnsiConsole.MarkupLine($"  • [cyan]{templateName}[/]");
            }
        }
    }
}