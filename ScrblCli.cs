using System.CommandLine;
using Spectre.Console;

namespace scrbl
{
    internal class ScrblCli
    {
        public static int Run(string[] args)
        {
            var rootCommand = BuildRootCommand();
            return rootCommand.Parse(args).Invoke();
        }

        private static RootCommand BuildRootCommand()
        {
            var rootCommand = new RootCommand("Scrbl")
            {
                SetupCommand()
            };
            return rootCommand;
        }

        private static Command SetupCommand()
        {
            var command = new Command("setup", "Configure note path");
            var pathArg = new Argument<string>("path");
            command.Arguments.Add(pathArg);

            command.SetAction(parseResult =>
            {
                var path = parseResult.GetValue(pathArg);
                
                AnsiConsole.Write(
                    new FigletText("Scrbl")
                        .LeftJustified()
                        .Color(Color.Pink1));

                if (!string.IsNullOrEmpty(path))
                {
                    try
                    {
                        AnsiConsole.Status()
                            .Start("Setting up notes directory...", ctx =>
                            {
                                ctx.Spinner(Spinner.Known.Star);
                                ctx.SpinnerStyle(Style.Parse("green"));
                                
                                ConfigManager.SaveNotesPath(path);
                            });

                        AnsiConsole.MarkupLine($"[green]✓[/] Notes configured successfully!");
                        AnsiConsole.MarkupLine($"[dim]Location:[/] {Path.GetFullPath(path)}");
                        
                        var panel = new Panel(
                            "[yellow]Next steps:[/]\n" +
                            "• Use [cyan]scrbl write[/] to add new notes\n" +
                            "• Use [cyan]scrbl read[/] to view your notes")
                            .Header("Getting Started")
                            .Border(BoxBorder.Rounded)
                            .BorderColor(Color.Green);
                        
                        AnsiConsole.Write(panel);
                    }
                    catch (Exception ex)
                    {
                        AnsiConsole.MarkupLine($"[red]✗ Error:[/] {ex.Message}");
                        return 1;
                    }
                }
                else
                {
                    AnsiConsole.MarkupLine("[yellow]⚠[/] No path provided");
                    return 1;
                }
                return 0;
            });

            return command;
        }
    }
}
