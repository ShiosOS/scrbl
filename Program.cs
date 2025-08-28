using scrbl.Commands;
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
                    .WithDescription("Create daily entry template (auto-syncs if online)");


                config.AddCommand<AppendCommand>("append")
                    .WithAlias("a")
                    .WithDescription("Add content to today's section (auto-syncs if online)");

                config.AddCommand<EditCommand>("edit")
                    .WithDescription("Edit notes in a text editor (auto-syncs if online)");

                config.AddCommand<ShowCommand>("show")
                    .WithDescription("Display recent entries from notes");

                config.AddCommand<StatusCommand>("status")
                    .WithDescription("Show file and sync status information");

                config.AddCommand<ConfigCommand>("config")
                    .WithDescription("Display current configuration");

                config.AddCommand<SyncCommand>("sync")
                    .WithDescription("Manually sync local notes with the remote server");
            });
            
            return app.Run(args);
        }
    }
}