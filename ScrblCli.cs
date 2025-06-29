using System.CommandLine;

namespace scrbl
{
    internal class ScrblCli
    {
        public static int Run(string[] args)
        {
            var rootcommand = BuildRootCommand();
            return rootcommand.Parse(args).Invoke();
        }

        private static RootCommand BuildRootCommand()
        {
            var rootCommand = new RootCommand("Scrbl")
            {
                SetupCommand()
            };
            return rootCommand;
        }

        private static Command SetupCommand()
        {
            var command = new Command("setup", "Configure note path");
            var pathArg = new Argument<string>("path");
            command.Arguments.Add(pathArg);

            command.SetAction(parseResult =>
            {
                var path = parseResult.GetValue(pathArg);
                Console.WriteLine($"Received path: {path}");

                if (!string.IsNullOrEmpty(path))
                {
                    try
                    {
                        ConfigManager.SaveNotesPath(path);
                        Console.WriteLine($"✓ Notes file configured: {Path.GetFullPath(path)}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("No path provided");
                }
                return 0;
            });

            return command;
        }
    }
}
