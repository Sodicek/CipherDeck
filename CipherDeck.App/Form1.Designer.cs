#nullable enable

namespace CipherDeck;

partial class Form1
{
    private System.ComponentModel.IContainer? components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            components?.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        var palette = UiTheme.GetPalette(darkTheme: true);
        var darkBackground = palette.Background;
        var panelBackground = palette.Surface;
        var inputBackground = palette.Content;
        var textPrimary = palette.Text;
        var textSecondary = palette.Muted;

        mainLayout = new TableLayoutPanel();
        headerPanel = new Panel();
        titleLabel = new Label();
        subtitleLabel = new Label();
        headerActionsPanel = new TableLayoutPanel();
        challengeButton = new SmoothButton();
        historyButton = new SmoothButton();
        analysisButton = new SmoothButton();
        helpButton = new SmoothButton();
        themeButton = new SmoothButton();
        optionsPanel = new RoundedTableLayoutPanel();
        cipherLabel = new Label();
        cipherSelector = new ComboBox();
        modePanel = new FlowLayoutPanel();
        encryptMode = new RadioButton();
        decryptMode = new RadioButton();
        keyPanel = new FlowLayoutPanel();
        keyLabel = new Label();
        shiftValue = new NumericUpDown();
        randomNumericKeyButton = new SmoothButton();
        textKeyPanel = new FlowLayoutPanel();
        textKeyLabel = new Label();
        textKeyInput = new TextBox();
        randomTextKeyButton = new SmoothButton();
        cipherDescription = new Label();
        editorLayout = new TableLayoutPanel();
        inputGroup = new SmoothGroupBox();
        inputText = new RichTextBox();
        outputGroup = new SmoothGroupBox();
        outputText = new RichTextBox();
        actionsPanel = new TableLayoutPanel();
        importButton = new SmoothButton();
        exportButton = new SmoothButton();
        transformButton = new SmoothButton();
        swapButton = new SmoothButton();
        copyButton = new SmoothButton();
        clearButton = new SmoothButton();
        explainButton = new SmoothButton();
        detectButton = new SmoothButton();
        livePreview = new CheckBox();
        footerPanel = new TableLayoutPanel();
        statusLabel = new Label();
        characterCount = new Label();
        toolTip = new ToolTip(components);

        ((System.ComponentModel.ISupportInitialize)shiftValue).BeginInit();
        SuspendLayout();

