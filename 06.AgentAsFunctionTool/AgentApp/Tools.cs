using System.ComponentModel;

namespace AgentApp;

public static class Tools
{
    [Description("Get the Weather for a given location.")]
    public static string GetWeather([Description("The location to get the weather for.")] string location) =>
        $"The weather in {location} is cloudy with a high of 15°C.";

    [Description("Get the current date and time, optionally for a specific timezone.")]
    public static string GetCurrentDateTime(
        [Description("IANA timezone name (e.g. 'America/New_York', 'Asia/Kolkata'). Defaults to UTC if not provided.")] string? timezone = null)
    {
        try
        {
            var tz = timezone is null ? TimeZoneInfo.Utc : TimeZoneInfo.FindSystemTimeZoneById(timezone);
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
            return $"Current date and time in {tz.DisplayName}: {now:dddd, MMMM dd yyyy HH:mm:ss}";
        }
        catch
        {
            return $"Unknown timezone '{timezone}'. Current UTC time: {DateTime.UtcNow:dddd, MMMM dd yyyy HH:mm:ss}";
        }
    }

    [Description("Convert a temperature value between Celsius, Fahrenheit, and Kelvin.")]
    public static string ConvertTemperature(
        [Description("The numeric temperature value to convert.")] double value,
        [Description("The source unit: 'C' for Celsius, 'F' for Fahrenheit, 'K' for Kelvin.")] string fromUnit,
        [Description("The target unit: 'C' for Celsius, 'F' for Fahrenheit, 'K' for Kelvin.")] string toUnit)
    {
        double celsius = fromUnit.ToUpper() switch
        {
            "C" => value,
            "F" => (value - 32) * 5 / 9,
            "K" => value - 273.15,
            _ => throw new ArgumentException($"Unknown unit '{fromUnit}'")
        };

        double result = toUnit.ToUpper() switch
        {
            "C" => celsius,
            "F" => celsius * 9 / 5 + 32,
            "K" => celsius + 273.15,
            _ => throw new ArgumentException($"Unknown unit '{toUnit}'")
        };

        return $"{value}°{fromUnit.ToUpper()} = {result:F2}°{toUnit.ToUpper()}";
    }

    [Description("Convert a distance between common units: meters, kilometers, miles, feet, yards.")]
    public static string ConvertDistance(
        [Description("The numeric distance value to convert.")] double value,
        [Description("Source unit: 'm', 'km', 'mi', 'ft', 'yd'.")] string fromUnit,
        [Description("Target unit: 'm', 'km', 'mi', 'ft', 'yd'.")] string toUnit)
    {
        var toMeters = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
        {
            ["m"] = 1, ["km"] = 1000, ["mi"] = 1609.344, ["ft"] = 0.3048, ["yd"] = 0.9144
        };

        if (!toMeters.TryGetValue(fromUnit, out double fromFactor))
            return $"Unknown source unit '{fromUnit}'.";
        if (!toMeters.TryGetValue(toUnit, out double toFactor))
            return $"Unknown target unit '{toUnit}'.";

        double result = value * fromFactor / toFactor;
        return $"{value} {fromUnit} = {result:F4} {toUnit}";
    }

    [Description("Perform a basic arithmetic calculation: add, subtract, multiply, or divide two numbers.")]
    public static string Calculate(
        [Description("The first number.")] double a,
        [Description("The operation: 'add', 'subtract', 'multiply', 'divide'.")] string operation,
        [Description("The second number.")] double b)
    {
        return operation.ToLower() switch
        {
            "add"      => $"{a} + {b} = {a + b}",
            "subtract" => $"{a} - {b} = {a - b}",
            "multiply" => $"{a} × {b} = {a * b}",
            "divide"   => b == 0 ? "Cannot divide by zero." : $"{a} ÷ {b} = {a / b:F6}",
            _          => $"Unknown operation '{operation}'. Use: add, subtract, multiply, divide."
        };
    }
}
