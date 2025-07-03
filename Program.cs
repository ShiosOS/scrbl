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
            
            config.AddCommand<CreateCommand>("create")
                .WithDescription("Create a templated section")
                .WithExample(new[] { "create", "-d" })
                .WithExample(new[] { "create", "--template", "daily" });
        });
        
        return app.Run(args);
    }
}