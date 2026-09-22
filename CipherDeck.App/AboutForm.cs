namespace CipherDeck;

internal sealed class AboutForm : Form
{
    public AboutForm(bool darkTheme)
    {
        var palette = UiTheme.GetPalette(darkTheme);
        var background = palette.Background;
        var panel = palette.Surface;
        var text = palette.Text;
        var secondary = palette.Muted;

        Text = AppText.Get("AboutTitle");
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(680, 560);
        MinimumSize = new Size(580, 460);
        BackColor = background;
        Font = new Font("Segoe UI", 10F);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(26),
            ColumnCount = 1,
            RowCount = 3,
            BackColor = background
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 76));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

        var heading = new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI", 23F, FontStyle.Bold),
            ForeColor = text,
            Text = AppText.Format("AboutHeading", AppInfo.DisplayVersion)
        };
        var tagline = new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI", 10F),
            ForeColor = secondary,
            Location = new Point(3, 45),
            Text = AppText.Get("Tagline")
        };
        var header = new Panel { Dock = DockStyle.Fill };
        header.Controls.Add(heading);
        header.Controls.Add(tagline);

        var help = new RichTextBox
        {
            AccessibleName = AppText.Get("AboutHelpLabel"),
            BackColor = panel,
            BorderStyle = BorderStyle.None,
            Dock = DockStyle.Fill,
            ForeColor = secondary,
            ReadOnly = true,
            Text = AppText.Get("AboutHelp")
        };

        var closeButton = UiStyles.CreateGridButton(AppText.Get("AboutClose"), palette, primary: true);
        closeButton.DialogResult = DialogResult.Cancel;
        closeButton.Click += (_, _) => Close();

        var githubButton = UiStyles.CreateGridButton(AppText.Get("AboutGitHub"), palette);
        githubButton.Click += (_, _) => OpenGitHub();

        var buttons = new TableLayoutPanel
        {
            ColumnCount = 2,
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 3, 0, 3),
            RowCount = 1
        };
        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        buttons.Controls.Add(githubButton, 0, 0);
        buttons.Controls.Add(closeButton, 1, 0);

        layout.Controls.Add(header, 0, 0);
        layout.Controls.Add(help, 0, 1);
        layout.Controls.Add(buttons, 0, 2);
        Controls.Add(layout);
        AcceptButton = closeButton;
        CancelButton = closeButton;
    }

    private void OpenGitHub()
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = "https://github.com/Sodicek/CipherDeck",
                UseShellExecute = true
            });
        }
        catch (Exception exception) when (exception is System.ComponentModel.Win32Exception or InvalidOperationException)
        {
            MessageBox.Show(
                this,
                AppText.Get("AboutGitHubError"),
                "CipherDeck · GitHub",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }

}
