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
                .WithExample("create", "-d")
                .WithExample("create", "--template", "daily");

            config.AddCommand<WriteCommand>("write")
                .WithDescription("Write to the notes file");

            config.AddCommand<EditCommand>("edit")
                .WithDescription("Edit notes in text editor - defaults to nvim");
        });
        
        return app.Run(args);
    }
}