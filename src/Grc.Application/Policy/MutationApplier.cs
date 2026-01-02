using System.Text.Json;
using System.Text.Json.Nodes;

namespace Grc.Application.Policy;

public class MutationApplier
{
    public static object ApplyMutation(object obj, PolicyModels.Mutation mutation)
    {
        var jsonNode = JsonSerializer.SerializeToNode(obj) ?? throw new InvalidOperationException("Failed to serialize object");

        var pathParts = mutation.Path.Split('.', StringSplitOptions.RemoveEmptyEntries);
        
        if (pathParts.Length == 0)
            throw new ArgumentException("Invalid mutation path", nameof(mutation));

        var targetNode = NavigateToParent(jsonNode, pathParts[..^1]);
        var propertyName = pathParts[^1];

        switch (mutation.Op.ToLowerInvariant())
        {
            case "set":
                if (targetNode is JsonObject jsonObj)
                {
                    jsonObj[propertyName] = JsonSerializer.SerializeToNode(mutation.Value);
                }
                break;

            case "remove":
                if (targetNode is JsonObject jsonObjRemove)
                {
                    jsonObjRemove.Remove(propertyName);
                }
                break;

            case "add":
                if (targetNode is JsonObject jsonObjAdd)
                {
                    jsonObjAdd[propertyName] = JsonSerializer.SerializeToNode(mutation.Value);
                }
                else if (targetNode is JsonArray jsonArray)
                {
                    if (int.TryParse(propertyName, out var index))
                    {
                        jsonArray.Insert(index, JsonSerializer.SerializeToNode(mutation.Value));
                    }
                    else
                    {
                        jsonArray.Add(JsonSerializer.SerializeToNode(mutation.Value));
                    }
                }
                break;

            default:
                throw new ArgumentException($"Unknown mutation operation: {mutation.Op}", nameof(mutation));
        }

        return JsonSerializer.Deserialize<object>(jsonNode.ToString()) ?? obj;
    }

    private static JsonNode NavigateToParent(JsonNode root, string[] pathParts)
    {
        var current = root;
        foreach (var part in pathParts)
        {
            if (current is JsonObject jsonObject)
            {
                if (!jsonObject.TryGetPropertyValue(part, out var property))
                {
                    property = new JsonObject();
                    jsonObject[part] = property;
                }
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
                    throw new ArgumentException($"Invalid array index: {part}");
                }
            }
            else
            {
                throw new InvalidOperationException($"Cannot navigate path: {string.Join(".", pathParts)}");
            }
        }
        return current;
    }
}
