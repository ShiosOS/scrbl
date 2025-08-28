using scrbl.Managers;
using Spectre.Console;
using Spectre.Console.Cli;

namespace scrbl.Commands
{
    public class ConfigCommand : Command
    {
        public override int Execute(CommandContext context)
        {
            try
            {
                var config = ConfigManager.LoadConfig();

                var rule = new Rule("[bold lightcyan1]Scrbl Configuration[/]")
                    .LeftJustified()
                    .RuleStyle(new Style(Color.LightCyan1));
                AnsiConsole.Write(rule);
                AnsiConsole.WriteLine();

                var table = new Table()
                    .BorderStyle(new Style(Color.Pink1))
                    .Border(TableBorder.Heavy);

                table.AddColumn(new TableColumn("[bold magenta1]Setting[/]"));
                table.AddColumn(new TableColumn("[bold lightcyan1]Value[/]"));
                table.AddColumn(new TableColumn("[bold magenta1]Status[/]"));

                var notesPathConfigured = !string.IsNullOrEmpty(config.NotesFilePath);
                table.AddRow(
                    "[plum1]Notes File[/]", 
                    notesPathConfigured ? $"[lightcyan1]{config.NotesFilePath}[/]" : "[dim]Not set[/]",
                    notesPathConfigured ? "[lightcyan1]OK[/]" : "[plum1]Missing[/]"
                );
                
                var serverConfigured = !string.IsNullOrEmpty(config.ServerUrl);
                table.AddRow(
                    "[plum1]Server URL[/]", 
                    serverConfigured ? $"[lightcyan1]{config.ServerUrl}[/]" : "[dim]Not set[/]",
                    serverConfigured ? "[lightcyan1]OK[/]" : "[white]Optional[/]"
                );
                
                var apiKeyConfigured = !string.IsNullOrEmpty(config.ServerApiKey);
                table.AddRow(
                    "[plum1]API Key[/]", 
                    apiKeyConfigured ? "[lightcyan1]****************[/]" : "[dim]Not set[/]",
                    apiKeyConfigured ? "[lightcyan1]OK[/]" : "[white]Optional[/]"
                );

                var syncEnabled = serverConfigured && apiKeyConfigured;
                table.AddRow(
                    "[plum1]Sync[/]", 
                    syncEnabled ? "[lightcyan1]Enabled[/]" : "[dim]Disabled[/]",
                    syncEnabled ? "[lightcyan1]OK[/]" : "[white]Optional[/]"
                );

                var configPanel = new Panel(table)
                    .Header("[bold magenta1]Current Configuration[/]")
                    .BorderStyle(new Style(Color.Pink1))
                    .RoundedBorder();
                
                AnsiConsole.Write(configPanel);
                
                AnsiConsole.WriteLine();
                var footerPanel = new Panel(
                    "[dim]To modify configuration:[/]\n" +
                    "[lightcyan1]scrbl setup <path>[/] [dim]- Set notes file path[/]\n" +
                    "[lightcyan1]scrbl sync setup[/] [dim]- Configure sync settings[/]"
                )
                .Header("[bold magenta1]Tips[/]")
                .BorderStyle(new Style(Color.Pink1))
                .RoundedBorder();
                
                AnsiConsole.Write(footerPanel);

                return 0;
            }
            catch (Exception ex)
            {
                var errorPanel = new Panel($"[red1]{ex.Message}[/]")
                    .Header("[bold red1]✗ Error[/]")
                    .BorderStyle(new Style(Color.Red1))
                    .RoundedBorder();
                AnsiConsole.Write(errorPanel);
                return 1;
            }
        }
    }
}