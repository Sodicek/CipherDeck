using CipherDeck.Core.Analysis;

namespace CipherDeck;

internal sealed class FrequencyChartPanel : Panel
{
    private IReadOnlyList<LetterFrequency> _data = [];
    private bool _darkTheme;

    public FrequencyChartPanel()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
    }

    public void SetData(IReadOnlyList<LetterFrequency> data, bool darkTheme)
    {
        _data = data.Take(12).ToList();
        _darkTheme = darkTheme;
        BackColor = darkTheme ? Color.FromArgb(17, 24, 39) : Color.White;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (_data.Count == 0 || ClientSize.Width < 100 || ClientSize.Height < 100)
            return;

        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        var textColor = _darkTheme ? Color.FromArgb(203, 213, 225) : Color.FromArgb(51, 65, 85);
        var gridColor = _darkTheme ? Color.FromArgb(51, 65, 85) : Color.FromArgb(203, 213, 225);
        var chartArea = new Rectangle(48, 18, ClientSize.Width - 66, ClientSize.Height - 58);
        var maximum = _data.Max(item => item.Count);
        var slotWidth = chartArea.Width / (float)_data.Count;
        using var gridPen = new Pen(gridColor, 1);
        using var labelBrush = new SolidBrush(textColor);
        using var barBrush = new SolidBrush(Color.FromArgb(124, 58, 237));
        using var accentBrush = new SolidBrush(Color.FromArgb(34, 211, 238));
        using var labelFont = new Font("Segoe UI", 9F, FontStyle.Bold);
        using var percentFont = new Font("Segoe UI", 7.5F);

        e.Graphics.DrawLine(gridPen, chartArea.Left, chartArea.Bottom, chartArea.Right, chartArea.Bottom);

        for (var index = 0; index < _data.Count; index++)
        {
            var item = _data[index];
            var barHeight = Math.Max(3, chartArea.Height * item.Count / (float)maximum);
            var barWidth = Math.Max(8, slotWidth * 0.58F);
            var x = chartArea.Left + index * slotWidth + (slotWidth - barWidth) / 2;
            var bar = new RectangleF(x, chartArea.Bottom - barHeight, barWidth, barHeight);
            e.Graphics.FillRectangle(index == 0 ? accentBrush : barBrush, bar);

            var symbolSize = e.Graphics.MeasureString(item.Symbol, labelFont);
            e.Graphics.DrawString(item.Symbol, labelFont, labelBrush, x + (barWidth - symbolSize.Width) / 2, chartArea.Bottom + 5);

            var percentage = $"{item.Percentage:0.#}%";
            var percentSize = e.Graphics.MeasureString(percentage, percentFont);
            var percentY = Math.Max(chartArea.Top, bar.Top - percentSize.Height - 2);
            e.Graphics.DrawString(percentage, percentFont, labelBrush, x + (barWidth - percentSize.Width) / 2, percentY);
        }
    }
}
