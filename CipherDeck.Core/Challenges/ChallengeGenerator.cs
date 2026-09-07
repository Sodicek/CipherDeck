using CipherDeck.Core.Ciphers;

namespace CipherDeck.Core.Challenges;

public static class ChallengeGenerator
{
    private static readonly string[] EasyPhrases =
    [
        "TAJNA ZPRAVA",
        "AHOJ SVETE",
        "NAJDI KLIC",
        "SIFRY BAVI"
    ];

    private static readonly string[] MediumPhrases =
    [
        "KAZDA SIFRA MA SVUJ PRINCIP",
        "TRPELIVOST ODEMYKA TAJEMSTVI",
        "ZNAKY SE MOHOU PREMISTIT",
        "DOBRY LUSTITEL HLEDA VZORY"
    ];

    private static readonly string[] HardPhrases =
    [
        "NEJCENNEJSI NAPOVEDA BYVA UKRYTA V DETAILU",
        "FREKVENCE PISMEN MUZE PROZRADIT DRUH SIFRY",
        "TAJNY KLIC MUSI ZNAT ODESILATEL I PRIJEMCE",
        "KDYZ SELZE PRVNI NAPAD ZKUS JINY UHEL POHLEDU"
    ];

    public static CipherChallenge Generate(ChallengeDifficulty difficulty, Random? random = null)
    {
        random ??= Random.Shared;
        var (phrases, ciphers) = difficulty switch
        {
            ChallengeDifficulty.Easy => (EasyPhrases, new ICipher[] { new ReverseCipher(), new AtbashCipher() }),
            ChallengeDifficulty.Medium => (MediumPhrases, new ICipher[] { new CaesarCipher(), new SkipCipher() }),
            ChallengeDifficulty.Hard => (HardPhrases, new ICipher[] { new VigenereCipher(), new RailFenceCipher() }),
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
            cipher.Name,
            key,
            CreateHint(cipher, key));
    }

    private static string CreateHint(ICipher cipher, CipherKey? key) => cipher switch
    {
        ReverseCipher => "Začni od konce.",
        AtbashCipher => "První písmeno abecedy se páruje s posledním.",
        CaesarCipher => $"Každé písmeno je posunuté o {key?.Number} míst.",
        SkipCipher => $"Text byl rozdělen do {key?.Number} sloupců.",
        VigenereCipher => $"Opakující se klíč má {key?.Text?.Length} písmen.",
        RailFenceCipher => $"Text běží cikcak přes {key?.Number} řádky.",
        _ => cipher.Description
    };
}
