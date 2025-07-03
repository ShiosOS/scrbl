using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace scrbl.Commands
{
    public class SetupCommand : Command<SetupCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            [CommandArgument(0, "<PATH>")]
            [Description("Path where notes will be stored")]
            public string Path { get; set; } = string.Empty;
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            
            AnsiConsole.Write(
                new FigletText("Scrbl")
                    .LeftJustified()
                    .Color(Color.Pink1));
            if (string.IsNullOrEmpty(settings.Path))
            {
                AnsiConsole.MarkupLine("[bold red]SError:[/] Path is required");
                return 1;
            }

            try
            {
                AnsiConsole.Status()
                    .Start("Setting up notes directory...", ctx =>
                    {
                        ctx.Spinner(Spinner.Known.Star);
                        ctx.SpinnerStyle(Style.Parse("green"));

                        ConfigManager.SaveNotesPath(settings.Path);
                    });

                AnsiConsole.MarkupLine($"[green]✓[/] Notes configured successfully!");
                AnsiConsole.MarkupLine($"[dim]Location:[/] {Path.GetFullPath(settings.Path)}");

                var panel = new Panel(
                        "[yellow]Next steps:[/]\n" +
                        "• Use [cyan]scrbl write[/] to add new notes\n" +
                        "• Use [cyan]scrbl read[/] to view your notes")
                    .Header("Getting Started")
                    .Border(BoxBorder.Rounded)
                    .BorderColor(Color.Green);

                AnsiConsole.Write(panel);
                return 0;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]x Error:[/] {ex.Message}");
                return 1;
            }
        }
    }
}

    