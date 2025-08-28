using scrbl.Managers;
using scrbl.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace scrbl.Commands
{
    public class StatusCommand : AsyncCommand
    {
        public override async Task<int> ExecuteAsync(CommandContext context)
        {
            if (!ConfigManager.IsConfigured())
            {
                var errorPanel = new Panel("[red]✗ No notes file configured[/]")
                    .Header("[bold red]Configuration Error[/]")
                    .BorderStyle(new Style(Color.Red))
                    .RoundedBorder();
                AnsiConsole.Write(errorPanel);
                AnsiConsole.MarkupLine("\n[dim]Run[/] [bold cyan]scrbl setup <path>[/] [dim]to get started[/]");
                return 1;
            }

            try
            {
                var config = ConfigManager.LoadConfig();
                var notesFile = new NotesFileManager(config.NotesFilePath);
                var fileInfo = notesFile.GetFileInfo();
                var stats = notesFile.GetStatistics();

                var rule = new Rule("[bold magenta1]Scrbl Status[/]")
                    .LeftJustified()
                    .RuleStyle(new Style(Color.Pink1));
                AnsiConsole.Write(rule);
                AnsiConsole.WriteLine();

                var fileTable = new Table()
                    .BorderStyle(new Style(Color.Pink1))
                    .Border(TableBorder.Heavy);
                
                fileTable.AddColumn(new TableColumn("[bold magenta1]Property[/]"));
                fileTable.AddColumn(new TableColumn("[bold lightcyan1]Value[/]"));

                fileTable.AddRow("[plum1]Notes File[/]", $"[lightcyan1]{config.NotesFilePath}[/]");
                fileTable.AddRow("[plum1]File Exists[/]", fileInfo.Exists ? "[lightcyan1]Yes[/]" : "[white]No[/]");
                
                if (fileInfo.Exists)
                {
                    fileTable.AddRow("[plum1]File Size[/]", $"[lightcyan1]{fileInfo.Length:N0} bytes[/]");
                    fileTable.AddRow("[plum1]Last Modified[/]", $"[lightcyan1]{fileInfo.LastWriteTime:yyyy-MM-dd HH:mm:ss}[/]");
                }

                var filePanel = new Panel(fileTable)
                    .Header("[bold magenta1]File Information[/]")
                    .BorderStyle(new Style(Color.Pink1))
                    .RoundedBorder();
                AnsiConsole.Write(filePanel);
                AnsiConsole.WriteLine();

                var hasSyncConfig = !string.IsNullOrEmpty(config.ServerUrl) && !string.IsNullOrEmpty(config.ServerApiKey);
                
                var syncTable = new Table()
                    .BorderStyle(new Style(Color.LightCyan1))
                    .Border(TableBorder.Heavy);
                
                syncTable.AddColumn(new TableColumn("[bold lightcyan1]Property[/]"));
                syncTable.AddColumn(new TableColumn("[bold magenta1]Value[/]"));

                syncTable.AddRow("[lightcyan1]Sync Enabled[/]", hasSyncConfig ? "[lightcyan1]Yes[/]" : "[white]No[/]");
                
                if (hasSyncConfig)
                {
                    syncTable.AddRow("[lightcyan1]Server URL[/]", $"[plum1]{config.ServerUrl}[/]");
                    
                    string syncStatus = "[white]Testing...[/]";
                    
                    await AnsiConsole.Status()
                        .StartAsync("[dim]Testing sync connectivity...[/]", async ctx =>
                        {
                            ctx.Spinner(Spinner.Known.Dots);
                            ctx.SpinnerStyle(Style.Parse("plum1"));
                            
                            var syncService = new SyncService();
                            
                            try
                            {
                                await syncService.TrySyncAsync(showMessages: false);
                                syncStatus = "[lightcyan1]Connected[/]";
                            }
                            catch
                            {
                                syncStatus = "[plum1]Connection Failed[/]";
                            }
                        });
                    
                    syncTable.AddRow("[lightcyan1]Sync Status[/]", syncStatus);
                }

                var syncPanel = new Panel(syncTable)
                    .Header("[bold lightcyan1]Sync Configuration[/]")
                    .BorderStyle(new Style(Color.LightCyan1))
                    .RoundedBorder();
                AnsiConsole.Write(syncPanel);
                AnsiConsole.WriteLine();

                if (fileInfo.Exists)
                {
                    var statsTable = new Table()
                        .BorderStyle(new Style(Color.Pink1))
                        .Border(TableBorder.Heavy);
                    
                    statsTable.AddColumn(new TableColumn("[bold magenta1]Metric[/]"));
                    statsTable.AddColumn(new TableColumn("[bold lightcyan1]Value[/]"));

                    statsTable.AddRow("[plum1]Total Lines[/]", $"[lightcyan1]{stats["TotalLines"]:N0}[/]");
                    statsTable.AddRow("[plum1]Total Words[/]", $"[lightcyan1]{stats["TotalWords"]:N0}[/]");
                    statsTable.AddRow("[plum1]Total Characters[/]", $"[lightcyan1]{stats["TotalCharacters"]:N0}[/]");

                    var statsPanel = new Panel(statsTable)
                        .Header("[bold magenta1]Content Statistics[/]")
                        .BorderStyle(new Style(Color.Pink1))
                        .RoundedBorder();
                    AnsiConsole.Write(statsPanel);
                    AnsiConsole.WriteLine();

                    var activityTable = new Table()
                        .BorderStyle(new Style(Color.Pink1))
                        .Border(TableBorder.Heavy);
                    
                    activityTable.AddColumn(new TableColumn("[bold magenta1]Period[/]"));
                    activityTable.AddColumn(new TableColumn("[bold lightcyan1]Notes Added[/]"));

                    activityTable.AddRow("[plum1]Today[/]", $"[lightcyan1]{stats["NotesToday"]}[/]");
                    activityTable.AddRow("[plum1]This Week[/]", $"[lightcyan1]{stats["NotesThisWeek"]}[/]");
                    activityTable.AddRow("[plum1]This Month[/]", $"[lightcyan1]{stats["NotesThisMonth"]}[/]");

                    var activityPanel = new Panel(activityTable)
                        .Header("[bold magenta1]Recent Activity[/]")
                        .BorderStyle(new Style(Color.Pink1))
                        .RoundedBorder();
                    AnsiConsole.Write(activityPanel);
                }

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