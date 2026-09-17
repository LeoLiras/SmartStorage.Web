using System.Net;
using System.Text;
using System.Text.Json;

namespace SmartStorage.Blazor.Tests;

/// <summary>
/// HttpMessageHandler que responde o que o teste registrar e guarda o corpo de
/// cada requisicao. O corpo e guardado como texto cru de proposito: os dois bugs
/// de front que estes testes cobrem estavam no que o Blazor serializava, nao no
/// objeto em memoria.
/// </summary>
public class ApiFalsa : HttpMessageHandler
{
    private readonly Dictionary<string, (HttpStatusCode, string, int?)> _respostas = new();

    public List<(HttpMethod Metodo, string Caminho, string Corpo)> Requisicoes { get; } = new();

    public List<string> Consultas { get; } = new();

    public ApiFalsa Responde(HttpMethod metodo, string caminho, object corpo,
                             HttpStatusCode status = HttpStatusCode.OK, int? total = null)
    {
        var json = corpo is string texto ? texto : JsonSerializer.Serialize(corpo, OpcoesJson);

        _respostas[Chave(metodo, caminho)] = (status, json, total);

        return this;
    }

    public HttpClient Cliente() => new(this, disposeHandler: false)
    {
        BaseAddress = new Uri("http://localhost/"),
    };

    public string CorpoDe(HttpMethod metodo, string caminho)
    {
        var achado = Requisicoes.LastOrDefault(
            r => r.Metodo == metodo && r.Caminho.TrimEnd('/') == caminho.TrimEnd('/'));

        if (achado.Caminho is null)
            throw new InvalidOperationException(
                $"nenhuma requisicao {metodo} para {caminho}. Houve: " +
                string.Join(", ", Requisicoes.Select(r => $"{r.Metodo} {r.Caminho}")));

        return achado.Corpo;
    }

    public JsonDocument JsonDe(HttpMethod metodo, string caminho)
        => JsonDocument.Parse(CorpoDe(metodo, caminho));

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var caminho = request.RequestUri!.AbsolutePath.TrimStart('/');
        var corpo = request.Content is null
            ? string.Empty
            : await request.Content.ReadAsStringAsync(cancellationToken);

        Requisicoes.Add((request.Method, caminho, corpo));
        Consultas.Add(request.RequestUri.Query);

        if (!_respostas.TryGetValue(Chave(request.Method, caminho), out var resposta))
            return new HttpResponseMessage(HttpStatusCode.NotFound)
            {
                Content = new StringContent(
                    $"ApiFalsa nao tem resposta registrada para {request.Method} {caminho}")
            };

        var mensagem = new HttpResponseMessage(resposta.Item1)
        {
            Content = new StringContent(resposta.Item2, Encoding.UTF8, "application/json"),
        };

        if (resposta.Item3 is int total)
            mensagem.Headers.Add("X-Total-Count", total.ToString());

        return mensagem;
    }

    private static string Chave(HttpMethod metodo, string caminho)
        => $"{metodo} {caminho.Trim('/')}";

    public static readonly JsonSerializerOptions OpcoesJson = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };
}
