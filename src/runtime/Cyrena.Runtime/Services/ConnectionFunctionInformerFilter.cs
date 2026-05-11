using Cyrena.Contracts;
using Microsoft.SemanticKernel;

namespace Cyrena.Runtime.Services
{
    /// <summary>
    /// Informs <see cref="IConnection"/> when a function call starts to reduce history pollution
    /// </summary>
    internal class ConnectionFunctionInformerFilter : IAutoFunctionInvocationFilter
    {
        private readonly IConnection _connection;
        public ConnectionFunctionInformerFilter(IConnection connection)
        {
            _connection = connection;
        }

        public async Task OnAutoFunctionInvocationAsync(AutoFunctionInvocationContext context, Func<AutoFunctionInvocationContext, Task> next)
        {
            _connection.FunctionCallStart();
            await next(context);
        }
    }
}
