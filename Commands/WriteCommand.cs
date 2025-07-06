using System.ComponentModel;
using Spectre.Console.Cli;

namespace scrbl.Commands;

public class WriteCommand : Command<CreateCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<CONTENT>")]
        [Description("The content to insert into notes")]
        public string Content { get; set; }
        
        [CommandOption("-s |--section")]
        [Description("Section hearder for the new entry")]
        public string Section { get; set; }
        
        [CommandOption("-t |--task")]
        [Description("Format as a task item")]
        public bool IsTask { get; set; }
    }

    public override int Execute(CommandContext context, CreateCommand.Settings settings)
    {
        throw new NotImplementedException();
    }
}