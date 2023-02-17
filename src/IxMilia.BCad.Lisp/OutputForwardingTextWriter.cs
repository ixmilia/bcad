using System.IO;
using System.Text;

namespace IxMilia.BCad.Lisp
{
    internal class OutputForwardingTextWriter : TextWriter
    {
        private IWorkspace _workspace;
        private StringBuilder _sb = new StringBuilder();

        public override Encoding Encoding => Encoding.UTF8;

        public OutputForwardingTextWriter(IWorkspace workspace)
        {
            _workspace = workspace;
        }

        public override void Write(char value)
        {
            _sb.Append(value);
            if (value == '\n')
            {
                Flush();
            }
        }

        public override void Flush()
        {
            var line = _sb.ToString();
            _workspace.OutputService.WriteLine(line);
            _sb.Clear();
        }
    }
}
