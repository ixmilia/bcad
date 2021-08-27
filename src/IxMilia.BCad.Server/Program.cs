using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace IxMilia.BCad.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            var outFiles = new List<string>();
            for (int i = 0; i < args.Length; i += 2)
            {
                if (args[i] == "--out-file" && args.Length > i + 1)
                {
                    outFiles.Add(args[i + 1]);
                }
                else
                {
                    Console.WriteLine("Arguments must be one or more of `--out-file <path/to/file.ts>`");
                    Console.WriteLine($"    found args: [{string.Join("] [", args)}]");
                    Environment.Exit(1);
                }
            }

            if (outFiles.Count > 0)
            {
                var generator = new ContractGenerator(outFiles);
                generator.Run();
            }
            else
            {
                new Program().RunAsync().GetAwaiter().GetResult();
            }
        }

        private async Task RunAsync()
        {
            var server = new FullServer();
            server.Start(Console.OpenStandardOutput(), Console.OpenStandardInput());

            while (server.Agent.IsRunning)
            {
                await Task.Delay(50);
            }
        }
    }
}
