namespace CipherDeck.Core;

public interface ICipher
{
    string Name { get; }
    string Description { get; }
    CipherKeyType KeyType { get; }
    string KeyLabel { get; }
    int MinimumNumericKey { get; }
    int MaximumNumericKey { get; }
    int DefaultNumericKey { get; }
    string DefaultTextKey { get; }
    string Encrypt(string input, CipherKey? key = null);
    string Decrypt(string input, CipherKey? key = null);
}
