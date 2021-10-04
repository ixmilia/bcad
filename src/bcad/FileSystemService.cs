using System;
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

        public Task<string> GetFileNameFromUserForOpen()
        {
            return DispatchAsync(() => OpenFileDialog.OpenFile());
        }

        public Task<string> GetFileNameFromUserForSave()
        {
            return DispatchAsync(() => OpenFileDialog.SaveFile());
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
