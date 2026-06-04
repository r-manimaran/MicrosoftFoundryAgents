using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharedLib;

public class FoundryService
{
    private readonly AppConfig _config;

    public FoundryService(IOptions<AppConfig> options)
    {
        _config = options.Value;

    }
    public void PrintConfig()
    {
        Console.WriteLine($"Endpoint : {_config.FoundryProjectEndpoint}");
        Console.WriteLine($"Model    : {_config.ModelDeploymentName}");
    }

}
