using System.Text.Json;
using System.Text.Json.Nodes;

namespace Grc.Application.Policy;

public class DotPathResolver
{
    public static object? Resolve(object obj, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return obj;

        var parts = path.Split('.', StringSplitOptions.RemoveEmptyEntries);
        var current = JsonSerializer.SerializeToNode(obj);

        foreach (var part in parts)
        {
            if (current == null)
                return null;

            if (current is JsonObject jsonObject)
            {
                if (!jsonObject.TryGetPropertyValue(part, out var property))
                    return null;
                current = property;
            }
            else if (current is JsonArray jsonArray)
            {
                if (int.TryParse(part, out var index) && index >= 0 && index < jsonArray.Count)
                {
                    current = jsonArray[index];
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        return current?.GetValue<object>();
    }

    public static bool Exists(object obj, string path)
    {
        return Resolve(obj, path) != null;
    }

    public static T? Resolve<T>(object obj, string path)
    {
        var value = Resolve(obj, path);
        if (value == null)
            return default;

        try
        {
            return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value));
        }
        catch
        {
            return default;
        }
    }
}
