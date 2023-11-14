using System.Diagnostics;
using System.IO;

namespace bcad
{
    class ScriptUpdater
    {
        public static void StartUpdate(string appName, string installScriptUrl, params (string Name, string Value)[] environmentVariables)
        {
            var updateLogPath = Path.Combine(Path.GetTempPath(), appName + "-updater.log");
            var outputRedirect = $">\"{updateLogPath}\" 2>&1";
            var commandArgs = string.Join(" ", new[]
            {
                "/c", // execute the following then close
                "powershell", "-Command", $"Invoke-Expression (Invoke-WebRequest '{installScriptUrl}').ToString()",
                outputRedirect, // redirect all output to log file
            });
            var psi = new ProcessStartInfo()
            {
                FileName = "cmd.exe",
                Arguments = commandArgs,
                WorkingDirectory = Path.GetTempPath(),
            };
            foreach (var (name, value) in environmentVariables)
            {
                psi.Environment[name] = value;
            }

            var proc = new Process()
            {
                StartInfo = psi,
            };
            proc.Start(); // fire and forget
        }
    }
}
