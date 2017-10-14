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
            return ExitCode.Success;
        }
    }
}
