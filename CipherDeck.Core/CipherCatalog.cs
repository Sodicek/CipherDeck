using CipherDeck.Core.Ciphers;

namespace CipherDeck.Core;

public static class CipherCatalog
{
    private static readonly IReadOnlyDictionary<string, string> LegacyNames =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Pozpátku"] = CipherIds.Reverse,
            ["Caesarova šifra"] = CipherIds.Caesar,
            ["Atbash"] = CipherIds.Atbash,
            ["Vigenèrova šifra"] = CipherIds.Vigenere,
            ["Rail Fence"] = CipherIds.RailFence,
            ["Přeskakování"] = CipherIds.Skip
        };

    public static IReadOnlyList<ICipher> All { get; } =
    [
        new ReverseCipher(),
        new CaesarCipher(),
        new AtbashCipher(),
        new VigenereCipher(),
        new RailFenceCipher(),
        new SkipCipher()
    ];

    public static ICipher? FindById(string? id) => All.FirstOrDefault(
        cipher => string.Equals(cipher.Id, id, StringComparison.Ordinal));

    public static ICipher? FindByName(string? name)
    {
        var localizedMatch = All.FirstOrDefault(
            cipher => string.Equals(cipher.Name, name, StringComparison.CurrentCulture));
        if (localizedMatch is not null)
            return localizedMatch;

        return name is not null && LegacyNames.TryGetValue(name, out var id) ? FindById(id) : null;
    }
}
