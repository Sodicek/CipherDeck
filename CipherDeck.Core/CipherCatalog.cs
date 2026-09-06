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
}
