using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;

namespace CipherDeck;

internal enum ShareCardStyle
{
    Violet,
    Midnight,
    Paper
}

internal sealed record ShareCardData(
    string Title,
    string Text,
    string CipherName,
    string OperationLabel);

internal static class ShareCardRenderer
{
    public const int CardWidth = 1200;
    public const int CardHeight = 630;

    public static Bitmap Render(ShareCardData data, ShareCardStyle style)
    {
        var colors = GetColors(style);
        var bitmap = new Bitmap(CardWidth, CardHeight);
        bitmap.SetResolution(96, 96);

        using var graphics = Graphics.FromImage(bitmap);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
        graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

        using (var background = new LinearGradientBrush(
                   new Rectangle(0, 0, CardWidth, CardHeight), colors.BackgroundStart, colors.BackgroundEnd, 24F))
        {
            graphics.FillRectangle(background, 0, 0, CardWidth, CardHeight);
        }

        DrawDecoration(graphics, colors);
        DrawHeader(graphics, colors);
        DrawMessageCard(graphics, colors, data);
        DrawFooter(graphics, colors, data);
        return bitmap;
    }

    private static void DrawDecoration(Graphics graphics, CardColors colors)
    {
        using var largeGlow = new SolidBrush(Color.FromArgb(28, colors.Accent));
        using var smallGlow = new SolidBrush(Color.FromArgb(18, colors.SecondaryAccent));
        graphics.FillEllipse(largeGlow, 900, -190, 430, 430);
        graphics.FillEllipse(smallGlow, -140, 430, 360, 360);

        using var linePen = new Pen(Color.FromArgb(38, colors.Accent), 2F);
        for (var offset = 0; offset < 5; offset++)
            graphics.DrawLine(linePen, 940 + offset * 34, 60, 1120 + offset * 34, 240);
    }

    private static void DrawHeader(Graphics graphics, CardColors colors)
    {
        using var brandFont = new Font("Segoe UI", 30F, FontStyle.Bold);
        using var taglineFont = new Font("Segoe UI Semibold", 11F);
        using var brandBrush = new SolidBrush(colors.Text);
        using var mutedBrush = new SolidBrush(colors.MutedText);
        graphics.DrawString("CipherDeck", brandFont, brandBrush, 68, 50);
        graphics.DrawString("CLASSIC CIPHERS · MODERN INTERFACE", taglineFont, mutedBrush, 72, 100);

        var badgeBounds = new RectangleF(920, 60, 210, 44);
        using var badgePath = SmoothButton.CreateRoundedPath(badgeBounds, 22);
        using var badgeBrush = new SolidBrush(Color.FromArgb(42, colors.Accent));
        graphics.FillPath(badgeBrush, badgePath);
        using var badgeFont = new Font("Segoe UI Semibold", 11F);
        using var accentBrush = new SolidBrush(colors.Accent);
        using var badgeFormat = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        graphics.DrawString("SHARE CARD", badgeFont, accentBrush, badgeBounds, badgeFormat);
    }

    private static void DrawMessageCard(Graphics graphics, CardColors colors, ShareCardData data)
    {
        var cardBounds = new RectangleF(68, 145, 1064, 350);
        using var cardPath = SmoothButton.CreateRoundedPath(cardBounds, 30);
        using var cardBrush = new SolidBrush(colors.CardSurface);
        using var borderPen = new Pen(colors.CardBorder, 1.5F);
        graphics.FillPath(cardBrush, cardPath);
        graphics.DrawPath(borderPen, cardPath);

        using var accentBrush = new SolidBrush(colors.Accent);
        using var accentPath = SmoothButton.CreateRoundedPath(new RectangleF(104, 183, 8, 56), 4);
        graphics.FillPath(accentBrush, accentPath);

        using var titleFont = new Font("Segoe UI Semibold", 15F);
        using var mutedBrush = new SolidBrush(colors.MutedText);
        graphics.DrawString(NormalizeTitle(data.Title).ToUpperInvariant(), titleFont, mutedBrush, 132, 190);

        var preparedText = PrepareMessage(data.Text);
        using var messageFont = new Font("Segoe UI", GetMessageFontSize(preparedText.Length), FontStyle.Bold);
        using var textBrush = new SolidBrush(colors.Text);
        using var messageFormat = new StringFormat
        {
            Alignment = StringAlignment.Near,
            LineAlignment = StringAlignment.Center,
            Trimming = StringTrimming.EllipsisCharacter,
            FormatFlags = StringFormatFlags.LineLimit
        };
        graphics.DrawString(preparedText, messageFont, textBrush, new RectangleF(104, 245, 992, 205), messageFormat);
    }

