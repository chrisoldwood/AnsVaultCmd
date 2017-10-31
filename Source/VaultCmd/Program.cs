using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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
            var arguments = CommandLineParser.Parse(args, Switches);

            if (arguments.IsSet(HelpSwitch))
            {
                ShowUsage(stdout);
                return ExitCode.Success;
            }

            if (arguments.IsSet(VersionSwitch))
            {
                stdout.WriteLine(Assembly.GetExecutingAssembly().GetName().Version.ToString());
                return ExitCode.Success;
            }

            if (!arguments.IsSet(PasswordSwitch))
            {
                stdout.WriteLine("ERROR: No vault password specified [--password]");
                ShowUsage(stdout);
                return ExitCode.Failure;
            }

            string password = arguments.Value(PasswordSwitch);
            var ciphertext = new List<string>();

            if (!arguments.IsSet(InFileSwitch))
            {
                string input;

                while((input = stdin.ReadLine()) != null)
                    ciphertext.Add(input);
            }
            else
            {
                var inputFile = arguments.Value(InFileSwitch);

                ciphertext = File.ReadAllLines(inputFile)
                                 .ToList();
            }

            var plaintext = Decrypter.Decypt(ciphertext, password);

            if (!arguments.IsSet(OutFileSwitch))
            {
                stdout.Write(plaintext);
            }
            else
            {
                var outputFile = arguments.Value(OutFileSwitch);

                File.WriteAllText(outputFile, plaintext, Encoding.ASCII);
            }

            return ExitCode.Success;
        }

        private static void ShowUsage(TextWriter stdout)
        {
            stdout.WriteLine();
            stdout.WriteLine("USAGE: VaultCmd [options...]");
            stdout.WriteLine();
            foreach (var line in CommandLineParser.FormatSwitches(Switches))
                stdout.WriteLine(line);
        }

        private const int HelpSwitch = 1;
        private const int VersionSwitch = 2;
        private const int PasswordSwitch = 3;
        private const int InFileSwitch = 4;
        private const int OutFileSwitch = 5;

        private static readonly Switch[] Switches = new[]
        {
            new Switch(HelpSwitch, "h", "help", "Show program usage"),
            new Switch(VersionSwitch, Switch.NoShortName, "version", "Show program version"),
            new Switch(PasswordSwitch, "p", "password", Switch.ValueType.Value, "The vault password"),
            new Switch(InFileSwitch, "i", "infile", Switch.ValueType.Value, "The encypted input file (overrides stdin)"),
            new Switch(OutFileSwitch, "o", "outfile", Switch.ValueType.Value, "The plaintext output file (overrides stdout)"),
        };
    }
}
