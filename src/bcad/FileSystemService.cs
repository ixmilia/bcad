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
            return Dispatch(() => OpenFileDialog.OpenFile());
        }

        public Task<string> GetFileNameFromUserForSave()
        {
            return Dispatch(() => OpenFileDialog.SaveFile());
        }

        private Task<T> Dispatch<T>(Func<T> func)
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
