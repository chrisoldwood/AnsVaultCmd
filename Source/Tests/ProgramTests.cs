using System.IO;
using Mechanisms.Tests;

namespace Tests
{
    public static class ProgramTests
    {
        [TestCases]
        public static void exit_code()
        {
            "An innvocation with no arguments returns non-zero".Is(() =>
            {
                var successfulInnvocationArgs = new[] { "" };

                var exitCode = VaultCmd.Program.AppMain(successfulInnvocationArgs, AnyStdIn, AnyStdOut, AnyStdErr);

                Assert.True(exitCode != 0);
            });

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

                Assert.True(stdOut.ToString().Contains("USAGE"));
            });

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

                Assert.True(stdOut.ToString().Contains("0.0.0.1"));
            });
        }

        private static readonly TextReader AnyStdIn = new StringReader("");
        private static readonly TextWriter AnyStdOut = new StringWriter();
        private static readonly TextWriter AnyStdErr = new StringWriter();
    }
}
