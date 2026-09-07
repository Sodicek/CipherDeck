using System.Drawing.Drawing2D;

namespace CipherDeck;

internal readonly record struct UiPalette(
    Color Background,
    Color Surface,
    Color SurfaceHover,
    Color Content,
    Color Border,
    Color Text,
    Color Muted,
    Color Accent,
    Color AccentHover,
    Color AccentPressed,
    Color AccentText,
    Color OutputText,
    Color Success,
    Color Error);

internal static class UiTheme
{
    public static UiPalette GetPalette(bool darkTheme) => darkTheme
        ? new UiPalette(
            Color.FromArgb(10, 17, 32),
            Color.FromArgb(27, 38, 58),
            Color.FromArgb(40, 53, 76),
            Color.FromArgb(16, 25, 42),
            Color.FromArgb(55, 69, 94),
            Color.FromArgb(241, 245, 249),
            Color.FromArgb(148, 163, 184),
            Color.FromArgb(124, 58, 237),
            Color.FromArgb(139, 78, 246),
            Color.FromArgb(109, 40, 217),
            Color.White,
            Color.FromArgb(196, 181, 253),
            Color.FromArgb(134, 239, 172),
            Color.FromArgb(253, 164, 175))
        : new UiPalette(
            Color.FromArgb(244, 247, 251),
            Color.White,
            Color.FromArgb(238, 242, 249),
            Color.FromArgb(249, 250, 252),
            Color.FromArgb(216, 223, 235),
            Color.FromArgb(15, 23, 42),
            Color.FromArgb(71, 85, 105),
            Color.FromArgb(109, 40, 217),
            Color.FromArgb(124, 58, 237),
            Color.FromArgb(91, 33, 182),
            Color.White,
            Color.FromArgb(91, 33, 182),
            Color.FromArgb(22, 101, 52),
            Color.FromArgb(190, 18, 60));
}

internal static class UiStyles
{
    public static SmoothButton CreateButton(string text, int width, UiPalette palette, bool primary = false) =>
        ConfigureButton(new SmoothButton(), text, width, palette, primary);

    public static T ConfigureButton<T>(T button, string text, int width, UiPalette palette, bool primary = false)
        where T : Button
    {
        button.Cursor = Cursors.Hand;
        button.Font = new Font("Segoe UI Semibold", 9.5F);
        button.Height = 38;
        button.Margin = new Padding(0, 0, 10, 0);
        button.Text = text;
        button.Width = width;
        ApplyButtonTheme(button, palette, primary);
        return button;
    }

    public static void ApplyButtonTheme(Button button, UiPalette palette, bool primary = false)
    {
        var background = primary ? palette.Accent : palette.Surface;
        button.BackColor = background;
        button.ForeColor = primary ? palette.AccentText : palette.Text;
        button.FlatAppearance.BorderSize = 0;
        button.FlatStyle = FlatStyle.Flat;
        button.UseVisualStyleBackColor = false;

        if (button is not SmoothButton smoothButton)
            return;

        smoothButton.HoverBackColor = primary ? palette.AccentHover : palette.SurfaceHover;
        smoothButton.PressedBackColor = primary ? palette.AccentPressed : palette.Border;
        smoothButton.BorderColor = primary ? Color.Transparent : palette.Border;
        smoothButton.FocusColor = palette.AccentHover;
    }
}

internal sealed class SmoothButton : Button
{
    private bool _hovered;
    private bool _pressed;
    private int _cornerRadius = 10;

