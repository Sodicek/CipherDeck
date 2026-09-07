using CipherDeck.Core.Detection;

namespace CipherDeck;

internal sealed class DetectionForm : Form
{
    private readonly IReadOnlyList<CipherDetection> _detections;
    private readonly ListBox _results;
    private readonly Label _reason;
    private readonly RichTextBox _preview;

    public CipherDetection? SelectedDetection { get; private set; }

    public DetectionForm(string input, bool darkTheme)
    {
        _detections = CipherDetector.Detect(input);
        var background = darkTheme ? Color.FromArgb(15, 23, 42) : Color.FromArgb(241, 245, 249);
        var panel = darkTheme ? Color.FromArgb(30, 41, 59) : Color.White;
        var content = darkTheme ? Color.FromArgb(17, 24, 39) : Color.FromArgb(248, 250, 252);
        var text = darkTheme ? Color.FromArgb(241, 245, 249) : Color.FromArgb(15, 23, 42);
        var secondary = darkTheme ? Color.FromArgb(148, 163, 184) : Color.FromArgb(71, 85, 105);

        Text = "CipherDeck · Odhad šifry";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(780, 560);
        MinimumSize = new Size(650, 480);
        BackColor = background;
        Font = new Font("Segoe UI", 10F);

        var layout = new TableLayoutPanel
        {
            BackColor = background,
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Padding = new Padding(26),
            RowCount = 6
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 38));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 62));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));

        var heading = new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI", 20F, FontStyle.Bold),
            ForeColor = text,
            Text = "Pravděpodobný typ šifry"
        };
        var disclaimer = new Label
        {
            AutoSize = true,
            ForeColor = secondary,
            Text = "Heuristický odhad pro Pozpátku, Atbash a Caesarovu šifru — ne zaručený výsledek."
        };
        _results = new ListBox
        {
            BackColor = content,
            BorderStyle = BorderStyle.None,
            Dock = DockStyle.Fill,
            ForeColor = text,
            IntegralHeight = false
        };
        foreach (var detection in _detections)
            _results.Items.Add($"{detection.Confidence:P0}   {detection.CipherName}");
        _results.SelectedIndexChanged += (_, _) => ShowSelection();

        _reason = new Label
        {
            AutoEllipsis = true,
            Dock = DockStyle.Fill,
            ForeColor = secondary,
            Padding = new Padding(0, 10, 0, 0)
        };
        _preview = new RichTextBox
        {
            BackColor = content,
            BorderStyle = BorderStyle.None,
            Dock = DockStyle.Fill,
            Font = new Font("Cascadia Mono", 12F),
            ForeColor = darkTheme ? Color.FromArgb(196, 181, 253) : Color.FromArgb(91, 33, 182),
            ReadOnly = true
        };

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 12, 0, 0)
        };
        var useButton = CreateButton("Použít návrh", Color.FromArgb(124, 58, 237), Color.White, 140);
        var closeButton = CreateButton("Zavřít", panel, text, 100);
        useButton.Click += (_, _) => SelectAndClose();
        closeButton.Click += (_, _) => Close();
        buttons.Controls.Add(useButton);
        buttons.Controls.Add(closeButton);

        layout.Controls.Add(heading, 0, 0);
        layout.Controls.Add(disclaimer, 0, 1);
        layout.Controls.Add(_results, 0, 2);
        layout.Controls.Add(_reason, 0, 3);
        layout.Controls.Add(_preview, 0, 4);
        layout.Controls.Add(buttons, 0, 5);
        Controls.Add(layout);

        if (_detections.Count > 0)
            _results.SelectedIndex = 0;
        else
            useButton.Enabled = false;
    }

    private void ShowSelection()
    {
        if (_results.SelectedIndex < 0 || _results.SelectedIndex >= _detections.Count)
            return;

        var detection = _detections[_results.SelectedIndex];
        _reason.Text = detection.Reason;
        _preview.Text = detection.SuggestedPlainText;
    }

    private void SelectAndClose()
    {
        if (_results.SelectedIndex < 0 || _results.SelectedIndex >= _detections.Count)
            return;

        SelectedDetection = _detections[_results.SelectedIndex];
        DialogResult = DialogResult.OK;
        Close();
    }

    private static Button CreateButton(string text, Color background, Color foreground, int width) => new()
    {
        BackColor = background,
        FlatStyle = FlatStyle.Flat,
        ForeColor = foreground,
        Height = 36,
        Text = text,
        UseVisualStyleBackColor = false,
        Width = width
    };
}
