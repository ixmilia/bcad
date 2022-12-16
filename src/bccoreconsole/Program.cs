using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IxMilia.BCad
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var workspace = new ConsoleWorkspace();
            string inputFile = null;
            string scriptFile = null;
            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                switch (arg.ToLowerInvariant())
                {
                    case "/i":
                        if (i + 1 < args.Length)
                        {
                            inputFile = args[i + 1];
                            i++;
                        }
                        else
                        {
                            throw new ArgumentException("Expected input file name after '/i' switch.");
                        }
                        break;
                    case "/s":
                        if (i + 1 < args.Length)
                        {
                            scriptFile = args[i + 1];
                            i++;
                        }
                        else
                        {
                            throw new ArgumentException("Expected script file name after '/i' switch.");
                        }
                        break;
                    default:
                        throw new ArgumentException($"Unexpected argument {arg}");
                }
            }

            if (inputFile is null)
            {
                throw new ArgumentException("Expected input file specified with '/i' switch.");
            }

            if (scriptFile is null)
            {
                throw new ArgumentException("Expected script file specified with '/s' switch.");
            }

            await workspace.ExecuteCommand("File.Open", inputFile);
            var batchCommandLines = File.ReadAllLines(scriptFile).Select(line => line.Trim()).Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
            foreach (var batchCommandLine in batchCommandLines)
            {
                var result = await workspace.ExecuteCommandLine(batchCommandLine);
                if (!result)
                {
                    throw new Exception($"Error executing batch command: {batchCommandLine}");
                }
            }
        }
    }
}
