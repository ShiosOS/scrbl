using scrbl.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace scrbl.Commands
{
    public class SyncCommand : AsyncCommand
    {
        public override async Task<int> ExecuteAsync(CommandContext context)
        {
            try
            {
                var syncService = new SyncService();
                await syncService.TrySyncAsync(showMessages: true);
                return 0;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Sync failed: {ex.Message}[/]");
                return 1;
            }
        }
    }
}