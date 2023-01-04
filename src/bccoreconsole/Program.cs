using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace IxMilia.BCad
{
    class Program
    {
        static int Main(string[] args)
        {
            var binDirectory = AppContext.BaseDirectory;
#if DEBUG
            binDirectory = Path.Join(binDirectory, "..", "..", "..", "bcad", "Debug", "net7.0");
#endif

            var proc = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = Path.Join(binDirectory, "bcad.exe"),
                    Arguments = $"coreconsole {string.Join(" ", args.Select(a => string.Concat("\"", a, "\"")))}",
                    WorkingDirectory = binDirectory,
                }
            };

            proc.Start();
            proc.WaitForExit();
            return proc.ExitCode;
        }
    }
}
