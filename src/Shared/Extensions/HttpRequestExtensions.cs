using Microsoft.AspNetCore.Http;

using System.Text.Json;
using System.Text.Json.Serialization;

namespace DefaultProjects.Shared.Extensions;

public static class HttpRequestExtensions
{
    public static bool IsJsonType(this HttpRequest request)
    {
        return request.ContentType != null && request.ContentType.Contains("application/json", StringComparison.OrdinalIgnoreCase);
    }

    public static bool TryGetJsonBody(this HttpRequest request, out object? result)
    {
        try
        {
            using var reader = new StreamReader(request.Body);
            var body = reader.ReadToEnd();

            result = JsonSerializer.Deserialize<object>(body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            });

            return true;
        }
        catch (JsonException)
        {
            result = null;
            return false;
        }
    }
}
