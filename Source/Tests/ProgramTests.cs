using Mechanisms.Tests;

namespace Tests
{
    public static class ProgramTests
    {
        [TestCases]
        public static void exit_code()
        {
            "A succeessful innvocation returns 0".Is(() =>
            {
                var successfulInnvocationArgs = new[] { "" };

                var exitCode = VaultCmd.Program.AppMain(successfulInnvocationArgs);

                Assert.True(exitCode == 0);
            });
        }
    }
}
