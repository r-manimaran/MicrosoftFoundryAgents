using AgentApp.Middlewares;
using Azure.AI.Projects;
using Azure.AI.Projects.Agents;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Foundry;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Client;
using SharedLib;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<AppConfig>(builder.Configuration.GetSection(AppConfig.SectionName));

IHost host = builder.Build();

AppConfig config = host.Services.GetRequiredService<IOptions<AppConfig>>().Value;

string endpoint = config.FoundryProjectEndpoint;

string deploymentName = config.ModelDeploymentName;

const string agentName = "MicrosoftDocsAgent";

// -- Connect to Microsoft Learn MCP server via HTTP
Utils.Green("Connecting to MCP server at https://learn.microsoft.com/api/mcp ...");
await using McpClient mcpClient = await McpClient.CreateAsync(new HttpClientTransport(new()
{
    Endpoint = new Uri("https://learn.microsoft.com/api/mcp"),
    Name = "Microsoft Learn MCP"
}));

// Retrive the list of tools available in the MCP server
IList<McpClientTool> mcpTools = await mcpClient.ListToolsAsync();
Utils.Gray($"MCP tools available:{string.Join(", ", mcpTools.Select(t => t.Name))}");
List<AITool> agentTools = [.. mcpTools.Cast<AITool>()];

// Create the Agent
AIProjectClient aiProjectClient = new AIProjectClient(new Uri(endpoint),
                                    new AzureCliCredential());
AIAgent agent = aiProjectClient.AsAIAgent(deploymentName,
    instructions: "You are a helpful assistant that can help with microsoft documentation questions. Use the Microsoft " +
    "Learn MCP tool to search for documentation.",
    name: agentName,
    tools: agentTools)
    .AsBuilder()
    .Use(AppMiddlewares.FunctionCallMiddleware).Build();

Utils.Green($"Agent '{agent.Name} created. Ask the questions");

AgentSession session = await agent.CreateSessionAsync();

while (true)
{
    Console.Write("User >");
    string input = Console.ReadLine()!;
    if (string.IsNullOrEmpty(input))
        return;

    AgentResponse response = await agent.RunAsync(input, session);
    Utils.WriteLineInformation(response.ToString());
}
