using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SharedLib;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<AppConfig>(builder.Configuration.GetSection(AppConfig.SectionName));

IHost host = builder.Build();

AppConfig config = host.Services.GetRequiredService<IOptions<AppConfig>>().Value;

string endpoint = config.FoundryProjectEndpoint;

string deploymentName = config.ModelDeploymentName;

const string agentName = "CsharpBestPracticeAgent";

AIAgent agent= new AIProjectClient(new Uri(endpoint), new DefaultAzureCredential())
        .AsAIAgent(deploymentName, instructions: "You are a helpful assistant that provides C# best practices to developers.", 
        name: agentName);

AgentSession session = await agent.CreateSessionAsync();

AgentResponse response = await agent.RunAsync("Tell me about some C# best practices for web Api development.", session);

Utils.WriteLineInformation($"Agent Response: {response}");
response?.Usage?.OutputAsInformation();


Utils.Separator();
Utils.Gray("Second run with context from previous run...");
AgentResponse agentResponse = await agent.RunAsync("What are some best practices for error handling in C# web Api development?", session);

Utils.WriteLineInformation($"Agent Response: {agentResponse}");
agentResponse?.Usage?.OutputAsInformation();

