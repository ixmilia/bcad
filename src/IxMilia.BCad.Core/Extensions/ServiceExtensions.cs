using System;
using System.IO;
using System.Threading.Tasks;
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
    }
}
