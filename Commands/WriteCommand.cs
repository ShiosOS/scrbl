using Spectre.Console.Cli;

namespace scrbl.Commands;

public class WriteCommand : Command<CreateCommand.Settings>
{
    public class Settings : CommandSettings
    {
        
    }

    public override int Execute(CommandContext context, CreateCommand.Settings settings)
    {
        throw new NotImplementedException();
    }
}