        mainLayout.BackColor = darkBackground;
        mainLayout.ColumnCount = 1;
        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        mainLayout.Controls.Add(headerPanel, 0, 0);
        mainLayout.Controls.Add(optionsPanel, 0, 1);
        mainLayout.Controls.Add(editorLayout, 0, 2);
        mainLayout.Controls.Add(actionsPanel, 0, 3);
        mainLayout.Controls.Add(footerPanel, 0, 4);
        mainLayout.Dock = DockStyle.Fill;
        mainLayout.Padding = new Padding(28, 20, 28, 16);
        mainLayout.RowCount = 5;
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 82F));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 114F));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100F));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));

        headerPanel.Dock = DockStyle.Fill;
        headerPanel.TabIndex = 4;
        headerPanel.Controls.Add(titleLabel);
        headerPanel.Controls.Add(subtitleLabel);
        headerPanel.Controls.Add(headerActionsPanel);

        titleLabel.AutoSize = true;
        titleLabel.Font = new Font("Segoe UI", 26F, FontStyle.Bold);
        titleLabel.ForeColor = textPrimary;
        titleLabel.Location = new Point(0, 0);
        titleLabel.Text = "CipherDeck";

        subtitleLabel.AutoSize = true;
        subtitleLabel.Font = new Font("Segoe UI", 10.5F);
        subtitleLabel.ForeColor = textSecondary;
        subtitleLabel.Location = new Point(4, 51);
        subtitleLabel.Text = "Classic ciphers. Modern interface.";

        headerActionsPanel.ColumnCount = 5;
        for (var column = 0; column < 5; column++)
            headerActionsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
        headerActionsPanel.Controls.Add(challengeButton, 0, 0);
        headerActionsPanel.Controls.Add(historyButton, 1, 0);
        headerActionsPanel.Controls.Add(analysisButton, 2, 0);
        headerActionsPanel.Controls.Add(helpButton, 3, 0);
        headerActionsPanel.Controls.Add(themeButton, 4, 0);
        headerActionsPanel.Dock = DockStyle.Right;
        headerActionsPanel.Padding = new Padding(0, 18, 0, 18);
        headerActionsPanel.RowCount = 1;
        headerActionsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        headerActionsPanel.Width = 580;

        ConfigureGridButton(challengeButton, "Výzvy", palette);
        challengeButton.TabIndex = 0;
        challengeButton.Click += ChallengeButton_Click;

        ConfigureGridButton(historyButton, "Historie (0)", palette);
        historyButton.TabIndex = 1;
        historyButton.Click += HistoryButton_Click;

        ConfigureGridButton(analysisButton, "Analýza", palette);
        analysisButton.TabIndex = 2;
        analysisButton.Click += AnalysisButton_Click;

        ConfigureGridButton(helpButton, "Nápověda", palette);
        helpButton.TabIndex = 3;
        helpButton.Click += HelpButton_Click;

        ConfigureGridButton(themeButton, "☀  Světlý", palette);
        themeButton.TabIndex = 4;
        themeButton.Click += ThemeButton_Click;

        optionsPanel.BackColor = panelBackground;
        optionsPanel.BorderColor = palette.Border;
        optionsPanel.ColumnCount = 4;
        optionsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
        optionsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 250F));
        optionsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 300F));
        optionsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        optionsPanel.Controls.Add(cipherLabel, 0, 0);
        optionsPanel.Controls.Add(cipherSelector, 1, 0);
        optionsPanel.Controls.Add(modePanel, 2, 0);
        optionsPanel.Controls.Add(keyPanel, 3, 0);
        optionsPanel.Controls.Add(textKeyPanel, 3, 0);
        optionsPanel.Controls.Add(cipherDescription, 0, 1);
        optionsPanel.Dock = DockStyle.Fill;
        optionsPanel.Margin = new Padding(0, 0, 0, 18);
        optionsPanel.Padding = new Padding(16, 12, 16, 8);
        optionsPanel.RowCount = 2;
        optionsPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 43F));
        optionsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        optionsPanel.SetColumnSpan(cipherDescription, 4);
        optionsPanel.TabIndex = 0;

        cipherLabel.Anchor = AnchorStyles.Left;
        cipherLabel.AutoSize = true;
        cipherLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        cipherLabel.ForeColor = textPrimary;
        cipherLabel.Text = "ŠIFRA";

        cipherSelector.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        cipherSelector.BackColor = inputBackground;
        cipherSelector.DropDownStyle = ComboBoxStyle.DropDownList;
        cipherSelector.FlatStyle = FlatStyle.Flat;
        cipherSelector.Font = new Font("Segoe UI", 11F);
        cipherSelector.ForeColor = textPrimary;
        cipherSelector.FormattingEnabled = true;
        cipherSelector.Margin = new Padding(0, 3, 20, 3);
        cipherSelector.TabIndex = 0;
        cipherSelector.SelectedIndexChanged += CipherSelector_SelectedIndexChanged;

        modePanel.Anchor = AnchorStyles.Left;
        modePanel.AutoSize = true;
        modePanel.Controls.Add(encryptMode);
        modePanel.Controls.Add(decryptMode);
        modePanel.FlowDirection = FlowDirection.LeftToRight;
        modePanel.Margin = new Padding(0);
        modePanel.TabIndex = 1;

        encryptMode.AutoSize = true;
        encryptMode.Font = new Font("Segoe UI", 10.5F);
        encryptMode.ForeColor = textPrimary;
        encryptMode.Margin = new Padding(0, 7, 18, 0);
        encryptMode.Text = "Zašifrovat";
        encryptMode.TabIndex = 0;
        encryptMode.CheckedChanged += PreviewSettingChanged;

        decryptMode.AutoSize = true;
        decryptMode.Font = new Font("Segoe UI", 10.5F);
        decryptMode.ForeColor = textPrimary;
        decryptMode.Margin = new Padding(0, 7, 0, 0);
        decryptMode.Text = "Odšifrovat";
        decryptMode.TabIndex = 1;
        decryptMode.CheckedChanged += PreviewSettingChanged;

        keyPanel.Anchor = AnchorStyles.Left;
        keyPanel.AutoSize = true;
        keyPanel.Controls.Add(keyLabel);
        keyPanel.Controls.Add(shiftValue);
        keyPanel.Controls.Add(randomNumericKeyButton);
        keyPanel.Margin = new Padding(0);
        keyPanel.TabIndex = 2;
        keyPanel.Visible = false;

        keyLabel.AutoSize = true;
        keyLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        keyLabel.ForeColor = textPrimary;
        keyLabel.Margin = new Padding(0, 8, 10, 0);
        keyLabel.Text = "POSUN";

        shiftValue.BackColor = inputBackground;
        shiftValue.BorderStyle = BorderStyle.FixedSingle;
        shiftValue.Font = new Font("Segoe UI", 11F);
        shiftValue.ForeColor = textPrimary;
        shiftValue.Maximum = 25;
        shiftValue.Minimum = 1;
        shiftValue.Value = 3;
        shiftValue.Width = 70;
        shiftValue.TabIndex = 0;
        shiftValue.ValueChanged += PreviewSettingChanged;

        ConfigureButton(randomNumericKeyButton, "⟳", 34, palette);
        randomNumericKeyButton.Height = 29;
        randomNumericKeyButton.Margin = new Padding(7, 3, 0, 0);
        randomNumericKeyButton.AccessibleName = "Vygenerovat náhodný klíč";
        randomNumericKeyButton.TabIndex = 1;
        randomNumericKeyButton.Click += GenerateKeyButton_Click;

        textKeyPanel.Anchor = AnchorStyles.Left;
        textKeyPanel.AutoSize = true;
        textKeyPanel.Controls.Add(textKeyLabel);
        textKeyPanel.Controls.Add(textKeyInput);
        textKeyPanel.Controls.Add(randomTextKeyButton);
        textKeyPanel.Margin = new Padding(0);
        textKeyPanel.TabIndex = 2;
        textKeyPanel.Visible = false;

        textKeyLabel.AutoSize = true;
        textKeyLabel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        textKeyLabel.ForeColor = textPrimary;
        textKeyLabel.Margin = new Padding(0, 8, 10, 0);
        textKeyLabel.Text = "KLÍČ";

        textKeyInput.BackColor = inputBackground;
        textKeyInput.BorderStyle = BorderStyle.FixedSingle;
        textKeyInput.Font = new Font("Segoe UI", 11F);
        textKeyInput.ForeColor = textPrimary;
        textKeyInput.Margin = new Padding(0, 3, 0, 0);
        textKeyInput.MaxLength = 64;
        textKeyInput.Width = 135;
        textKeyInput.TabIndex = 0;
        textKeyInput.TextChanged += PreviewSettingChanged;

        ConfigureButton(randomTextKeyButton, "⟳", 34, palette);
        randomTextKeyButton.Height = 29;
        randomTextKeyButton.Margin = new Padding(7, 3, 0, 0);
        randomTextKeyButton.AccessibleName = "Vygenerovat náhodný klíč";
        randomTextKeyButton.TabIndex = 1;
        randomTextKeyButton.Click += GenerateKeyButton_Click;

        cipherDescription.Anchor = AnchorStyles.Left;
        cipherDescription.AutoEllipsis = true;
        cipherDescription.AutoSize = true;
        cipherDescription.Font = new Font("Segoe UI", 10F);
        cipherDescription.ForeColor = textSecondary;
        cipherDescription.Margin = new Padding(0, 4, 0, 0);

        editorLayout.ColumnCount = 2;
        editorLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        editorLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        editorLayout.Controls.Add(inputGroup, 0, 0);
        editorLayout.Controls.Add(outputGroup, 1, 0);
        editorLayout.Dock = DockStyle.Fill;
        editorLayout.RowCount = 1;
        editorLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        editorLayout.TabIndex = 1;

        inputGroup.Controls.Add(inputText);
        inputGroup.BackColor = inputBackground;
        inputGroup.BorderColor = palette.Border;
        inputGroup.Dock = DockStyle.Fill;
        inputGroup.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        inputGroup.ForeColor = textPrimary;
        inputGroup.Margin = new Padding(0, 0, 10, 0);
        inputGroup.Padding = new Padding(12, 10, 12, 12);
        inputGroup.Text = "  VSTUP  ";
        inputGroup.TabIndex = 0;
        inputGroup.TabStop = false;

        inputText.AcceptsTab = true;
        inputText.BackColor = inputBackground;
        inputText.BorderStyle = BorderStyle.None;
        inputText.Dock = DockStyle.Fill;
        inputText.Font = new Font("Segoe UI", 12F);
        inputText.ForeColor = textPrimary;
        inputText.TabIndex = 0;
        inputText.TextChanged += InputText_TextChanged;

        outputGroup.Controls.Add(outputText);
        outputGroup.BackColor = inputBackground;
        outputGroup.BorderColor = palette.Border;
        outputGroup.Dock = DockStyle.Fill;
        outputGroup.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        outputGroup.ForeColor = textPrimary;
        outputGroup.Margin = new Padding(10, 0, 0, 0);
        outputGroup.Padding = new Padding(12, 10, 12, 12);
        outputGroup.Text = "  VÝSTUP  ";
        outputGroup.TabIndex = 1;
        outputGroup.TabStop = false;

        outputText.BackColor = inputBackground;
        outputText.BorderStyle = BorderStyle.None;
        outputText.Dock = DockStyle.Fill;
        outputText.Font = new Font("Segoe UI", 12F);
        outputText.ForeColor = Color.FromArgb(196, 181, 253);
        outputText.ReadOnly = true;
        outputText.TabStop = false;

        actionsPanel.ColumnCount = 4;
        for (var column = 0; column < 4; column++)
            actionsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        actionsPanel.Controls.Add(transformButton, 0, 0);
        actionsPanel.Controls.Add(swapButton, 1, 0);
        actionsPanel.Controls.Add(copyButton, 2, 0);
        actionsPanel.Controls.Add(clearButton, 3, 0);
        actionsPanel.Controls.Add(importButton, 0, 1);
        actionsPanel.Controls.Add(exportButton, 1, 1);
        actionsPanel.Controls.Add(explainButton, 2, 1);
        actionsPanel.Controls.Add(detectButton, 3, 1);
        actionsPanel.Dock = DockStyle.Fill;
        actionsPanel.Padding = new Padding(0, 6, 0, 6);
        actionsPanel.RowCount = 2;
        actionsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        actionsPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        actionsPanel.TabIndex = 2;

        ConfigureGridButton(importButton, "Načíst · Ctrl+O", palette);
        importButton.TabIndex = 4;
        importButton.Click += ImportButton_Click;
        ConfigureGridButton(exportButton, "Exportovat · Ctrl+S", palette);
        exportButton.TabIndex = 5;
        exportButton.Click += ExportButton_Click;
        ConfigureGridButton(transformButton, "Provést  ·  Ctrl+Enter", palette, primary: true);
        transformButton.TabIndex = 0;
        transformButton.Click += TransformButton_Click;
        ConfigureGridButton(swapButton, "⇄  Prohodit", palette);
        swapButton.TabIndex = 1;
        swapButton.Click += SwapButton_Click;
        ConfigureGridButton(copyButton, "Kopírovat", palette);
        copyButton.TabIndex = 2;
        copyButton.Click += CopyButton_Click;
        ConfigureGridButton(clearButton, "Vymazat", palette);
        clearButton.TabIndex = 3;
        clearButton.Click += ClearButton_Click;
        ConfigureGridButton(explainButton, "Jak to funguje", palette);
        explainButton.TabIndex = 6;
        explainButton.Click += ExplainButton_Click;
        ConfigureGridButton(detectButton, "Odhad šifry", palette);
        detectButton.TabIndex = 7;
        detectButton.Click += DetectButton_Click;

        livePreview.AutoSize = true;
        livePreview.Checked = true;
        livePreview.CheckState = CheckState.Checked;
        livePreview.Font = new Font("Segoe UI", 10F);
        livePreview.ForeColor = textPrimary;
        livePreview.Anchor = AnchorStyles.Right;
        livePreview.Margin = new Padding(0, 0, 28, 0);
        livePreview.Text = "Živý náhled";
        livePreview.TabIndex = 0;
        livePreview.CheckedChanged += LivePreview_CheckedChanged;

        footerPanel.ColumnCount = 3;
        footerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        footerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        footerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        footerPanel.Controls.Add(statusLabel, 0, 0);
        footerPanel.Controls.Add(livePreview, 1, 0);
        footerPanel.Controls.Add(characterCount, 2, 0);
        footerPanel.Dock = DockStyle.Fill;
        footerPanel.TabIndex = 3;

        toolTip.AutoPopDelay = 8000;
        toolTip.InitialDelay = 450;
        toolTip.ReshowDelay = 100;
        toolTip.SetToolTip(challengeButton, "Otevřít šifrovací výzvy ve třech obtížnostech.");
        toolTip.SetToolTip(historyButton, "Zobrazit posledních 30 provedených operací.");
        toolTip.SetToolTip(analysisButton, "Analyzovat četnost písmen ve vstupu nebo výstupu.");
        toolTip.SetToolTip(helpButton, "Otevřít nápovědu a informace o aplikaci.");
        toolTip.SetToolTip(themeButton, "Přepnout světlý a tmavý motiv.");
        toolTip.SetToolTip(transformButton, "Provést vybranou operaci a uložit ji do historie (Ctrl+Enter).");
        toolTip.SetToolTip(swapButton, "Přesunout výstup zpět do vstupu.");
        toolTip.SetToolTip(copyButton, "Zkopírovat výstup do schránky.");
        toolTip.SetToolTip(clearButton, "Vymazat vstup i výstup.");
        toolTip.SetToolTip(importButton, "Načíst textový soubor v UTF-8 (Ctrl+O).");
        toolTip.SetToolTip(exportButton, "Otevřít export TXT a sdílitelných PNG kartiček (Ctrl+S).");
        toolTip.SetToolTip(explainButton, "Ukázat aktuální šifrování krok za krokem.");
        toolTip.SetToolTip(detectButton, "Odhadnout použitou jednoduchou šifru.");
        toolTip.SetToolTip(livePreview, "Automaticky přepočítá výsledek krátce po změně vstupu.");

        statusLabel.Anchor = AnchorStyles.Left;
        statusLabel.AutoSize = true;
        statusLabel.Font = new Font("Segoe UI", 9.5F);
        statusLabel.ForeColor = Color.FromArgb(134, 239, 172);
        statusLabel.Text = "Připraveno";

        characterCount.Anchor = AnchorStyles.Right;
        characterCount.AutoSize = true;
        characterCount.Font = new Font("Segoe UI", 9.5F);
        characterCount.ForeColor = textSecondary;

        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = darkBackground;
        ClientSize = new Size(1120, 720);
        Controls.Add(mainLayout);
        Font = new Font("Segoe UI", 9F);
        KeyPreview = true;
        MinimumSize = new Size(900, 640);
        Name = "Form1";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "CipherDeck · v0.9";
        KeyDown += Form1_KeyDown;

        ((System.ComponentModel.ISupportInitialize)shiftValue).EndInit();
        ResumeLayout(false);
    }

    private static void ConfigureButton(Button button, string text, int width, UiPalette palette, bool primary = false) =>
        UiStyles.ConfigureButton(button, text, width, palette, primary);

    private static void ConfigureGridButton(Button button, string text, UiPalette palette, bool primary = false)
    {
        UiStyles.ConfigureButton(button, text, 100, palette, primary);
        button.Dock = DockStyle.Fill;
        button.Margin = new Padding(5, 4, 5, 4);
    }

    private TableLayoutPanel mainLayout = null!;
    private Panel headerPanel = null!;
    private Label titleLabel = null!;
    private Label subtitleLabel = null!;
    private TableLayoutPanel headerActionsPanel = null!;
    private Button challengeButton = null!;
    private Button historyButton = null!;
    private Button analysisButton = null!;
    private Button helpButton = null!;
    private Button themeButton = null!;
    private RoundedTableLayoutPanel optionsPanel = null!;
    private Label cipherLabel = null!;
    private ComboBox cipherSelector = null!;
    private FlowLayoutPanel modePanel = null!;
    private RadioButton encryptMode = null!;
    private RadioButton decryptMode = null!;
    private FlowLayoutPanel keyPanel = null!;
    private Label keyLabel = null!;
    private NumericUpDown shiftValue = null!;
    private Button randomNumericKeyButton = null!;
    private FlowLayoutPanel textKeyPanel = null!;
    private Label textKeyLabel = null!;
    private TextBox textKeyInput = null!;
    private Button randomTextKeyButton = null!;
    private Label cipherDescription = null!;
    private TableLayoutPanel editorLayout = null!;
    private SmoothGroupBox inputGroup = null!;
    private RichTextBox inputText = null!;
    private SmoothGroupBox outputGroup = null!;
    private RichTextBox outputText = null!;
    private TableLayoutPanel actionsPanel = null!;
    private Button importButton = null!;
    private Button exportButton = null!;
    private Button transformButton = null!;
    private Button swapButton = null!;
    private Button copyButton = null!;
    private Button clearButton = null!;
    private Button explainButton = null!;
    private Button detectButton = null!;
    private CheckBox livePreview = null!;
    private TableLayoutPanel footerPanel = null!;
    private Label statusLabel = null!;
    private Label characterCount = null!;
    private ToolTip toolTip = null!;
}
