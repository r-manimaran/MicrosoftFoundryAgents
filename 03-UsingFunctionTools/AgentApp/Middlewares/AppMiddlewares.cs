using AgentApp.Tools;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using SharedLib;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AgentApp.Middlewares;

public static class AppMiddlewares
{
    private static readonly Regex MyRegex = new(@"\b\d{3}-\d{2}-\d{4}\b");
    private static readonly Regex EmailRegex = new(@"\b[\w.+-]+@[\w-]+\.[\w.]+\b");
    private static readonly Regex FullNameRegex = new(@"\b[A-Z][a-z]+ [A-Z][a-z]+\b");

    /// <summary>
    /// Function Invocation middleware that logs before and after function calls.
    /// </summary>
    /// <param name="agent"></param>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<object?> FunctionCallMiddleware(AIAgent agent, FunctionInvocationContext context,
        Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next,
        CancellationToken cancellationToken)
    {
        Utils.Gray($"Function Name:{context!.Function.Name} - Middleware 1 Pre-Invoke");
        var result = await next(context, cancellationToken);
        Utils.Gray($"Function Name:{context!.Function.Name} - Middleware 1 Post-Invoke");

        return result;
    }

    // - Middleware 2

    /// <summary>
    /// Function invocation middleware that overrides the result of the GetWeatherForecast function.
    /// </summary>
    /// <param name="agent"></param>
    /// <param name="context"></param>
    /// <param name="next"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async ValueTask<object?> FunctionCallOverrideWeather(AIAgent agent, FunctionInvocationContext context,
        Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next,
        CancellationToken cancellationToken)
    {
        Utils.Gray($"Function Name: {context!.Function.Name} - Middleware 2 Pre-Invoke");
        var result = await next(context, cancellationToken);

        if (context.Function.Name == nameof(AgentTools.GetWeatherForecast))
        {
            result = "The weather is sunny with a high of 25°C.";
        }

        Utils.Gray($"Function Name:{context!.Function.Name} - Middleware 2 Post-Invoke");
        return result;
    }

    public static async Task<AgentResponse> PIIMiddleware(IEnumerable<ChatMessage> messages, AgentSession? session,
        AgentRunOptions? agentRunOptions, AIAgent innerAgent, CancellationToken cancellationToken)
    {
        var filteredMessages = FilterMessages(messages);

        var agentResponse = await innerAgent.RunAsync(filteredMessages, session, agentRunOptions, cancellationToken).ConfigureAwait(false);

        agentResponse.Messages = FilterMessages(agentResponse.Messages);

        Utils.Gray("PII Middleware - Filtered Messages Post Run:");

        return agentResponse;

        static List<ChatMessage> FilterMessages(IEnumerable<ChatMessage> messages)
        {
            return messages.Select(m => new ChatMessage(m.Role, FilterPii(m.Text))).ToList();
        }

        static string FilterPii(string content)
        {
            Regex[] piiPatterns = [MyRegex, EmailRegex, FullNameRegex];
            foreach (var pattern in piiPatterns)
            {
                content = pattern.Replace(content, "[REDACTED: PII]");
            }
            return content;
        }
    }

    /// <summary>
    /// Guardrail middleware that filters out harmful content from messages before sending them to the agent and also filters the agent's response.
    /// </summary>
    /// <param name="messages"></param>
    /// <param name="session"></param>
    /// <param name="agentRunOptions"></param>
    /// <param name="innerAgent"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static async Task<AgentResponse> GuardrailMiddleware(IEnumerable<ChatMessage> messages, AgentSession? session,
                            AgentRunOptions? agentRunOptions, AIAgent innerAgent, CancellationToken cancellationToken)
    {
        var filteredMessages = FilterMessages(messages);
        Utils.Gray("Guardrail Middleware - Filtered Messages Pre Run:");
        
        var agentResponse = await innerAgent.RunAsync(filteredMessages, session, agentRunOptions, cancellationToken).ConfigureAwait(false);
        agentResponse.Messages = FilterMessages(agentResponse.Messages);

        Utils.Gray("Guardrail Middleware - Filtered Messages Post Run:");
        return agentResponse;

        List<ChatMessage> FilterMessages(IEnumerable<ChatMessage> messages)
        {
            return messages.Select(m => new ChatMessage(m.Role, FilterContent(m.Text))).ToList();
        }

        static string FilterContent(string content)
        {
            foreach (var keyword in new[] { "harmful", "illegal", "violence" })
            {
                if (content.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    return "[REDACTED: Forbidden content]";
                }
            }
            return content;
        }
    }


    public static async Task<AgentResponse> LoggingMiddleware(IEnumerable<ChatMessage> messages, AgentSession? session,
        AgentRunOptions? agentRunOptions, AIAgent innerAgent, CancellationToken cancellationToken)
    {
        Utils.Gray("Logging Middleware - Messages Pre Run:");
        foreach (var message in messages)
        {
            Utils.Gray($"Role: {message.Role}, Content: {message.Text}");
        }
        var agentResponse = await innerAgent.RunAsync(messages, session, agentRunOptions, cancellationToken).ConfigureAwait(false);
        Utils.Gray("Logging Middleware - Messages Post Run:");
        foreach (var message in agentResponse.Messages)
        {
            Utils.Gray($"Role: {message.Role}, Content: {message.Text}");
        }
        return agentResponse;
    }

    public static async Task<AgentResponse> ConsolePromptingApprovalMiddleware(IEnumerable<ChatMessage> messages, AgentSession? session,
        AgentRunOptions? options,  AIAgent innerAgent, CancellationToken cancellationToken)
    {
        AgentResponse agentResponse = await innerAgent.RunAsync(messages, session, options, cancellationToken);

        List<ToolApprovalRequestContent> approvalRequests = agentResponse.Messages.SelectMany(m => m.Contents).OfType<ToolApprovalRequestContent>().ToList();

        while (approvalRequests.Count > 0)
        {
            agentResponse.Messages = approvalRequests
                .ConvertAll(functionApprovalRequest =>
                {
                    Console.WriteLine($"The agent would like to invoke the following function, please reply Y to approve: Name {((FunctionCallContent)functionApprovalRequest.ToolCall).Name}");
                    bool approved = Console.ReadLine()?.Equals("Y", StringComparison.OrdinalIgnoreCase) ?? false;
                    return new ChatMessage(ChatRole.User, [functionApprovalRequest.CreateResponse(approved)]);
                });

            agentResponse = await innerAgent.RunAsync(agentResponse.Messages, session, options, cancellationToken);

            approvalRequests = agentResponse.Messages.SelectMany(m => m.Contents).OfType<ToolApprovalRequestContent>().ToList();
        }

        return agentResponse;
    }
}


