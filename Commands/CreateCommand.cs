using System.ComponentModel;
using System.Threading.Tasks;
using scrbl.Managers;
using Spectre.Console;
using Spectre.Console.Cli;

namespace scrbl.Commands
{
    public class CreateCommand : AutoSyncCommand<CreateCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            [CommandOption("-d|--daily")]
            public bool Daily { get; set; }
            
            [CommandOption("-t|--template <TEMPLATE>")]
            public string? Template { get; set; }
        }

        protected override Task<int> ExecuteLocalAsync(CommandContext context, Settings settings)
        {
            try
            {
                var templateName = GetTemplateName(settings);
                if (string.IsNullOrWhiteSpace(templateName))
                {
                    AnsiConsole.MarkupLine("[yellow]No template specified![/]\n");
                    return Task.FromResult(1);
                }
                
                var templateContent = TemplateManager.GenerateTemplate(templateName, DateTime.Now);
                var notesFile = new NotesFileManager(ConfigManager.LoadNotesPath());
                
                notesFile.AppendTemplate(templateContent);
                
                AnsiConsole.MarkupLine($"[green]✓[/] Template '[cyan]{templateName}[/]' added to local file.");
                return Task.FromResult(0); // Success
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]✗ Error:[/] {ex.Message}");
                return Task.FromResult(1); // Failure
            }
        }

        private static string GetTemplateName(Settings settings)
        {
            if (settings.Daily) return "daily";
            if (string.IsNullOrEmpty(settings.Template)) return string.Empty;
            if (TemplateManager.TemplateExists(settings.Template)) return settings.Template;
            AnsiConsole.MarkupLine($"[red]Template '{settings.Template}' not found![/]");
            return string.Empty;
        }
    }
}
