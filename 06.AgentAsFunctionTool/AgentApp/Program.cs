using AgentApp;
using Azure.AI.Projects;
using Azure.AI.Projects.Agents;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Foundry;
using Microsoft.Extensions.AI;
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

// Tools
AITool weatherTool         = AIFunctionFactory.Create(Tools.GetWeather);
AITool dateTimeTool        = AIFunctionFactory.Create(Tools.GetCurrentDateTime);
AITool tempConvertTool     = AIFunctionFactory.Create(Tools.ConvertTemperature);
AITool distanceConvertTool = AIFunctionFactory.Create(Tools.ConvertDistance);
AITool calculateTool       = AIFunctionFactory.Create(Tools.Calculate);


// Create the Agent
AIProjectClient aiProjectClient = new AIProjectClient(new Uri(endpoint),new AzureCliCredential());

// Weather Agent
AIAgent weatherAgent = aiProjectClient.AsAIAgent(deploymentName,
    instructions: "You answer questions about the weather, temperature conversions, distance conversions, and basic math.",
    name: "WeatherAgent",
    tools: [weatherTool, tempConvertTool, distanceConvertTool, calculateTool]);

AIAgent dateTimeAgent = aiProjectClient.AsAIAgent(deploymentName,
    instructions: "You answer about Date and Time.",
    name: "DateTimeAgent",
    tools: [dateTimeTool]);

AIAgent agent = aiProjectClient.AsAIAgent(deploymentName,
    instructions: "You are a helpful assistant responds in French and Spanish ",
    name: "MainAgent",
    tools: [weatherAgent.AsAIFunction(),
            dateTimeAgent.AsAIFunction()])
    .AsBuilder()
    .Use(AppMiddlewares.FunctionCallMiddleware).Build();

string prompt = "What's the weather in Chennai and what's the time now?";
Utils.Green(prompt);
AgentSession session = await agent.CreateSessionAsync();
AgentResponse response = await agent.RunAsync(prompt, session);
Utils.WriteLineInformation(response.ToString());


