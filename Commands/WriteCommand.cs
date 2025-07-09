using System.ComponentModel;
using scrbl.Managers;
using Spectre.Console;
using Spectre.Console.Cli;

namespace scrbl.Commands;

public class WriteCommand : Command<WriteCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<CONTENT>")]
        [Description("The content to insert into notes")]
        public string? Content { get; set; }
        
        [CommandOption("-s|--section")]
        [Description("Section header for the new entry")]
        public string? Section { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings) 
    {
        if (!ConfigManager.IsConfigured())
        {
            AnsiConsole.MarkupLine("[red]No notes file configured. Run 'scrbl setup <path>' first.[/]");
            return 1;
        }

        try
        {
            var notesFile = new NotesFileManager(ConfigManager.LoadNotesPath());
            var content = $"* {settings.Content}";

            if (string.IsNullOrEmpty(settings.Section))
            {
                if (!notesFile.AddToLastHeader(content))
                {
                    AnsiConsole.MarkupLine($"[red]No header found. Use 'scrbl create' first.[/]");
                    return 1;
                }
            }
            else
            {
                if (!notesFile.AddToSection(settings.Section, content))
                {
                    AnsiConsole.MarkupLine($"[red]Section '{settings.Section}' not found under the current header.[/]");
                    return 1;
                }
            }

            notesFile.Save();
            ShowSuccessMessage(settings.Section);
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]✗ Error:[/] {ex.Message}");
            return 1;
        }
    }

    private static void ShowSuccessMessage(string? section)
    {
        var sectionText = !string.IsNullOrEmpty(section) ? $" under {section}" : "";
        AnsiConsole.MarkupLine($"[green]Added entry{sectionText}[/]");
    }
}