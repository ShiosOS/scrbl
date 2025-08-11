using scrbl.Managers;
using Spectre.Console;
using Spectre.Console.Cli;

namespace scrbl.Commands
{
    public class WriteCommand : AutoSyncCommand<WriteCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            [CommandArgument(0, "<CONTENT>")]
            public string? Content { get; set; }
            
            [CommandOption("-s|--section")]
            public string? Section { get; set; }
        }

        protected override Task<int> ExecuteLocalAsync(CommandContext context, Settings settings)
        {
            if (!ConfigManager.IsConfigured())
            {
                AnsiConsole.MarkupLine("[red]No notes file configured. Run 'scrbl setup <path>' first.[/]");
                return Task.FromResult(1);
            }

            try
            {
                var notesFile = new NotesFileManager(ConfigManager.LoadNotesPath());
                var content = $"* {settings.Content}\n";

                notesFile.AppendContent(content);
                AnsiConsole.MarkupLine($"[green]✓ Entry added to local notes file.[/]");
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