using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using SharedLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgentApp.Middlewares;

public static class AppMiddlewares
{
    public static async ValueTask<object?> FunctionCallMiddleware(AIAgent agent, FunctionInvocationContext context,
       Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next,
       CancellationToken cancellationToken)
    {
        Utils.Gray($"Function Name:{context!.Function.Name} - Middleware 1 Pre-Invoke");
        var result = await next(context, cancellationToken);
        Utils.Gray($"Function Name:{context!.Function.Name} - Middleware 1 Post-Invoke");

        return result;
    }
}
