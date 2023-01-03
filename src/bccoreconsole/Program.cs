using System;
using System.IO;
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

            if (scriptFile is null)
            {
                throw new ArgumentException("Expected script file specified with '/s' switch.");
            }

            if (inputFile is not null)
            {
                await workspace.ExecuteCommand("File.Open", inputFile);
            }

            var batchCommandScript = File.ReadAllText(scriptFile);
            var result = await workspace.ExecuteTokensFromScriptAsync(batchCommandScript);
            if (!result)
            {
                throw new Exception($"Error executing batch command: {batchCommandScript}");
            }
        }
    }
}
