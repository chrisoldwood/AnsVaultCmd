using System.Reflection;
using Mechanisms.Tests;

namespace Tests
{
    public class Program
    {
        public static int Main(string[] args)
        {
            return Runner.TestsMain(args, Assembly.GetExecutingAssembly());
        }
    }
}
