using scrbl.Managers;
using Spectre.Console;
using Spectre.Console.Cli;

namespace scrbl.Commands
{
    public class ShowCommand : Command<ShowCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            [CommandOption("-n|--lines")]
            public int Lines { get; set; } = 10;
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            if (!ConfigManager.IsConfigured())
            {
                var errorPanel = new Panel("[red]No notes file configured[/]")
                    .Header("[bold red]Configuration Error[/]")
                    .BorderStyle(new Style(Color.Red));
                AnsiConsole.Write(errorPanel);
                AnsiConsole.MarkupLine("\n[dim]Run[/] [bold cyan]scrbl setup <path>[/] [dim]to get started[/]");
                return 1;
            }

            try
            {
                var notesFile = new NotesFileManager(ConfigManager.LoadNotesPath());
                var recentLines = notesFile.GetLastLines(settings.Lines);

                if (recentLines.Length == 0)
                {
                    var emptyPanel = new Panel("[white]Your notes file is empty[/]")
                        .Header("[bold plum1]No Content[/]")
                        .BorderStyle(new Style(Color.Pink1))
                        .RoundedBorder();
                    AnsiConsole.Write(emptyPanel);
                    AnsiConsole.MarkupLine("\n[dim]Start adding notes with[/] [bold lightcyan1]scrbl \"Your note here\"[/]");
                    return 0;
                }

                var rule = new Rule($"[bold plum1]Recent Notes (Last {recentLines.Length} lines)[/]")
                    .LeftJustified()
                    .RuleStyle(new Style(Color.Pink1));
                AnsiConsole.Write(rule);
                AnsiConsole.WriteLine();

                var content = string.Join("\n", recentLines);
                var notesPanel = new Panel($"[lightcyan1]{content.EscapeMarkup()}[/]")
                    .Header("[bold plum1]Notes Content[/]")
                    .BorderStyle(new Style(Color.Pink1))
                    .RoundedBorder()
                    .Padding(1, 0, 1, 0);
                
                AnsiConsole.Write(notesPanel);

                AnsiConsole.WriteLine();
                var footerText = $"[dim]Showing last {recentLines.Length} lines. Use[/] [bold lightcyan1]--lines N[/] [dim]to show more.[/]";
                AnsiConsole.MarkupLine(footerText);

                return 0;
            }
            catch (Exception ex)
            {
                var errorPanel = new Panel($"[red1]{ex.Message}[/]")
                    .Header("[bold red1]Error[/]")
                    .BorderStyle(new Style(Color.Red1));
                AnsiConsole.Write(errorPanel);
                return 1;
            }
        }
    }
}