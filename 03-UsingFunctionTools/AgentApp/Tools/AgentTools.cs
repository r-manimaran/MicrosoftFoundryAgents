using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace AgentApp.Tools;

public static class AgentTools
{
    [Description("Get the weather forecast for a given location.")]
    public static string GetWeatherForecast([Description("The location to get the weather for.")]string location)
    {
        // Placeholder implementation
        return $"The weather forecast for {location} is sunny with a high of 25°C.";
    }

    [Description("Get the current date and time.")]
    public static string GetCurrentDateTime()
    {
        return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
