using BCad.EventArguments;

namespace BCad.Services
{
    public delegate void WriteLineEventHandler(object sender, WriteLineEventArgs e);

    public interface IOutputService
    {
        event WriteLineEventHandler LineWritten;
        void WriteLine(string text, params object[] args);
    }
}
