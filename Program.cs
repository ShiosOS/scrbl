using scrbl.Commands;
using scrbl.Services; // We might add a manual sync command
using Spectre.Console.Cli;

namespace scrbl
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            var app = new CommandApp();

            app.Configure(config =>
            {
                config.AddCommand<SetupCommand>("setup")
                    .WithDescription("Configure local note path and optional remote server URL");
                
                config.AddCommand<CreateCommand>("create")
                    .WithDescription("Create a templated section (auto-syncs if online)");

                config.AddCommand<WriteCommand>("write")
                    .WithAlias("w")
                    .WithDescription("Write to the notes file (auto-syncs if online)");

                config.AddCommand<EditCommand>("edit")
                    .WithDescription("Edit notes in a text editor (auto-syncs if online)");

                // It's also good practice to include a manual sync command
                config.AddCommand<ManualSyncCommand>("sync")
                    .WithDescription("Manually sync local notes with the remote server");
            });
            
            return app.Run(args);
        }
    }

    // A new command for manual syncing, in case the user wants to force it.
    public class ManualSyncCommand : AsyncCommand
    {
        public override async Task<int> ExecuteAsync(CommandContext context)
        {
            var syncService = new SyncService();
            await syncService.TrySyncAsync();
            return 0;
        }
    }
}