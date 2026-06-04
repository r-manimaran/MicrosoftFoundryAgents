namespace SharedLib;

public class AppConfig
{
    public const string SectionName = "AzureAI";
    public string FoundryProjectEndpoint { get; set; } = string.Empty;
    public string ModelDeploymentName { get; set; } = string.Empty;
}
