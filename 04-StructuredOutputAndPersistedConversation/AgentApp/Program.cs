using AgentApp;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SharedLib;
using System.Text.Json;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<AppConfig>(builder.Configuration.GetSection(AppConfig.SectionName));

//builder.Logging.AddOpenTelemetry(o =>
//{
//    o.IncludeFormattedMessage = true;
//    o.IncludeScopes = true;
//});

IHost host = builder.Build();

AppConfig config = host.Services.GetRequiredService<IOptions<AppConfig>>().Value;

string endpoint = config.FoundryProjectEndpoint;

string deploymentName = config.ModelDeploymentName;

const string agentName = "CsharpBestPracticeAgent";

// WARNING: DefaultAzureCredential or AzureCliCredential is convenient for development but requires careful consideration in production.
// In production, consider using a specific credential (e.g., ManagedIdentityCredential) to avoid
// latency issues, unintended credential probing, and potential security risks from fallback mechanisms.

AIProjectClient aiProjectClient = new AIProjectClient(new Uri(endpoint),
                                       new AzureCliCredential());

AIAgent aiAgent = aiProjectClient.AsAIAgent(new ChatClientAgentOptions
{
    Name = "StructuredOutputAssistant",
    ChatOptions = new()
    {
        ModelId = deploymentName,
        Instructions = "You are a helpful assistant that extracts structured information about Person",
        ResponseFormat = ChatResponseFormat.ForJsonSchema<PersonInfo>()
    }
});

AgentSession session = await aiAgent.CreateSessionAsync();

string prompt = "Please provide information about John Smith, who is a 35-year-old software engineer.";
Utils.WriteLineInformation($"Sending prompt to agent: {prompt}");
AgentResponse<PersonInfo> response = await aiAgent.RunAsync<PersonInfo>(prompt, session);
Utils.Gray("Assistant response:");
Utils.WriteLineInformation($"Name: {response.Result.Name}");
Utils.WriteLineInformation($"Age: {response.Result.Age}");
Utils.WriteLineInformation($"Occupation: {response.Result.Occupation}");

// Serialize the session state to a JSON element, so it can be stored for later use.
JsonElement serializedSession = await aiAgent.SerializeSessionAsync(session);

// Save the session to local
string tempFile = Path.GetTempFileName();
await File.WriteAllTextAsync(tempFile, JsonSerializer.Serialize(serializedSession));

// Load Serialized Session from the temp file
JsonElement reloadSerializedSession = JsonElement.Parse(await File.ReadAllTextAsync(tempFile))!;

// AgentSession 
AgentSession resumedSession = await aiAgent.DeserializeSessionAsync(reloadSerializedSession);
string newPrompt = "Now tell me about the person we have discussed earlier.";
AgentResponse newResponse = await aiAgent.RunAsync(prompt, resumedSession);
Utils.WriteLineInformation($"{newPrompt}");
Utils.WriteLineInformation("--------------------");
Utils.Yellow($"New Response: {newResponse}");
Utils.Separator();





