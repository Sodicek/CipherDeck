namespace CipherDeck.Core;

public static class CipherKeyGenerator
{
    private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public static CipherKey? Generate(ICipher cipher, Random? random = null)
    {
        ArgumentNullException.ThrowIfNull(cipher);
        random ??= Random.Shared;

        return cipher.KeyType switch
        {
            CipherKeyType.Number => new CipherKey(Number: random.Next(
                cipher.MinimumNumericKey,
                cipher.MaximumNumericKey + 1)),
            CipherKeyType.Text => new CipherKey(Text: new string(
                Enumerable.Range(0, 8)
                    .Select(_ => Alphabet[random.Next(Alphabet.Length)])
                    .ToArray())),
            _ => null
        };
    }
}
