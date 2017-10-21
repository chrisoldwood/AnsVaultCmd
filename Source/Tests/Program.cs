using System.Reflection;
using Mechanisms.Host;
using Mechanisms.Tests;

namespace Tests
{
    public class Program
    {
        public static int Main(string[] args)
        {
            return Bootstrapper.Run(AppMain, args);
        }

        internal static int AppMain(string[] args)
        {
            return Runner.Run(Assembly.GetExecutingAssembly(), args);
        }
    }
}
