namespace CipherDeck.Core;

public interface ICipher
{
    string Id { get; }
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

    string Encrypt(string input, CipherKey? key, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = Encrypt(input, key);
        cancellationToken.ThrowIfCancellationRequested();
        return result;
    }

    string Decrypt(string input, CipherKey? key, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = Decrypt(input, key);
        cancellationToken.ThrowIfCancellationRequested();
        return result;
    }
}