    public SmoothButton()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.UserPaint, true);
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        UseVisualStyleBackColor = false;
    }

    public Color HoverBackColor { get; set; }

    public Color PressedBackColor { get; set; }

    public Color BorderColor { get; set; } = Color.Transparent;

    public Color FocusColor { get; set; } = Color.Transparent;

    public int CornerRadius
    {
        get => _cornerRadius;
        set
        {
            _cornerRadius = Math.Max(2, value);
            UpdateRegion();
            Invalidate();
        }
    }

    protected override void OnMouseEnter(EventArgs eventArgs)
    {
        _hovered = true;
        Invalidate();
        base.OnMouseEnter(eventArgs);
    }

    protected override void OnMouseLeave(EventArgs eventArgs)
    {
        _hovered = false;
        _pressed = false;
        Invalidate();
        base.OnMouseLeave(eventArgs);
    }

    protected override void OnMouseDown(MouseEventArgs eventArgs)
    {
        _pressed = eventArgs.Button == MouseButtons.Left;
        Invalidate();
        base.OnMouseDown(eventArgs);
    }

    protected override void OnMouseUp(MouseEventArgs eventArgs)
    {
        _pressed = false;
        Invalidate();
        base.OnMouseUp(eventArgs);
    }

    protected override void OnKeyDown(KeyEventArgs eventArgs)
    {
        if (eventArgs.KeyCode is Keys.Space or Keys.Enter)
        {
            _pressed = true;
            Invalidate();
        }

        base.OnKeyDown(eventArgs);
    }

    protected override void OnKeyUp(KeyEventArgs eventArgs)
    {
        if (eventArgs.KeyCode is Keys.Space or Keys.Enter)
        {
            _pressed = false;
            Invalidate();
        }

        base.OnKeyUp(eventArgs);
    }

    protected override void OnMouseCaptureChanged(EventArgs eventArgs)
    {
        _pressed = false;
        Invalidate();
        base.OnMouseCaptureChanged(eventArgs);
    }

    protected override void OnEnabledChanged(EventArgs eventArgs)
    {
        Invalidate();
        base.OnEnabledChanged(eventArgs);
    }

    protected override void OnGotFocus(EventArgs eventArgs)
    {
        Invalidate();
        base.OnGotFocus(eventArgs);
    }

    protected override void OnLostFocus(EventArgs eventArgs)
    {
        Invalidate();
        base.OnLostFocus(eventArgs);
    }

    protected override void OnResize(EventArgs eventArgs)
    {
        base.OnResize(eventArgs);
        UpdateRegion();
    }

    protected override void OnPaint(PaintEventArgs eventArgs)
    {
        eventArgs.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        var bounds = new RectangleF(0.5F, 0.5F, Width - 1F, Height - 1F);
        using var path = CreateRoundedPath(bounds, CornerRadius);
        var fillColor = _pressed && PressedBackColor != Color.Empty
            ? PressedBackColor
            : _hovered && HoverBackColor != Color.Empty
                ? HoverBackColor
                : BackColor;
        if (!Enabled)
            fillColor = Blend(fillColor, Parent?.BackColor ?? SystemColors.Control, 0.55F);

        using var fillBrush = new SolidBrush(fillColor);
        eventArgs.Graphics.FillPath(fillBrush, path);

        if (BorderColor != Color.Transparent)
        {
            using var borderPen = new Pen(BorderColor);
            eventArgs.Graphics.DrawPath(borderPen, path);
        }

        if (Focused && ShowFocusCues && FocusColor != Color.Transparent)
        {
            var focusBounds = RectangleF.Inflate(bounds, -2.5F, -2.5F);
            using var focusPath = CreateRoundedPath(focusBounds, Math.Max(3, CornerRadius - 2));
            using var focusPen = new Pen(FocusColor, 1.5F);
            eventArgs.Graphics.DrawPath(focusPen, focusPath);
        }

        var textColor = Enabled ? ForeColor : Blend(ForeColor, fillColor, 0.55F);
        TextRenderer.DrawText(
            eventArgs.Graphics,
            Text,
            Font,
            ClientRectangle,
            textColor,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
            TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
    }

    private void UpdateRegion()
    {
        if (Width <= 0 || Height <= 0)
            return;

        using var path = CreateRoundedPath(new RectangleF(0, 0, Width, Height), CornerRadius);
        Region?.Dispose();
        Region = new Region(path);
    }

    internal static GraphicsPath CreateRoundedPath(RectangleF bounds, float radius)
    {
        var diameter = Math.Min(radius * 2, Math.Min(bounds.Width, bounds.Height));
        var arc = new RectangleF(bounds.X, bounds.Y, diameter, diameter);
        var path = new GraphicsPath();
        path.AddArc(arc, 180, 90);
        arc.X = bounds.Right - diameter;
        path.AddArc(arc, 270, 90);
        arc.Y = bounds.Bottom - diameter;
        path.AddArc(arc, 0, 90);
        arc.X = bounds.Left;
        path.AddArc(arc, 90, 90);
        path.CloseFigure();
        return path;
    }

    private static Color Blend(Color source, Color target, float amount) => Color.FromArgb(
        (int)(source.R + (target.R - source.R) * amount),
        (int)(source.G + (target.G - source.G) * amount),
        (int)(source.B + (target.B - source.B) * amount));
}

internal sealed class RoundedTableLayoutPanel : TableLayoutPanel
{
    private int _cornerRadius = 14;

    public RoundedTableLayoutPanel()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw, true);
    }

    public Color BorderColor { get; set; } = Color.Transparent;

    public int CornerRadius
    {
        get => _cornerRadius;
        set
        {
            _cornerRadius = Math.Max(2, value);
            UpdateRegion();
            Invalidate();
        }
    }

    protected override void OnResize(EventArgs eventArgs)
    {
        base.OnResize(eventArgs);
        UpdateRegion();
    }

    protected override void OnPaint(PaintEventArgs eventArgs)
    {
        base.OnPaint(eventArgs);
        if (BorderColor == Color.Transparent)
            return;

        eventArgs.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var path = SmoothButton.CreateRoundedPath(
            new RectangleF(0.5F, 0.5F, Width - 1F, Height - 1F), CornerRadius);
        using var pen = new Pen(BorderColor);
        eventArgs.Graphics.DrawPath(pen, path);
    }

    private void UpdateRegion()
    {
        if (Width <= 0 || Height <= 0)
            return;

        using var path = SmoothButton.CreateRoundedPath(new RectangleF(0, 0, Width, Height), CornerRadius);
        Region?.Dispose();
        Region = new Region(path);
    }
}

internal sealed class SmoothGroupBox : GroupBox
{
    public SmoothGroupBox()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.UserPaint, true);
    }

    public Color BorderColor { get; set; } = Color.Transparent;

    protected override void OnPaint(PaintEventArgs eventArgs)
    {
        eventArgs.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        eventArgs.Graphics.Clear(Parent?.BackColor ?? BackColor);

        var textSize = TextRenderer.MeasureText(Text, Font);
        var borderBounds = new RectangleF(0.5F, textSize.Height / 2F, Width - 1F, Height - textSize.Height / 2F - 1F);
        using var path = SmoothButton.CreateRoundedPath(borderBounds, 14);
        using var fillBrush = new SolidBrush(BackColor);
        eventArgs.Graphics.FillPath(fillBrush, path);

        if (BorderColor != Color.Transparent)
        {
            using var borderPen = new Pen(BorderColor);
            eventArgs.Graphics.DrawPath(borderPen, path);
        }

        var labelBounds = new Rectangle(14, 0, textSize.Width + 8, textSize.Height);
        using var labelBrush = new SolidBrush(Parent?.BackColor ?? BackColor);
        eventArgs.Graphics.FillRectangle(labelBrush, labelBounds);
        TextRenderer.DrawText(eventArgs.Graphics, Text, Font, labelBounds, ForeColor,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
    }
}