    private static void DrawFooter(Graphics graphics, CardColors colors, ShareCardData data)
    {
        using var metaFont = new Font("Segoe UI Semibold", 12F);
        using var noteFont = new Font("Segoe UI", 10F);
        using var accentBrush = new SolidBrush(colors.Accent);
        using var mutedBrush = new SolidBrush(colors.MutedText);

        var cipherName = string.IsNullOrWhiteSpace(data.CipherName) ? "Bez vybrané šifry" : data.CipherName.Trim();
        graphics.DrawString($"{cipherName}  ·  {data.OperationLabel}", metaFont, accentBrush, 72, 540);
        graphics.DrawString("Vytvořeno v CipherDecku · klasické šifry jsou určené pro výuku a zábavu", noteFont, mutedBrush, 72, 578);
    }

    internal static string PrepareMessage(string text)
    {
        var prepared = text.Replace("\t", "    ").Trim();
        var elementIndexes = StringInfo.ParseCombiningCharacters(prepared);
        return elementIndexes.Length <= 700
            ? prepared
            : string.Concat(prepared.AsSpan(0, elementIndexes[697]), "…");
    }

    private static string NormalizeTitle(string title) => string.IsNullOrWhiteSpace(title)
        ? "Šifrovaná zpráva"
        : title.Trim();

    private static float GetMessageFontSize(int length) => length switch
    {
        <= 55 => 42F,
        <= 110 => 34F,
        <= 220 => 27F,
        <= 400 => 22F,
        _ => 18F
    };

    private static CardColors GetColors(ShareCardStyle style) => style switch
    {
        ShareCardStyle.Midnight => new CardColors(
            Color.FromArgb(2, 6, 23),
            Color.FromArgb(8, 47, 73),
            Color.FromArgb(34, 211, 238),
            Color.FromArgb(129, 140, 248),
            Color.FromArgb(248, 250, 252),
            Color.FromArgb(165, 180, 202),
            Color.FromArgb(184, 15, 23, 42),
            Color.FromArgb(68, 103, 232, 249)),
        ShareCardStyle.Paper => new CardColors(
            Color.FromArgb(248, 250, 252),
            Color.FromArgb(226, 232, 240),
            Color.FromArgb(109, 40, 217),
            Color.FromArgb(14, 165, 233),
            Color.FromArgb(15, 23, 42),
            Color.FromArgb(71, 85, 105),
            Color.FromArgb(226, 255, 255, 255),
            Color.FromArgb(82, 148, 163, 184)),
        _ => new CardColors(
            Color.FromArgb(15, 23, 42),
            Color.FromArgb(76, 29, 149),
            Color.FromArgb(167, 139, 250),
            Color.FromArgb(34, 211, 238),
            Color.FromArgb(248, 250, 252),
            Color.FromArgb(203, 213, 225),
            Color.FromArgb(166, 17, 24, 39),
            Color.FromArgb(70, 196, 181, 253))
    };

    private sealed record CardColors(
        Color BackgroundStart,
        Color BackgroundEnd,
        Color Accent,
        Color SecondaryAccent,
        Color Text,
        Color MutedText,
        Color CardSurface,
        Color CardBorder);
}
