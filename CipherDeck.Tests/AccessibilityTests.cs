using System.Globalization;
using System.Runtime.ExceptionServices;
using CipherDeck.Core.Analysis;
using CipherDeck.Core.Detection;
using CipherDeck.Core.Learning;
using Xunit;

namespace CipherDeck.Tests;

public sealed class AccessibilityTests
{
    [Theory]
    [InlineData("cs-CZ", true)]
    [InlineData("cs-CZ", false)]
    [InlineData("en-US", true)]
    [InlineData("en-US", false)]
    public void SecondaryWindowsNameEveryInteractiveControl(string cultureName, bool darkTheme)
    {
        RunOnSta(() =>
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(cultureName);
            using var challenge = new ChallengeForm(darkTheme);
            using var detection = new DetectionForm(Array.Empty<CipherDetection>(), darkTheme);
            using var history = new HistoryForm(Array.Empty<HistoryEntry>(), darkTheme);
            using var explanation = new ExplanationForm(
                new CipherExplanation("test", "Test", "Summary", "Result", [new ExplanationStep("Step", "Detail", "Snapshot")]),
                darkTheme);
            using var export = new ExportForm(new ShareCardData("Title", "Text", "Cipher", "Encrypt"), darkTheme);
            using var analysis = new AnalysisForm([new LetterFrequency("A", 1, 100)], darkTheme);
            using var about = new AboutForm(darkTheme);

            foreach (var form in new Form[] { challenge, detection, history, explanation, export, analysis, about })
            {
                if (form is not ChallengeForm)
                {
                    Assert.NotNull(form.AcceptButton);
                    Assert.NotNull(form.CancelButton);
                }

                foreach (var control in Descendants(form))
                {
                    if (control is TextBoxBase or ComboBox or ListBox or DataGridView or PictureBox or FrequencyChartPanel)
                        Assert.False(string.IsNullOrWhiteSpace(control.AccessibleName), $"{form.GetType().Name}: {control.GetType().Name}");

                    if (control is TextBoxBase)
                        Assert.True(control.TabStop, $"{form.GetType().Name}: {control.GetType().Name} cannot be reached by Tab");

                    if (control is ButtonBase)
                        Assert.True(!string.IsNullOrWhiteSpace(control.AccessibleName) ||
                                    !string.IsNullOrWhiteSpace(control.Text),
                            $"{form.GetType().Name}: unnamed {control.GetType().Name}");
                }
            }
        });
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void TextColorsMeetNormalTextContrast(bool darkTheme)
    {
        var palette = UiTheme.GetPalette(darkTheme);
        AssertContrast(palette.Text, palette.Background);
        AssertContrast(palette.Text, palette.Surface);
        AssertContrast(palette.Text, palette.SurfaceHover);
        AssertContrast(palette.Text, palette.Border);
        AssertContrast(palette.Text, palette.Content);
        AssertContrast(palette.Muted, palette.Background);
        AssertContrast(palette.Muted, palette.Surface);
        AssertContrast(palette.OutputText, palette.Content);
        AssertContrast(palette.AccentText, palette.Accent);
        AssertContrast(palette.AccentText, palette.AccentHover);
        AssertContrast(palette.AccentText, palette.AccentPressed);
        AssertContrast(palette.Success, palette.Background);
        AssertContrast(palette.Error, palette.Background);
    }

    private static IEnumerable<Control> Descendants(Control parent)
    {
        foreach (Control child in parent.Controls)
        {
            yield return child;
            foreach (var descendant in Descendants(child))
                yield return descendant;
        }
    }

    private static void AssertContrast(Color foreground, Color background)
    {
        var light = Math.Max(Luminance(foreground), Luminance(background));
        var dark = Math.Min(Luminance(foreground), Luminance(background));
        Assert.True((light + 0.05) / (dark + 0.05) >= 4.5,
            $"Contrast below 4.5:1 for {foreground} on {background}");
    }

    private static double Luminance(Color color) =>
        0.2126 * Linear(color.R) + 0.7152 * Linear(color.G) + 0.0722 * Linear(color.B);

    private static double Linear(byte channel)
    {
        var value = channel / 255.0;
        return value <= 0.04045 ? value / 12.92 : Math.Pow((value + 0.055) / 1.055, 2.4);
    }

    private static void RunOnSta(Action action)
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try { action(); }
            catch (Exception exception) { failure = exception; }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure is not null)
            ExceptionDispatchInfo.Capture(failure).Throw();
    }
}
