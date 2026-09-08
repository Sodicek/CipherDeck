namespace CipherDeck;

internal sealed class HistoryForm : Form
{
    private readonly ListBox _historyList;
    private readonly TextBox _preview;

    public HistoryEntry? SelectedEntry { get; private set; }

    public bool ClearRequested { get; private set; }

    public HistoryForm(IReadOnlyList<HistoryEntry> entries, bool darkTheme)
    {
        var palette = UiTheme.GetPalette(darkTheme);
        var background = palette.Background;
        var panel = palette.Surface;
        var input = palette.Content;
        var text = palette.Text;
        var secondary = palette.Muted;

        Text = "CipherDeck · Historie";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(760, 500);
        MinimumSize = new Size(620, 420);
        BackColor = background;
        Font = new Font("Segoe UI", 10F);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(22),
            ColumnCount = 1,
            RowCount = 4,
            BackColor = background
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));

        var heading = new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI", 20F, FontStyle.Bold),
            ForeColor = text,
            Text = "Historie operací"
        };

        _historyList = new ListBox
        {
            BackColor = input,
            BorderStyle = BorderStyle.None,
            DisplayMember = nameof(HistoryEntry.DisplayText),
            Dock = DockStyle.Fill,
            ForeColor = text,
            IntegralHeight = false,
            DataSource = entries.ToList()
        };
        _historyList.SelectedIndexChanged += (_, _) => ShowPreview();
        _historyList.DoubleClick += (_, _) => SelectAndClose();

        _preview = new TextBox
        {
            BackColor = panel,
            BorderStyle = BorderStyle.None,
            Dock = DockStyle.Fill,
            ForeColor = secondary,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Margin = new Padding(0, 12, 0, 0)
        };

        var buttonPanel = new TableLayoutPanel
        {
            ColumnCount = 3,
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 8, 0, 2),
            RowCount = 1
        };
        for (var column = 0; column < 3; column++)
            buttonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / 3F));
        var useButton = UiStyles.CreateGridButton("Načíst operaci", palette, primary: true);
        var closeButton = UiStyles.CreateGridButton("Zavřít", palette);
        var clearButton = UiStyles.CreateGridButton("Vymazat historii", palette);
        closeButton.DialogResult = DialogResult.Cancel;
        useButton.Click += (_, _) => SelectAndClose();
        closeButton.Click += (_, _) => Close();
        clearButton.Click += (_, _) => ClearHistory();
        buttonPanel.Controls.Add(clearButton, 0, 0);
        buttonPanel.Controls.Add(closeButton, 1, 0);
        buttonPanel.Controls.Add(useButton, 2, 0);

        layout.Controls.Add(heading, 0, 0);
        layout.Controls.Add(_historyList, 0, 1);
        layout.Controls.Add(_preview, 0, 2);
        layout.Controls.Add(buttonPanel, 0, 3);
        Controls.Add(layout);
        AcceptButton = useButton;
        CancelButton = closeButton;

        if (entries.Count == 0)
        {
            _preview.Text = "Historie je zatím prázdná. Operace se přidá po stisknutí tlačítka PROVÉST.";
            useButton.Enabled = false;
            clearButton.Enabled = false;
        }
        else
        {
            _historyList.SelectedIndex = 0;
        }
    }

    private void ShowPreview()
    {
        if (_historyList.SelectedItem is not HistoryEntry entry)
            return;

        _preview.Text = $"VSTUP{Environment.NewLine}{entry.Input}{Environment.NewLine}{Environment.NewLine}" +
                        $"VÝSTUP{Environment.NewLine}{entry.Output}";
    }

    private void SelectAndClose()
    {
        if (_historyList.SelectedItem is not HistoryEntry entry)
            return;

        SelectedEntry = entry;
        DialogResult = DialogResult.OK;
        Close();
    }

    private void ClearHistory()
    {
        var answer = MessageBox.Show(
            this,
            "Opravdu chceš trvale vymazat celou historii operací?",
            "Vymazat historii",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (answer != DialogResult.Yes)
            return;

        ClearRequested = true;
        Close();
    }

}
