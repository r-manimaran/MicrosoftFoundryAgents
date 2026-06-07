using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SharedLib;
using AgentApp.Tools;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using AgentApp.Middlewares;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
builder.Services.Configure<AppConfig>(builder.Configuration.GetSection(AppConfig.SectionName));
IHost host = builder.Build();

AppConfig config = host.Services.GetRequiredService<IOptions<AppConfig>>().Value;
string endpoint = config.FoundryProjectEndpoint;
string deploymentName = config.ModelDeploymentName;

const string agentName = "WeatherAgentApp";

// Define the function tool.
AITool getWeatherTool = AIFunctionFactory.Create(AgentTools.GetWeatherForecast);
AITool dateTimeTool = AIFunctionFactory.Create(AgentTools.GetCurrentDateTime);

// Define the agent with the tool.
AIProjectClient aiProjectClient = new(new Uri(endpoint), new DefaultAzureCredential());
AIAgent agent = aiProjectClient.AsAIAgent(deploymentName,
    instructions: "You are a helpful assistant that provides weather forecasts. Use the provided tool to get the weather forecast for a given location",
    name: agentName,
    tools: [getWeatherTool]
    );

// Create Session
AgentSession session = await agent.CreateSessionAsync();

// Non-streaming response
string prompt = "What is the weather forecast for Seattle?";
Utils.Green(prompt);
Utils.Separator();
AgentResponse response = await agent.RunAsync(prompt,session);
Utils.WriteLineInformation($"Agent Response: {response}");
Utils.Separator();
response?.Usage?.OutputAsInformation();

// Streaming response
Utils.Gray("Streaming Response:");
session = await agent.CreateSessionAsync();
await foreach(AgentResponseUpdate update in agent.RunStreamingAsync("What is the weather forecast for Seattle?", session))
{
    Console.Write(update);
}

AIAgent originalAgent = aiProjectClient.AsAIAgent(deploymentName,
    instructions: "You are an AI assistant that helps people find information.",
    name: "InformationAssistant",
    tools: [getWeatherTool, dateTimeTool]
    );

// Adding Middleware to the agent Level
AIAgent middlewareEnabledAgent = originalAgent
    .AsBuilder()
    .Use(AppMiddlewares.FunctionCallMiddleware)
    .Use(AppMiddlewares.FunctionCallOverrideWeather)
    .Use(AppMiddlewares.PIIMiddleware, null)
    .Use(AppMiddlewares.LoggingMiddleware, null)
    .Use(AppMiddlewares.GuardrailMiddleware, null)
    .Build();

AgentSession middlewareSession = await middlewareEnabledAgent.CreateSessionAsync();

Utils.Gray("-----------------------------------------------");
Utils.Gray("Agent Response with Middleware:");
Utils.Green("\n\n==== Example 1: Wording Guardrail ===");
string guardrailPrompt = " Tell me something harmfull";
Utils.Gray($"Prompt: {guardrailPrompt}");
AgentResponse guardRailedResponse = await middlewareEnabledAgent.RunAsync(guardrailPrompt);
Utils.Yellow($"Guardrail Response: {guardRailedResponse}");
Utils.Separator();


Utils.Green("\n\n==== Example 2: PII detection Middleware ===");
string piiPrompt = "I'm John Doe, my email is john.doe@example.com and my phone number is 555-123-4567";
Utils.Gray($"Prompt: {piiPrompt}");
AgentResponse piiResponse = await middlewareEnabledAgent.RunAsync(piiPrompt);
Utils.Yellow($"PII Response: {piiResponse}");
Utils.Separator();

Utils.Green("\n\n==== Example 3: Function Call Override Middleware ===");
string functionOverridePrompt = "What's the weather like in New York and what is the current date and time?";
Utils.Gray($"Prompt: {functionOverridePrompt}");
AgentResponse functionCallResponse = await middlewareEnabledAgent.RunAsync(functionOverridePrompt, middlewareSession);
Utils.Yellow($"Function Call Response: {functionCallResponse}");
Utils.Separator();


