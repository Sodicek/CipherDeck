using CipherDeck.Core.Learning;

namespace CipherDeck;

internal sealed class ExplanationForm : Form
{
    private readonly CipherExplanation _explanation;
    private readonly Label _stepCounter;
    private readonly Label _stepTitle;
    private readonly Label _stepDetail;
    private readonly RichTextBox _snapshot;
    private readonly Button _previousButton;
    private readonly Button _nextButton;
    private int _currentStep;

    public ExplanationForm(CipherExplanation explanation, bool darkTheme)
    {
        _explanation = explanation;
        var palette = UiTheme.GetPalette(darkTheme);
        var background = palette.Background;
        var input = palette.Content;
        var text = palette.Text;
        var secondary = palette.Muted;

        Text = $"CipherDeck · Jak funguje {explanation.CipherName}";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(780, 570);
        MinimumSize = new Size(650, 500);
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
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58));

        var heading = new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI", 20F, FontStyle.Bold),
            ForeColor = text,
            Text = explanation.CipherName
        };
        var summary = new Label
        {
            AutoSize = true,
            ForeColor = secondary,
            Text = explanation.Summary
        };
        _stepCounter = new Label
        {
            Anchor = AnchorStyles.Left,
            AutoSize = true,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = palette.AccentHover
        };
        _stepTitle = new Label
        {
            Anchor = AnchorStyles.Left,
            AutoSize = true,
            Font = new Font("Segoe UI", 16F, FontStyle.Bold),
            ForeColor = text
        };
        var stepHeader = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            Margin = new Padding(0)
        };
        _stepCounter.Margin = new Padding(0, 13, 20, 0);
        _stepTitle.Margin = new Padding(0, 7, 0, 0);
        stepHeader.Controls.Add(_stepCounter);
        stepHeader.Controls.Add(_stepTitle);
        _stepDetail = new Label
        {
            AutoEllipsis = true,
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 10.5F),
            ForeColor = secondary
        };
        _snapshot = new RichTextBox
        {
            BackColor = input,
            BorderStyle = BorderStyle.None,
            Dock = DockStyle.Fill,
            Font = new Font("Cascadia Mono", 12F),
            ForeColor = palette.OutputText,
            ReadOnly = true,
            ScrollBars = RichTextBoxScrollBars.Both,
            WordWrap = true
        };

        var buttons = new TableLayoutPanel
        {
            ColumnCount = 3,
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 8, 0, 2),
            RowCount = 1
        };
        for (var column = 0; column < 3; column++)
            buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / 3F));
        _nextButton = UiStyles.CreateGridButton("Další →", palette, primary: true);
        _previousButton = UiStyles.CreateGridButton("← Předchozí", palette);
        var closeButton = UiStyles.CreateGridButton("Zavřít", palette);
        _nextButton.Click += (_, _) => AdvanceOrClose();
        _previousButton.Click += (_, _) => MoveStep(-1);
        closeButton.Click += (_, _) => Close();
        buttons.Controls.Add(closeButton, 0, 0);
        buttons.Controls.Add(_previousButton, 1, 0);
        buttons.Controls.Add(_nextButton, 2, 0);

        layout.Controls.Add(heading, 0, 0);
        layout.Controls.Add(summary, 0, 1);
        layout.Controls.Add(stepHeader, 0, 2);
        layout.Controls.Add(_stepDetail, 0, 3);
        layout.Controls.Add(_snapshot, 0, 4);
        layout.Controls.Add(buttons, 0, 5);
        Controls.Add(layout);

        ShowCurrentStep();
    }

    private void MoveStep(int direction)
    {
        _currentStep = Math.Clamp(_currentStep + direction, 0, _explanation.Steps.Count - 1);
        ShowCurrentStep();
    }

    private void AdvanceOrClose()
    {
        if (_currentStep == _explanation.Steps.Count - 1)
        {
            Close();
            return;
        }

        MoveStep(1);
    }

    private void ShowCurrentStep()
    {
        var step = _explanation.Steps[_currentStep];
        _stepCounter.Text = $"KROK {_currentStep + 1}/{_explanation.Steps.Count}";
        _stepTitle.Text = step.Title;
        _stepDetail.Text = step.Detail;
        _snapshot.Text = step.Snapshot;
        _previousButton.Enabled = _currentStep > 0;
        _nextButton.Text = _currentStep == _explanation.Steps.Count - 1 ? "Hotovo" : "Další →";
    }

}
