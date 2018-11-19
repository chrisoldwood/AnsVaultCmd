using System.IO;
using System.Text;
using Mechanisms.Extensions;
using Mechanisms.Host;
using Mechanisms.Tests;

namespace Tests
{
    public static class ProgramTests
    {
        [TestCases]
        public static void help_switch()
        {
            "Requesting help returns 0".Is(() =>
            {
                var helpArg = new[] { "--help" };

                var exitCode = AnsVaultCmd.Program.AppMain(helpArg, AnyStreams);

                Assert.True(exitCode == 0);
            });

            "Requesting help outputs a usage message".Is(() =>
            {
                var helpArg = new[] { "--help" };
                var stdOut = new StringWriter();

                AnsVaultCmd.Program.AppMain(helpArg, new IOStreams(AnyStdIn, stdOut, AnyStdErr));

                var output = stdOut.ToString();
                Assert.True(output.Contains("USAGE"));
                Assert.True(output.Contains("--help"));
            });

            "Requesting help lists the switches".Is(() =>
            {
                var helpArg = new[] { "--help" };
                var stdOut = new StringWriter();

                AnsVaultCmd.Program.AppMain(helpArg, new IOStreams(AnyStdIn, stdOut, AnyStdErr));

                var output = stdOut.ToString();
                Assert.True(output.Contains("-h | --help"));
                Assert.True(output.Contains("--version"));
                Assert.True(output.Contains("-p | --password"));
                Assert.True(output.Contains("-i | --infile"));
                Assert.True(output.Contains("-o | --outfile"));
            });
        }

        [TestCases]
        public static void version_switch()
        {
            "Requesting the version returns 0".Is(() =>
            {
                var versionArg = new[] { "--version" };

                var exitCode = AnsVaultCmd.Program.AppMain(versionArg, AnyStreams);

                Assert.True(exitCode == 0);
            });

            "Requesting the version outputs a version string".Is(() =>
            {
                var versionArg = new[] { "--version" };
                var stdOut = new StringWriter();

                AnsVaultCmd.Program.AppMain(versionArg, new IOStreams(AnyStdIn, stdOut, AnyStdErr));

                var output = stdOut.ToString();
                Assert.True(output.Contains("0.0.0.1"));
            });
        }

        [TestCases]
        public static void decryption()
        {
            "A missing password returns non-zero".Is(() =>
            {
                var noArgs = new string[0];

                var exitCode = AnsVaultCmd.Program.AppMain(noArgs, AnyStreams);

                Assert.True(exitCode != 0);
            });

            "A missing password outputs an error and usage message".Is(() =>
            {
                var noArgs = new string[0];
                var stdOut = new StringWriter();
                var stdErr = new StringWriter();

                AnsVaultCmd.Program.AppMain(noArgs, new IOStreams(AnyStdIn, stdOut, stdErr));

                Assert.True(stdErr.ToString().Contains("ERROR:"));
                Assert.True(stdOut.ToString().Contains("USAGE"));
            });

            "A valid password and ciphertext returns non-zero".Is(() =>
            {
                var passwordOnlyArgs = new[] { "--password", ValidPassword };
                var stdIn = CreateCiphertextInput();

                var exitCode = AnsVaultCmd.Program.AppMain(passwordOnlyArgs, new IOStreams(stdIn, AnyStdOut, AnyStdErr));

                Assert.True(exitCode == 0);
            });

            "A valid password and ciphertext writes to stdout by default".Is(() =>
            {
                var passwordOnlyArgs = new[] { "--password", ValidPassword };
                var stdIn = CreateCiphertextInput();
                var stdOut = new StringWriter();

                AnsVaultCmd.Program.AppMain(passwordOnlyArgs, new IOStreams(stdIn, stdOut, AnyStdErr));

                var output = stdOut.ToString();
                Assert.True(output.Contains(ValidPlainText));
            });

            "The ciphertext can be read from a file instead".Is(() =>
            {
                var inputFile = CreateCiphertextTestFile();

                try
                {
                    var args = new[] { "--password", ValidPassword, "--infile", inputFile };
                    var emptyStdIn = new StringReader("");
                    var stdOut = new StringWriter();

                    AnsVaultCmd.Program.AppMain(args, new IOStreams(emptyStdIn, stdOut, AnyStdErr));

                    var output = stdOut.ToString();
                    Assert.True(output.Contains(ValidPlainText));
                }
                finally
                {
                    File.Delete(inputFile);
                }
            });

            "An invalid ciphertext file generates an error".Is(() =>
            {
                var inputFile = CreateMissingTempFile();
                var args = new[] { "--password", ValidPassword, "--infile", inputFile };
                var emptyStdIn = new StringReader("");
                var stdErr = new StringWriter();

                var exitCode = AnsVaultCmd.Program.AppMain(args, new IOStreams(emptyStdIn, AnyStdOut, stdErr));

                Assert.True(exitCode != 0);
                Assert.False(stdErr.ToString().IsEmpty());
            });

            "The plaintext can be written to a file instead".Is(() =>
            {
                var outputFile = Path.GetTempFileName();

                try
                {
                    var args = new[] { "--password", ValidPassword, "--outfile", outputFile };
                    var stdIn = CreateCiphertextInput();
                    var stdOut = new StringWriter();

                    AnsVaultCmd.Program.AppMain(args, new IOStreams(stdIn, stdOut, AnyStdErr));


                    var plaintext = File.ReadAllText(outputFile);
                    Assert.True(plaintext == ValidPlainText);

                    var output = stdOut.ToString();
                    Assert.True(output.IsEmpty());
                }
                finally
                {
                    File.Delete(outputFile);
                }
            });

            "An invalid plaintext file generates an error".Is(() =>
            {
                var outputFile = Path.GetTempPath();
                var args = new[] { "--password", ValidPassword, "--outfile", outputFile };
                var stdIn = CreateCiphertextInput();
                var stdErr = new StringWriter();

                var exitCode = AnsVaultCmd.Program.AppMain(args, new IOStreams(stdIn, AnyStdOut, stdErr));

                Assert.True(exitCode != 0);
                Assert.False(stdErr.ToString().IsEmpty());
            });
        }

        private static TextReader CreateCiphertextInput()
        {
            var ciphertext = string.Join("\r\n", ValidCiphertext);

            return new StringReader(ciphertext); 
        }

        private static string CreateCiphertextTestFile()
        {
            var inputFile = Path.GetTempFileName();
            var ciphertext = string.Join("\r\n", ValidCiphertext);

            File.WriteAllText(inputFile, ciphertext, Encoding.ASCII);

            return inputFile;
        }

        private static string CreateMissingTempFile()
        {
            var filename = Path.GetTempFileName();

            File.Delete(filename);

            return filename;
        }

        private static readonly TextReader AnyStdIn = new StringReader("");
        private static readonly TextWriter AnyStdOut = new StringWriter();
        private static readonly TextWriter AnyStdErr = new StringWriter();
        private static readonly IOStreams AnyStreams = new IOStreams(AnyStdIn, AnyStdOut, AnyStdErr);

        private const string ValidPassword = "password";
        private static readonly string[] ValidCiphertext = new[]
        { 
            "$ANSIBLE_VAULT;1.1;AES256",
            "65306664396363623635396463366664353130646532316462343063336536623663306432386637",
            "3632363837373131646265363639336132316637326534660a386438343963666565373361376163",
            "37396630366665633332663331303963363836316632363664336339663134326465383630363433",
            "6161336634656661370a353331666363373233373464316138376336356339366335663063653035",
            "6162"
        };

        private const string ValidPlainText = "TEST";
    }
}
