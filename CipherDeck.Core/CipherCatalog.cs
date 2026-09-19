using CipherDeck.Core.Ciphers;

namespace CipherDeck.Core;

public static class CipherCatalog
{
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

    public static ICipher? FindByName(string? name) => All.FirstOrDefault(
        cipher => string.Equals(cipher.Name, name, StringComparison.CurrentCulture));
}
