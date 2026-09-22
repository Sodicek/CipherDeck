using System.Globalization;
using System.Runtime.ExceptionServices;
using CipherDeck.Core.Analysis;
using CipherDeck.Core.Detection;
using CipherDeck.Core.Learning;
using Xunit;

namespace CipherDeck.Tests;

[Collection("Culture-sensitive tests")]
public sealed class DpiLayoutTests
{
    [Theory]
    [InlineData("cs-CZ")]
    [InlineData("en-US")]
    public void AllWindowsKeepLabelsAndButtonsVisibleAtScaledSizes(string cultureName)
    {
        var originalDefaultCulture = CultureInfo.DefaultThreadCurrentCulture;
        var originalDefaultUiCulture = CultureInfo.DefaultThreadCurrentUICulture;
        try
        {
            RunOnSta(() =>
            {
                CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(cultureName);
                using var host = new Form
                {
                    ClientSize = new Size(100, 100),
                    Location = new Point(-10000, -10000),
                    Opacity = 0,
                    ShowInTaskbar = false,
                    StartPosition = FormStartPosition.Manual
                };
                host.Show();
                foreach (var scale in new[] { 1F, 1.25F, 1.5F, 2F })
                {
                    foreach (var minimumSize in new[] { false, true })
                    {
                        foreach (var form in CreateWindows(cultureName))
                        {
                            using (form)
                            {
                                form.ShowInTaskbar = false;
                                form.TopLevel = false;
                                host.Controls.Add(form);
                                Assert.Equal(AutoScaleMode.Dpi, form.AutoScaleMode);
                                var baseWidth = form.ClientSize.Width;
                                var currentDpi = form.CurrentAutoScaleDimensions;
                                form.AutoScaleDimensions = new SizeF(currentDpi.Width / scale, currentDpi.Height / scale);
                                Assert.True(form.ClientSize.Width >= baseWidth * scale - 8,
                                    $"AutoScale did not resize {form.GetType().Name} at {scale:P0}: {baseWidth} -> {form.ClientSize.Width}");
                                form.Show();
                                if (minimumSize)
                                    form.Size = form.MinimumSize;
                                Application.DoEvents();
                                form.PerformLayout();

                                foreach (var control in Descendants(form))
                                {
                                    if (!control.Visible)
                                        continue;

                                    var context = $"{form.GetType().Name}, {cultureName}, {scale:P0}, minimum={minimumSize}, {control.GetType().Name}: {control.Text}";
                                    Assert.True(control.Right <= control.Parent!.ClientSize.Width + 2,
                                        $"Right edge outside parent: {context}; bounds={control.Bounds}, parent={control.Parent.ClientSize}, form={form.Size}");
                                    Assert.True(control.Bottom <= control.Parent.ClientSize.Height + 2,
                                        $"Bottom edge outside parent: {context}; bounds={control.Bounds}, parent={control.Parent.ClientSize}");
                                    if (control is not (Label or Button) || string.IsNullOrEmpty(control.Text))
                                        continue;

                                    var textSize = TextRenderer.MeasureText(control.Text, control.Font, Size.Empty,
                                        TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
                                    var horizontalPadding = control is Button ? 12 : 0;
                                    var measurementTolerance = control is Label ? 6 : 2;
                                    Assert.True(textSize.Width + horizontalPadding <= control.ClientSize.Width + measurementTolerance,
                                        $"Text clipped horizontally: {context}; text={textSize.Width}, control={control.ClientSize.Width}");
                                    Assert.True(textSize.Height <= control.ClientSize.Height + 2,
                                        $"Text clipped vertically: {context}; text={textSize.Height}, control={control.ClientSize.Height}");
                                }
                                form.Close();
                                host.Controls.Remove(form);
                            }
                        }
                    }
                }
                host.Close();
            });
        }
        finally
        {
            CultureInfo.DefaultThreadCurrentCulture = originalDefaultCulture;
            CultureInfo.DefaultThreadCurrentUICulture = originalDefaultUiCulture;
        }
    }

    private static Form[] CreateWindows(string cultureName) =>
    [
        new MainForm(
            new AppPreferences { LanguageCode = cultureName.StartsWith("en", StringComparison.Ordinal) ? AppLanguage.English : AppLanguage.Czech },
            new HistoryService(() => [], _ => true)),
        new ChallengeForm(true),
        new DetectionForm(Array.Empty<CipherDetection>(), true),
        new HistoryForm(Array.Empty<HistoryEntry>(), true),
        new ExplanationForm(new CipherExplanation("test", "Test", "Summary", "Result", [new ExplanationStep("Step", "Detail", "Snapshot")]), true),
        new ExportForm(new ShareCardData("Title", "Text", "Cipher", "Encrypt"), true),
        new AnalysisForm([new LetterFrequency("A", 1, 100)], true),
        new AboutForm(true)
    ];

    private static IEnumerable<Control> Descendants(Control parent)
    {
        foreach (Control child in parent.Controls)
        {
            yield return child;
            foreach (var descendant in Descendants(child))
                yield return descendant;
        }
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
