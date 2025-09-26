using System.Text.Json;

namespace BuildingBlocks.Infrastructure.Serialization;

/// <summary>
/// Tiny serializer to avoid coupling to any specific app layer
/// </summary>
internal static class SystemTextJsonSerializer
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    public static string Serialize<T>(T obj) => JsonSerializer.Serialize(obj, Options);

    public static object? Deserialize(string json, string typeName)
    {
        var type = Type.GetType(typeName, throwOnError: false);
        return type is null ? null : JsonSerializer.Deserialize(json, type, Options);
    }
}
