using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using IxMilia.BCad.Services;

namespace bcad
{
    internal class FileSystemService : IFileSystemService
    {
        private Action<Action> _dispatch;

        public FileSystemService(Action<Action> dispatch)
        {
            _dispatch = dispatch;
        }

        public Task<string> GetFileNameFromUserForOpen(IEnumerable<FileSpecification> fileSpecifications)
        {
            return DispatchAsync(() => FileDialogs.OpenFile(fileSpecifications));
        }

        public Task<string> GetFileNameFromUserForSave(IEnumerable<FileSpecification> fileSpecifications)
        {
            return DispatchAsync(() => FileDialogs.SaveFile(fileSpecifications));
        }

        public Task<byte[]> ReadAllBytesAsync(string path)
        {
            return File.ReadAllBytesAsync(path);
        }

        private Task<T> DispatchAsync<T>(Func<T> func)
        {
            var dispatchCompletion = new TaskCompletionSource<T>();
            _dispatch(() =>
            {
                var result = func();
                dispatchCompletion.SetResult(result);
            });
            return dispatchCompletion.Task;
        }
    }
}
