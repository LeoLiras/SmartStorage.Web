using Microsoft.JSInterop;

namespace SmartStorage.Blazor.Tests;

/// <summary>
/// localStorage que guarda o que recebe: o refresh troca os dois tokens e o logout os
/// apaga, e o teste confere o que sobrou, não só as chamadas feitas.
/// </summary>
public class LocalStorageFalso : IJSRuntime
{
    public Dictionary<string, string> Itens { get; } = new();

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
    {
        var chave = (string)args![0]!;

        switch (identifier)
        {
            case "localStorage.getItem":
                return ValueTask.FromResult((TValue)(object)Itens.GetValueOrDefault(chave)!);
            case "localStorage.setItem":
                Itens[chave] = (string)args[1]!;
                break;
            case "localStorage.removeItem":
                Itens.Remove(chave);
                break;
        }

        return ValueTask.FromResult(default(TValue)!);
    }

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        => InvokeAsync<TValue>(identifier, args);
}
