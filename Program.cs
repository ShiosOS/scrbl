using scrbl;
using System.CommandLine;

internal class Program
{
    static void Main(string[] args)
    {
        var cli = new ScrblCli();
        cli.Run(args);

        /**
         * These will add as a new bullet
         * PUT IN DSU -d
         * PUT IN SUMMARY -s
         * PUT IN TOP-LEVEL (no flag)
         * using the -f flag will open full editor
         * 
         */
    }
}
