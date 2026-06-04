
using Azure;
using Azure.AI.Projects;
using Azure.AI.Projects.Agents;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Foundry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SharedLib;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<AppConfig>(builder.Configuration.GetSection(AppConfig.SectionName));

builder.Services.AddSingleton<FoundryService>();

IHost host = builder.Build();

AppConfig config = host.Services.GetRequiredService<IOptions<AppConfig>>().Value;

string endpoint = config.FoundryProjectEndpoint;

string deploymentName = config.ModelDeploymentName;

const string agentName = "CsharpBestPracticeAgent";

AIProjectClient aiProjectClient = new AIProjectClient(new Uri(endpoint), new AzureCliCredential());

ProjectsAgentVersion agentVersion = await aiProjectClient.AgentAdministrationClient.CreateAgentVersionAsync(
    agentName,
    new ProjectsAgentVersionCreationOptions(
        new DeclarativeAgentDefinition(model: deploymentName)
        {
            Instructions = "You are a good at telling best Practices in .Net 10 and C#",
        }
    ));

FoundryAgent agent = aiProjectClient.AsAIAgent(agentVersion);

AgentResponse response = await agent.RunAsync("Tell me a best practice in developing .Net Web Api");
Console.WriteLine(response);
Utils.Separator();
response?.Usage?.OutputAsInformation();

await host.StopAsync();
host.Dispose();


