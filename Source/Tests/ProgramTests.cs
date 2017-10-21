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
            "65633232376535306266643337656230373531306536303761353166343434643231636664313339",
            "3365663032646265386239643939656636376361623863360a303737663737306231363039346366",
            "37643430383131313464616637396461653838343738363531633339613430386533663232346366",
            "3964616130336639390a313234643537613163636133346366393435343463643530626139303538",
            "62623665383866386664316431343834346265313564643438663532633961623562",
        };

        private const string ValidPlainText = "test_key: \"test_value\"";
    }
}
