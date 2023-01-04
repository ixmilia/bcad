using System;
using System.Diagnostics;
using System.IO;

namespace IxMilia.BCad
{
    class Program
    {
        static int Main(string[] args)
        {
            var binDirectory = AppContext.BaseDirectory;
            if (Debugger.IsAttached)
            {
#if DEBUG
                binDirectory = Path.Join(binDirectory, "..", "..", "..", "bcad", "Debug", "net7.0");
#endif
            }

            var proc = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "bcad.exe",
                    Arguments = $"coreconsole {string.Join(" ", args)}",
                    WorkingDirectory = binDirectory,
                }
            };

            proc.Start();
            proc.WaitForExit();
            return proc.ExitCode;
        }
    }
}
