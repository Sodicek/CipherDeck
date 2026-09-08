using Xunit;

namespace CipherDeck.Tests;

public sealed class ShareCardRendererTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void RenderCreatesExpectedImageForEveryStyle(int styleValue)
    {
        var data = new ShareCardData("Tajná zpráva", "KHOOR ZRUOG", "Caesarova šifra", "Zašifrováno");

        using var image = ShareCardRenderer.Render(data, (ShareCardStyle)styleValue);

        Assert.Equal(ShareCardRenderer.CardWidth, image.Width);
        Assert.Equal(ShareCardRenderer.CardHeight, image.Height);
    }

    [Fact]
    public void RenderHandlesBlankTitleAndLongUnicodeText()
    {
        var text = string.Concat(Enumerable.Repeat("Příliš žluťoučký kůň 🔐 ", 80));
        var data = new ShareCardData("   ", text, "Atbash", "Zašifrováno");

        using var image = ShareCardRenderer.Render(data, ShareCardStyle.Violet);

        Assert.Equal(ShareCardRenderer.CardWidth, image.Width);
        Assert.Equal(ShareCardRenderer.CardHeight, image.Height);
    }

    [Fact]
    public void CardStylesProduceDifferentBackgrounds()
    {
        var data = new ShareCardData("Zpráva", "TEST", "Atbash", "Zašifrováno");

        using var violet = ShareCardRenderer.Render(data, ShareCardStyle.Violet);
        using var paper = ShareCardRenderer.Render(data, ShareCardStyle.Paper);

        Assert.NotEqual(violet.GetPixel(0, 0), paper.GetPixel(0, 0));
    }
}
