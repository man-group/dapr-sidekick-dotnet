using System;

namespace Dapr.Sidekick.Resources
{
    public class ProcessProgram
    {
        public class Program
        {
            public static void Main(string[] args)
            {
                // Write the arguments to the Console with a pipe separator
                Console.WriteLine("ARGS|" + string.Join("|", args));
                Console.WriteLine("Program Started");

                // Wait for 1 second to complete
                System.Threading.Thread.Sleep(1000);
            }
        }
    }
}
