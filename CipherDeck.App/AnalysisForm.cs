using CipherDeck.Core.Analysis;

namespace CipherDeck;

internal sealed class AnalysisForm : Form
{
    public AnalysisForm(string analyzedText, bool darkTheme)
    {
        var frequencies = FrequencyAnalyzer.AnalyzeLetters(analyzedText);
        var background = darkTheme ? Color.FromArgb(15, 23, 42) : Color.FromArgb(241, 245, 249);
        var panel = darkTheme ? Color.FromArgb(30, 41, 59) : Color.White;
        var input = darkTheme ? Color.FromArgb(17, 24, 39) : Color.FromArgb(248, 250, 252);
        var text = darkTheme ? Color.FromArgb(241, 245, 249) : Color.FromArgb(15, 23, 42);
        var secondary = darkTheme ? Color.FromArgb(148, 163, 184) : Color.FromArgb(71, 85, 105);

        Text = "CipherDeck · Analýza četnosti";
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(820, 650);
        MinimumSize = new Size(680, 520);
        BackColor = background;
        Font = new Font("Segoe UI", 10F);

        var layout = new TableLayoutPanel
        {
            BackColor = background,
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Padding = new Padding(24),
            RowCount = 5
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 58));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 42));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

        var heading = new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI", 20F, FontStyle.Bold),
            ForeColor = text,
            Text = "Četnost písmen"
        };
        var totalLetters = frequencies.Sum(item => item.Count);
        var summary = new Label
        {
            AutoSize = true,
            ForeColor = secondary,
            Text = $"{totalLetters:N0} písmen · {frequencies.Count:N0} různých znaků · graf zobrazuje 12 nejčastějších"
        };

        var chart = new FrequencyChartPanel { Dock = DockStyle.Fill, Margin = new Padding(0, 8, 0, 12) };
        chart.SetData(frequencies, darkTheme);

        var grid = new DataGridView
        {
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            AutoGenerateColumns = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = input,
            BorderStyle = BorderStyle.None,
            ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None,
            ColumnHeadersHeight = 34,
            Dock = DockStyle.Fill,
            EnableHeadersVisualStyles = false,
            GridColor = panel,
            ReadOnly = true,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect
        };
        grid.ColumnHeadersDefaultCellStyle.BackColor = panel;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = text;
        grid.DefaultCellStyle.BackColor = input;
        grid.DefaultCellStyle.ForeColor = text;
        grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(109, 40, 217);
        grid.DefaultCellStyle.SelectionForeColor = Color.White;
        grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(LetterFrequency.Symbol), HeaderText = "Písmeno" });
        grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(LetterFrequency.Count), HeaderText = "Počet" });
        grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(LetterFrequency.Percentage), HeaderText = "Podíl", DefaultCellStyle = new DataGridViewCellStyle { Format = "0.00' %'" } });
        grid.DataSource = frequencies.ToList();

        var closeButton = new Button
        {
            Anchor = AnchorStyles.Right,
            BackColor = Color.FromArgb(124, 58, 237),
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.White,
            Height = 36,
            Text = "Zavřít",
            UseVisualStyleBackColor = false,
            Width = 110
        };
        closeButton.Click += (_, _) => Close();

        layout.Controls.Add(heading, 0, 0);
        layout.Controls.Add(summary, 0, 1);
        layout.Controls.Add(chart, 0, 2);
        layout.Controls.Add(grid, 0, 3);
        layout.Controls.Add(closeButton, 0, 4);
        Controls.Add(layout);
    }
}
