using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace scrbl
{
    internal class ScrblCli
    {
        public int Run(string[] args)
        {
            var rootcommand = BuildRootCommand();
            return rootcommand.Parse(args).Invoke();
        }

        private RootCommand BuildRootCommand()
        {
            var rootCommand = new RootCommand("Scrbl");
            rootCommand.Add(SetupCommand());
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
