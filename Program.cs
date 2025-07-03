using scrbl.Commands;
using Spectre.Console.Cli;

namespace scrbl;

internal static class Program
{
    private static int Main(string[] args)
    {
        var app = new CommandApp();

        app.Configure(config =>
        {
            config.AddCommand<SetupCommand>("setup").WithDescription("Configure note path");
        });
        
        return app.Run(args);
    }
}