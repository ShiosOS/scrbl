using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using scrbl.Managers;
using Spectre.Console;
using Spectre.Console.Cli;

namespace scrbl.Commands
{
    // Inherits from AutoSyncCommand to get automatic syncing
    public class EditCommand : AutoSyncCommand<EditCommand.Settings>
    {
        public class Settings : CommandSettings
        {
            [CommandOption("-e|--editor")]
            public string? Editor { get; set; }
        }

        // The core logic is now inside ExecuteLocalAsync
        protected override Task<int> ExecuteLocalAsync(CommandContext context, Settings settings)
        {
            if (!ConfigManager.IsConfigured())
            {
                AnsiConsole.MarkupLine("[red]No notes file configured. Run 'scrbl setup <path>' first.[/]");
                return Task.FromResult(1);
            }

            var filePath = ConfigManager.LoadNotesPath();
            var editor = settings.Editor ?? "nvim";

            try
            {
                // Invalidate the local index before editing
                var notesFile = new NotesFileManager(filePath);
                notesFile.InvalidateIndex();
                
                AnsiConsole.MarkupLine($"[green]Opening {filePath} for editing...[/]");
                
                var (fileName, arguments) = GetShellCommand(editor, filePath);
                
                var startInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    UseShellExecute = false,
                };

                using var process = Process.Start(startInfo);
                process?.WaitForExit();
                
                AnsiConsole.MarkupLine("[green]✓ Edit session finished.[/]");
                return Task.FromResult(0); // Success
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error opening editor: {ex.Message}[/]");
                return Task.FromResult(1); // Failure
            }
        }

        private static (string fileName, string arguments) GetShellCommand(string editor, string filePath)
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? ("cmd.exe", $"/c {editor} \"{filePath}\"")
                : (Environment.GetEnvironmentVariable("SHELL") ?? "/bin/bash", $"-c \"{editor} '{filePath}'\"");
        }
    }
}
