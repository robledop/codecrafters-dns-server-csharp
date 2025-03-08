using System.Text.Json;

namespace codecrafters_dns_server;

public static class DeepClone
{
    public static T? Clone<T>(this T source)
    {
        var serialized = JsonSerializer.Serialize(source);
        return JsonSerializer.Deserialize<T>(serialized);
    }
}