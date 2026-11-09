using System.Text.Json;
using System.Text.RegularExpressions;

namespace SmartStorage.APIGateway.Tests;

public static class CollectionCoverage
{
    private const string BaseUrl = "{{baseUrl}}";

    // O gateway escreve {id} e a coleção escreve {{productId}}: o nome da variável
    // não identifica a rota, só o lugar dela no caminho.
    private static readonly Regex PathVariable = new(@"^\{+[^{}]+\}+$");

    public static IReadOnlyList<string> FindDivergences(string gatewayJson, string collectionJson)
    {
        var routes = ReadRoutes(gatewayJson);
        var requests = ReadRequests(collectionJson);

        var routeKeys = routes.Select(route => route.Key).ToHashSet();
        var requestKeys = requests.Select(request => request.Key).ToHashSet();

        var missing = routes
            .Where(route => !requestKeys.Contains(route.Key))
            .Select(route => $"A rota {route} não tem requisição na coleção");

        var orphans = requests
            .Where(request => !routeKeys.Contains(request.Key))
            .Select(request => $"A requisição {request} não tem rota no gateway");

        var repeated = requests
            .Where(request => routeKeys.Contains(request.Key))
            .GroupBy(request => request.Key)
            .Where(group => group.Count() > 1)
            .Select(group => $"A coleção tem {group.Count()} requisições para a rota {group.First()}");

        return missing.Concat(orphans).Concat(repeated).ToList();
    }

    private static List<Endpoint> ReadRoutes(string gatewayJson)
    {
        using var document = JsonDocument.Parse(gatewayJson, RouteParity.ReadOptions);

        return document.RootElement.GetProperty("Routes")
            .EnumerateArray()
            .SelectMany(route => route.GetProperty("UpstreamHttpMethod")
                .EnumerateArray()
                .Select(method => new Endpoint(method.GetString(), route.GetProperty("UpstreamPathTemplate").GetString())))
            .ToList();
    }

    private static List<Endpoint> ReadRequests(string collectionJson)
    {
        using var document = JsonDocument.Parse(collectionJson);

        return ReadRequests(document.RootElement).ToList();
    }

    private static IEnumerable<Endpoint> ReadRequests(JsonElement parent)
    {
        foreach (var item in parent.GetProperty("item").EnumerateArray())
        {
            if (item.TryGetProperty("request", out var request))
            {
                yield return new Endpoint(request.GetProperty("method").GetString(), ReadPath(request.GetProperty("url")));
            }
            else
            {
                foreach (var nested in ReadRequests(item))
                {
                    yield return nested;
                }
            }
        }
    }

    // O formato v2.1 aceita a URL como texto ou como objeto com o campo raw.
    private static string ReadPath(JsonElement url)
    {
        var raw = url.ValueKind == JsonValueKind.String ? url.GetString() : url.GetProperty("raw").GetString();
        var path = raw.Split('?')[0];

        return path.StartsWith(BaseUrl, StringComparison.Ordinal) ? path[BaseUrl.Length..] : path;
    }

    private sealed record Endpoint(string Method, string Path)
    {
        public string Key =>
            $"{Method.ToUpperInvariant()} {string.Join("/", Path.Split('/').Select(segment => PathVariable.IsMatch(segment) ? "{}" : segment))}";

        public override string ToString() => $"{Method} {Path}";
    }
}
