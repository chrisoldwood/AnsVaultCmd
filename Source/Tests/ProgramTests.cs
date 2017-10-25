using System.IO;
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

                var exitCode = VaultCmd.Program.AppMain(helpArg, AnyStdIn, AnyStdOut, AnyStdErr);

                Assert.True(exitCode == 0);
            });

            "Requesting help outputs a usage message".Is(() =>
            {
                var helpArg = new[] { "--help" };
                var stdOut = new StringWriter();

                VaultCmd.Program.AppMain(helpArg, AnyStdIn, stdOut, AnyStdErr);

                var output = stdOut.ToString();
                Assert.True(output.Contains("USAGE"));
                Assert.True(output.Contains("--help"));
            });
        }

        [TestCases]
        public static void version_switch()
        {
            "Requesting the version returns 0".Is(() =>
            {
                var versionArg = new[] { "--version" };

                var exitCode = VaultCmd.Program.AppMain(versionArg, AnyStdIn, AnyStdOut, AnyStdErr);

                Assert.True(exitCode == 0);
            });

            "Requesting the version outputs a version string".Is(() =>
            {
                var versionArg = new[] { "--version" };
                var stdOut = new StringWriter();

                VaultCmd.Program.AppMain(versionArg, AnyStdIn, stdOut, AnyStdErr);

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

                var exitCode = VaultCmd.Program.AppMain(noArgs, AnyStdIn, AnyStdOut, AnyStdErr);

                Assert.True(exitCode != 0);
            });

            "A missing password outputs an error and usage message".Is(() =>
            {
                var noArgs = new string[0];
                var stdOut = new StringWriter();

                VaultCmd.Program.AppMain(noArgs, AnyStdIn, stdOut, AnyStdErr);

                var output = stdOut.ToString();
                Assert.True(output.Contains("ERROR:"));
                Assert.True(output.Contains("USAGE"));
            });

            "A valid password and ciphertext returns non-zero".Is(() =>
            {
                var passwordOnlyArgs = new[] { "--password", ValidPassword };
                var stdIn = CreateCiphertextInput();

                var exitCode = VaultCmd.Program.AppMain(passwordOnlyArgs, stdIn, AnyStdOut, AnyStdErr);

                Assert.True(exitCode == 0);
            });

            "A valid password and ciphertext writes to stdout by default".Is(() =>
            {
                var passwordOnlyArgs = new[] { "--password", ValidPassword };
                var stdIn = CreateCiphertextInput();
                var stdOut = new StringWriter();

                VaultCmd.Program.AppMain(passwordOnlyArgs, stdIn, stdOut, AnyStdErr);

                var output = stdOut.ToString();
                Assert.True(output.Contains(ValidPlainText));
            });
        }

        private static TextReader CreateCiphertextInput()
        {
            var ciphertext = string.Join("\r\n", ValidCiphertext);

            return new StringReader(ciphertext); 
        }

        private static readonly TextReader AnyStdIn = new StringReader("");
        private static readonly TextWriter AnyStdOut = new StringWriter();
        private static readonly TextWriter AnyStdErr = new StringWriter();

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
