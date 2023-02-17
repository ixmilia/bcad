using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IxMilia.BCad.Extensions;
using IxMilia.Lisp;

namespace IxMilia.BCad.Lisp
{
    internal class BCadContext
    {
        private IWorkspace _workspace;

        public BCadContext(IWorkspace workspace)
        {
            _workspace = workspace;
        }

        [LispFunction("COMMAND", Signature = "COMMAND-NAME &REST COMMAND-ARGS")]
        public async Task<LispObject> Command(LispHost host, LispExecutionState executionState, LispObject[] args, CancellationToken cancellationToken)
        {
            if (args.Length >= 1 &&
                args[0] is LispString commandName)
            {
                var commandStarted = await _workspace.InputService.TrySubmitValueAsync(commandName.Value);
                if (!commandStarted)
                {
                    executionState.ReportError(new LispError("Expected input to be accepted"), insertPop: true);
                    return host.Nil;
                }

                foreach (var arg in args.Skip(1))
                {
                    var argString = ConvertToString(arg);
                    var argAccepted = await _workspace.InputService.TrySubmitValueAsync(argString);
                    if (!argAccepted)
                    {
                        executionState.ReportError(new LispError("Expected input to be accepted"), insertPop: true);
                        return host.Nil;
                    }
                }

                if (_workspace.IsCommandExecuting)
                {
                    _workspace.InputService.PushNone();
                }

                if (_workspace.IsCommandExecuting)
                {
                    executionState.ReportError(new LispError("Expected command to be finished"), insertPop: true);
                    return host.Nil;
                }

                return host.Nil;
            }

            executionState.ReportError(new LispError("Expected command name and optional arguments"), insertPop: true);
            return host.Nil;
        }

        internal static string ConvertToString(LispObject obj)
        {
            return obj switch
            {
                LispSimpleNumber n => n.AsFloat().Value.ToString(),
                LispString s => s.Value,
                LispList l => string.Join(",", l.ToList().Select(ConvertToString)),
                _ => throw new NotSupportedException($"Unsupported argument type: {obj.GetType().Name}")
            };
        }
    }
}
