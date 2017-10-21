using System;
using System.IO;
using System.Reflection;
using Mechanisms.Host;

namespace VaultCmd
{
    public class Program
    {
        public static int Main(string[] args)
        {
            return Bootstrapper.Run(AppMain, args);
        }

        internal static int AppMain(string[] args)
        {
            return AppMain(args, Console.In, Console.Out, Console.Error);
        }

        internal static int AppMain(string[] args, TextReader stdin, TextWriter stdout, TextWriter stderr)
        {
            const int help = 1;
            const int version = 2;

            var switches = new[]
            {
                new Switch(help, "h", "help", "Show program usage"),
                new Switch(version, Switch.NoShortName, "version", "Show program version"),
            };

            var arguments = CommandLineParser.Parse(args, switches);

            if (arguments.IsSet(help))
            {
                stdout.WriteLine();
                stdout.WriteLine("USAGE: VaultCmd [options...]");
                stdout.WriteLine();
                foreach (var line in CommandLineParser.FormatSwitches(switches))
                    stdout.WriteLine(line);
                return ExitCode.Success;
            }

            if (arguments.IsSet(version))
            {
                stdout.WriteLine(Assembly.GetExecutingAssembly().GetName().Version.ToString());
                return ExitCode.Success;
            }

            return ExitCode.Failure;
        }
    }
}
