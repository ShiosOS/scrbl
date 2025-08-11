using System.ComponentModel;
using scrbl.Managers;
using Spectre.Console;
using Spectre.Console.Cli;
using System.IO;
using System;

namespace scrbl.Commands
{
    public class SetupCommand : Command<SetupCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            [CommandArgument(0, "<PATH>")]
            public string Path { get; set; } = string.Empty;

            [CommandOption("-u|--url <SERVER_URL>")]
            public string? ServerUrl { get; set; }

            // Add a new option to accept the API key from the command line.
            [CommandOption("--apikey <API_KEY>")]
            [Description("The secret API key for the remote server")]
            public string? ApiKey { get; set; }
        }

        public override int Execute(CommandContext context, Settings settings)
        {
            try
            {
                var config = ConfigManager.LoadConfig();

                config.NotesFilePath = Path.Combine(settings.Path, "scrbl.md");
                
                if (!string.IsNullOrEmpty(settings.ServerUrl))
                {
                    config.ServerUrl = settings.ServerUrl;
                }

                // Save the API key to the config if it was provided.
                if (!string.IsNullOrEmpty(settings.ApiKey))
                {
                    config.ServerApiKey = settings.ApiKey;
                }

                
                Directory.CreateDirectory(settings.Path);

                ConfigManager.SaveConfig(config);

                AnsiConsole.MarkupLine("[green]✓[/] Notes configured successfully!");
                AnsiConsole.MarkupLine($"[dim]Local Path:[/] {settings.Path}");
                if (!string.IsNullOrEmpty(config.ServerUrl))
                {
                    AnsiConsole.MarkupLine($"[dim]Server URL:[/] {config.ServerUrl}");
                }
                if (!string.IsNullOrEmpty(config.ServerApiKey))
                {
                    AnsiConsole.MarkupLine("[green]API Key has been configured.[/]");
                }
                return 0;
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]✗ Error:[/] {ex.Message}");
                return 1;
            }
        }
    }
}
