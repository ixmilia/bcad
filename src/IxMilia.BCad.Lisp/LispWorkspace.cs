using System;
using System.Threading.Tasks;
using IxMilia.Lisp;
using System.Threading;

namespace IxMilia.BCad.Lisp
{
    public abstract class LispWorkspace : WorkspaceBase
    {
        private Lazy<Task<LispHost>> _lispHostTask;

        protected LispWorkspace()
        {
            _lispHostTask = new Lazy<Task<LispHost>>(async () =>
            {
                var writer = new OutputForwardingTextWriter(this);
                var configuration = new LispHostConfiguration(output: writer, readerType: LispReaderType.NoReaderMacros);
                var host = await LispHost.CreateAsync(configuration);
                host.AddContextObject(new BCadContext(this));
                return host;
            });
        }

        public async Task<LispObject> EvaluateAsync(string code, CancellationToken cancellationToken = default)
        {
            var host = await _lispHostTask.Value;
            var executionState = host.CreateExecutionState(allowHalting: false);
            var evalResult = await host.EvalAsync("#<script>", code, executionState, cancellationToken);
            return evalResult.Value;
        }
    }
}
