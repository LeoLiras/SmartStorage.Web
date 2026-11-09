using System.Text.Json;

namespace SmartStorage.APIGateway.Tests;

public static class RouteParity
{
    // O appsettings.json do gateway separa os serviços com comentários, que o
    // JsonDocument recusa por padrão.
    internal static readonly JsonDocumentOptions ReadOptions = new()
    {
        CommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    private const string NoRoute = "nenhuma rota";

    public static IReadOnlyList<string> FindDivergences(string devJson, string dockerJson)
    {
        var devRoutes = ReadRoutes(devJson);
        var dockerRoutes = ReadRoutes(dockerJson);
        var divergences = new List<string>();

        for (var index = 0; index < Math.Max(devRoutes.Count, dockerRoutes.Count); index++)
        {
            var devRoute = index < devRoutes.Count ? devRoutes[index] : NoRoute;
            var dockerRoute = index < dockerRoutes.Count ? dockerRoutes[index] : NoRoute;

            if (devRoute != dockerRoute)
            {
                divergences.Add(
                    $"Posição {index + 1}: appsettings.json tem {devRoute}, appsettings.Docker.json tem {dockerRoute}");
            }
        }

        return divergences;
    }

    private static List<string> ReadRoutes(string json)
    {
        using var document = JsonDocument.Parse(json, ReadOptions);

        return document.RootElement.GetProperty("Routes").EnumerateArray().Select(Describe).ToList();
    }

    private static string Describe(JsonElement route)
    {
        var methods = route.GetProperty("UpstreamHttpMethod")
            .EnumerateArray()
            .Select(method => method.GetString().ToUpperInvariant())
            .OrderBy(method => method, StringComparer.Ordinal);

        return $"{string.Join(",", methods)} {route.GetProperty("UpstreamPathTemplate").GetString()}";
    }
}
