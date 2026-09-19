using CipherDeck.Core.Ciphers;
using CipherDeck.Core.Localization;

namespace CipherDeck.Core.Challenges;

public static class ChallengeGenerator
{
    public static CipherChallenge Generate(ChallengeDifficulty difficulty, Random? random = null)
    {
        random ??= Random.Shared;
        var (phrases, ciphers) = difficulty switch
        {
            ChallengeDifficulty.Easy => (CoreText.GetList("ChallengeEasyPhrases"), new ICipher[] { new ReverseCipher(), new AtbashCipher() }),
            ChallengeDifficulty.Medium => (CoreText.GetList("ChallengeMediumPhrases"), new ICipher[] { new CaesarCipher(), new SkipCipher() }),
            ChallengeDifficulty.Hard => (CoreText.GetList("ChallengeHardPhrases"), new ICipher[] { new VigenereCipher(), new RailFenceCipher() }),
            _ => throw new ArgumentOutOfRangeException(nameof(difficulty))
        };

        var plainText = phrases[random.Next(phrases.Length)];
        var cipher = ciphers[random.Next(ciphers.Length)];
        var key = CipherKeyGenerator.Generate(cipher, random);
        var encrypted = cipher.Encrypt(plainText, key);

        return new CipherChallenge(
            difficulty,
            plainText,
            encrypted,
            cipher.Id,
            cipher.Name,
            key,
            CreateHint(cipher, key));
    }

    private static string CreateHint(ICipher cipher, CipherKey? key) => cipher switch
    {
        ReverseCipher => CoreText.Get("ChallengeReverseHint"),
        AtbashCipher => CoreText.Get("ChallengeAtbashHint"),
        CaesarCipher => CoreText.Format("ChallengeCaesarHint", key?.Number),
        SkipCipher => CoreText.Format("ChallengeSkipHint", key?.Number),
        VigenereCipher => CoreText.Format("ChallengeVigenereHint", key?.Text?.Length),
        RailFenceCipher => CoreText.Format("ChallengeRailFenceHint", key?.Number),
        _ => cipher.Description
    };
}
