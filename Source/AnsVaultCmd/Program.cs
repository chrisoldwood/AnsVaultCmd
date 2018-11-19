using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Mechanisms.Host;

namespace AnsVaultCmd
{
    public class Program
    {
        public static int Main(string[] args)
        {
            return AppMain(args, new IOStreams());
        }

        internal static int AppMain(string[] args, IOStreams streams)
        {
            var parser = new CommandLineParser(args, Switches);
            return Bootstrapper.Run(AppMain, parser, streams);
        }

        internal static int AppMain(Arguments arguments, IOStreams streams)
        {
            if (!arguments.IsSet(PasswordSwitch))
                throw new CmdLineException("ERROR: No vault password specified [--password]");

            string password = arguments.Value(PasswordSwitch);
            var ciphertext = new List<string>();

            if (!arguments.IsSet(InFileSwitch))
            {
                string input;

                while((input = streams.StdIn.ReadLine()) != null)
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
                streams.StdOut.Write(plaintext);
            }
            else
            {
                var outputFile = arguments.Value(OutFileSwitch);

                File.WriteAllText(outputFile, plaintext, Encoding.ASCII);
            }

            return ExitCode.Success;
        }

        private const int HelpSwitch = CommandLineParser.HelpSwitch;
        private const int VersionSwitch = CommandLineParser.VersionSwitch;
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
