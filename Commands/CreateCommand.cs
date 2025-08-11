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
        }

        protected override Task<int> ExecuteLocalAsync(CommandContext context, Settings settings)
        {
            try
            {
                string templateContent;
                if (settings.Daily)
                {
                    var date = DateTime.Now.ToString("yyyy.MM.dd");
                    templateContent = $"## {date}\n### Daily Summary\n\n";
                }
                else
                {
                    AnsiConsole.MarkupLine("[yellow]Please specify --daily flag to create a daily entry[/]");
                    return Task.FromResult(1);
                }

                var notesFile = new NotesFileManager(ConfigManager.LoadNotesPath());
                notesFile.AppendContent(templateContent);
                
                AnsiConsole.MarkupLine("[green]✓[/] Daily entry added to notes file.");
                return Task.FromResult(0);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]✗ Error:[/] {ex.Message}");
                return Task.FromResult(1);
            }
        }
    }
}
