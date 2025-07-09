using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Spectre.Console;
using Spectre.Console.Cli;

namespace scrbl.Commands;

public class EditCommand : Command<EditCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("-e|--editor")]
        [Description("Editor to use (default: nvim)")]
        public string? Editor { get; set; }
    }

    public override int Execute(CommandContext context, Settings settings)
    {
        if (!ConfigManager.IsConfigured())
        {
            AnsiConsole.MarkupLine("[red]No notes file configured. Run 'scrbl setup <path>' first.[/]");
            return 1;
        }

        var filePath = ConfigManager.LoadNotesPath();
        var editor = settings.Editor ?? "nvim";

        try
        {
            AnsiConsole.MarkupLine($"[green]Opening {filePath}...[/]");
            
            var (fileName, arguments) = GetShellCommand(editor, filePath);
            
            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardInput = false,
                RedirectStandardOutput = false,
                RedirectStandardError = false
            };

            using var process = Process.Start(startInfo);
            
            if (process == null)
            {
                AnsiConsole.MarkupLine($"[red]Failed to start {editor}[/]");
                return 1;
            }

            process.WaitForExit();
            
            var exitCode = process.ExitCode;

            AnsiConsole.MarkupLine(exitCode == 0
                ? "[green]Notes saved successfully![/]"
                : $"[yellow]Editor exited with code {exitCode}[/]");

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error opening editor: {ex.Message}[/]");
            return 1;
        }
    }

    private static (string fileName, string arguments) GetShellCommand(string editor, string filePath)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return ("cmd.exe", $"/c {editor} \"{filePath}\"");
        }
        else
        {
            var shell = Environment.GetEnvironmentVariable("SHELL") ?? "/bin/bash";
            return (shell, $"-c '{editor} \"{filePath}\"'");
        }
    }
}