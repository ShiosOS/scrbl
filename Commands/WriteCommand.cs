using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;

namespace scrbl.Commands;

public class WriteCommand : Command<WriteCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<CONTENT>")]
        [Description("The content to insert into notes")]
        public string? Content { get; set; }
        
        [CommandOption("-s |--section")]
        [Description("Section hearder for the new entry")]
        public string? Section { get; set; }
        
        [CommandOption("-t |--task")]
        [Description("Format as a task item")]
        public bool IsTask { get; set; }
    }
    public override int Execute(CommandContext context, Settings settings) 
    {
        if (!ConfigManager.IsConfigured())
        {
            AnsiConsole.MarkupLine("[red]No notes file configured. Run 'scrbl setup <path>' first.[/]");
            return 1;
        }

        var filePath = ConfigManager.LoadNotesPath();
        var today = DateTime.Now.ToString("yyyy.MM.dd");
        var dateHeader = $"## {today}";

        var contentToAdd = settings.IsTask ? $"- [ ] {settings.Content}" : $"* {settings.Content}";
        
        var fileContent = File.Exists(filePath) ? File.ReadAllText(filePath) : string.Empty;
        var lines = fileContent.Split(["\r\n", "\n"], StringSplitOptions.None).ToList();

        var dateIndex = -1;
        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i].Trim() != dateHeader) continue;
            dateIndex = i;
            break;
        }
        
        if (dateIndex == -1)
        {
            if (lines.Count == 0 || string.IsNullOrWhiteSpace(string.Join("", lines)))
            {
                lines.Add(dateHeader);
            }
            else
            {
                lines.Add("");
                lines.Add(dateHeader);
            }
            dateIndex = lines.Count - 1;
        }
        
        if (!string.IsNullOrEmpty(settings.Section))
        {
            var nextDateIndex = lines.FindIndex(dateIndex + 1, line => line.StartsWith("## "));
            var dateSectionEnd = nextDateIndex == -1 ? lines.Count : nextDateIndex;
            
            var sectionHeader = $"### {settings.Section}";
            var sectionIndex = -1;
            
            for (var i = dateIndex + 1; i < dateSectionEnd; i++)
            {
                if (lines[i].Trim() != sectionHeader) continue;
                sectionIndex = i;
                break;
            }
            
            if (sectionIndex == -1)
            {
                lines.Insert(dateSectionEnd, sectionHeader);
                lines.Insert(dateSectionEnd + 1, contentToAdd);
            }
            else
            {
                var nextSectionIndex = -1;
                for (var i = sectionIndex + 1; i < dateSectionEnd; i++)
                {
                    if (!lines[i].StartsWith("### ")) continue;
                    nextSectionIndex = i;
                    break;
                }
                
                var insertIndex = nextSectionIndex == -1 ? dateSectionEnd : nextSectionIndex;
                lines.Insert(insertIndex, contentToAdd);
            }
        }
        else
        {
            var insertIndex = dateIndex + 1;
            
            while (insertIndex < lines.Count && 
                   !lines[insertIndex].StartsWith("### ") && 
                   !lines[insertIndex].StartsWith("## "))
            {
                insertIndex++;
            }
            
            lines.Insert(insertIndex, contentToAdd);
        }
        
        File.WriteAllText(filePath, string.Join(Environment.NewLine, lines));
        
        var sectionText = !string.IsNullOrEmpty(settings.Section) ? $" under {settings.Section}" : "";
        AnsiConsole.MarkupLine($"[green]Added entry to {today}{sectionText}[/]");
        return 0;
    }
}