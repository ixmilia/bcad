using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IxMilia.BCad.EventArguments;
using IxMilia.BCad.Services;

namespace IxMilia.BCad.Extensions
{
    public static class ServiceExtensions
    {
        public static Func<string, Task<byte[]>> GetContentResolverRelativeToPath(this IFileSystemService fileSystemService, string referencePath)
        {
            return path =>
            {
                var resolvedPath = path;
                if (referencePath != null &&
                    !Path.IsPathRooted(path) &&
                    Path.IsPathRooted(referencePath))
                {
                    resolvedPath = Path.Combine(Path.GetDirectoryName(referencePath), path);
                }

                return fileSystemService.ReadAllBytesAsync(resolvedPath);
            };
        }

        public static async Task<bool> TrySubmitValueAsync(this IInputService inputService, string text, Point cursorPoint = default)
        {
            var valueReceivedTaskCompletionSource = new TaskCompletionSource<bool>();
            void ValueReceivedHandler(object sender, ValueReceivedEventArgs e) => valueReceivedTaskCompletionSource.SetResult(true);

            inputService.ValueReceived += ValueReceivedHandler;

            var result = true;
            var handled = false;
            if (inputService.AllowedInputTypes.HasFlag(InputType.Directive) &&
                inputService.AllowedDirectives.Contains(text))
            {
                inputService.PushDirective(text);
                handled = true;
            }

            if (!handled && inputService.AllowedInputTypes.HasFlag(InputType.Distance))
            {
                if (string.IsNullOrEmpty(text))
                {
                    inputService.PushNone();
                    handled = true;
                }
                else if (DrawingSettings.TryParseUnits(text, out var distance))
                {
                    inputService.PushDistance(distance);
                    handled = true;
                }
            }

            if (!handled && inputService.AllowedInputTypes.HasFlag(InputType.Point))
            {
                if (inputService.TryParsePoint(text, cursorPoint, inputService.LastPoint, out var point))
                {
                    inputService.PushPoint(point);
                }
                else
                {
                    inputService.PushNone();
                }

                handled = true;
            }

            if (!handled && inputService.AllowedInputTypes.HasFlag(InputType.Command))
            {
                inputService.PushCommand(string.IsNullOrEmpty(text) ? null : text);
                handled = true;
            }

            if (!handled && inputService.AllowedInputTypes.HasFlag(InputType.Text))
            {
                inputService.PushText(text ?? string.Empty);
                handled = true;
            }

            if (!handled)
            {
                // not sure what to do
                result = false;
            }

            if (!result)
            {
                // still have to complete the task
                valueReceivedTaskCompletionSource.SetResult(false);
            }

            await valueReceivedTaskCompletionSource.Task;
            inputService.ValueReceived -= ValueReceivedHandler;

            return result;
        }
    }
}
