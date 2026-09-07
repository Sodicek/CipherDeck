using CipherDeck.Core.Challenges;

namespace CipherDeck;

internal sealed class ChallengeForm : Form
{
    private readonly bool _darkTheme;
    private readonly ComboBox _difficulty;
    private readonly RichTextBox _cipherText;
    private readonly RichTextBox _answer;
    private readonly Label _hint;
    private readonly Label _status;
    private CipherChallenge? _challenge;

    public ChallengeForm(bool darkTheme)
    {
        _darkTheme = darkTheme;
        var palette = UiTheme.GetPalette(darkTheme);
        var background = palette.Background;
        var input = palette.Content;
        var text = palette.Text;
        var secondary = palette.Muted;

        Text = "CipherDeck · Výzvy";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(780, 620);
        MinimumSize = new Size(650, 540);
        BackColor = background;
        Font = new Font("Segoe UI", 10F);
        KeyPreview = true;
        KeyDown += (_, eventArgs) =>
        {
            if (eventArgs.Control && eventArgs.KeyCode == Keys.Enter)
                CheckAnswer();
        };

        var layout = new TableLayoutPanel
        {
            BackColor = background,
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Padding = new Padding(26),
            RowCount = 9
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));

        var heading = new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI", 20F, FontStyle.Bold),
            ForeColor = text,
            Text = "Rozlušti zprávu"
        };
        _difficulty = new ComboBox
        {
            BackColor = input,
            DropDownStyle = ComboBoxStyle.DropDownList,
            FlatStyle = FlatStyle.Flat,
            ForeColor = text,
            Width = 190
        };
        _difficulty.Items.AddRange([
            new DifficultyOption("Lehká", ChallengeDifficulty.Easy),
            new DifficultyOption("Střední", ChallengeDifficulty.Medium),
            new DifficultyOption("Těžká", ChallengeDifficulty.Hard)
        ]);
        _difficulty.SelectedIndexChanged += (_, _) => NewChallenge();

        var cipherLabel = CreateLabel("ZAŠIFROVANÁ ZPRÁVA", text, bold: true);
        _cipherText = new RichTextBox
        {
            BackColor = input,
            BorderStyle = BorderStyle.None,
            Dock = DockStyle.Fill,
            Font = new Font("Cascadia Mono", 13F),
            ForeColor = palette.OutputText,
            ReadOnly = true
        };
        _hint = new Label
        {
            AutoEllipsis = true,
            Dock = DockStyle.Fill,
            ForeColor = secondary,
            Padding = new Padding(0, 12, 0, 0),
            Text = "Nápověda je zatím skrytá."
        };
        var answerLabel = CreateLabel("TVŮJ ROZLUŠTĚNÝ TEXT", text, bold: true);
        _answer = new RichTextBox
        {
            BackColor = input,
            BorderStyle = BorderStyle.None,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 12F),
            ForeColor = text
        };
        _status = new Label { AutoSize = true, ForeColor = secondary, Text = "Zkus najít původní zprávu." };

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(0, 12, 0, 0) };
        var checkButton = UiStyles.CreateButton("Zkontrolovat · Ctrl+Enter", 205, palette, primary: true);
        var hintButton = UiStyles.CreateButton("Nápověda", 115, palette);
        var revealButton = UiStyles.CreateButton("Odhalit", 105, palette);
        var newButton = UiStyles.CreateButton("Nová výzva", 120, palette);
        checkButton.Click += (_, _) => CheckAnswer();
        hintButton.Click += (_, _) => ShowHint();
        revealButton.Click += (_, _) => RevealAnswer();
        newButton.Click += (_, _) => NewChallenge();
        buttons.Controls.Add(checkButton);
        buttons.Controls.Add(hintButton);
        buttons.Controls.Add(revealButton);
        buttons.Controls.Add(newButton);

        layout.Controls.Add(heading, 0, 0);
        layout.Controls.Add(_difficulty, 0, 1);
        layout.Controls.Add(cipherLabel, 0, 2);
        layout.Controls.Add(_cipherText, 0, 3);
        layout.Controls.Add(_hint, 0, 4);
        layout.Controls.Add(answerLabel, 0, 5);
        layout.Controls.Add(_answer, 0, 6);
        layout.Controls.Add(_status, 0, 7);
        layout.Controls.Add(buttons, 0, 8);
        Controls.Add(layout);

        _difficulty.SelectedIndex = 0;
    }

    private void NewChallenge()
    {
        if (_difficulty.SelectedItem is not DifficultyOption option)
            return;

        _challenge = ChallengeGenerator.Generate(option.Value);
        _cipherText.Text = _challenge.EncryptedText;
        _answer.Clear();
        _hint.Text = "Nápověda je zatím skrytá.";
        _status.ForeColor = UiTheme.GetPalette(_darkTheme).Muted;
        _status.Text = "Zkus najít původní zprávu.";
        _answer.Focus();
    }

    private void CheckAnswer()
    {
        if (_challenge is null || string.IsNullOrWhiteSpace(_answer.Text))
            return;

        var correct = _challenge.IsCorrect(_answer.Text);
        var palette = UiTheme.GetPalette(_darkTheme);
        _status.ForeColor = correct ? palette.Success : palette.Error;
        _status.Text = correct ? "Správně! Výzva je vyřešená." : "Ještě ne. Zkontroluj pořadí a jednotlivá písmena.";
    }

    private void ShowHint()
    {
        if (_challenge is not null)
            _hint.Text = $"NÁPOVĚDA · {_challenge.Hint}";
    }

    private void RevealAnswer()
    {
        if (_challenge is null)
            return;

        var key = _challenge.Key?.Number?.ToString() ?? _challenge.Key?.Text ?? "bez klíče";
        _answer.Text = _challenge.PlainText;
        _status.ForeColor = UiTheme.GetPalette(_darkTheme).OutputText;
        _status.Text = $"Použitá šifra: {_challenge.CipherName} · klíč: {key}";
    }

    private static Label CreateLabel(string text, Color color, bool bold) => new()
    {
        Anchor = AnchorStyles.Left,
        AutoSize = true,
        Font = new Font("Segoe UI", 9F, bold ? FontStyle.Bold : FontStyle.Regular),
        ForeColor = color,
        Text = text
    };

    private sealed record DifficultyOption(string Name, ChallengeDifficulty Value)
    {
        public override string ToString() => Name;
    }
}
