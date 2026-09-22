using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;

namespace CipherDeck;

internal sealed class ExportForm : Form
{
    private readonly ShareCardData _baseData;
    private readonly bool _darkTheme;
    private readonly TextBox _titleInput;
    private readonly ComboBox _styleSelector;
    private readonly PictureBox _preview;
    private readonly Label _status;
    private Bitmap? _cardImage;

    public ExportForm(ShareCardData data, bool darkTheme)
    {
        _baseData = data;
        _darkTheme = darkTheme;
        var palette = UiTheme.GetPalette(darkTheme);

        Text = AppText.Get("ExportTitle");
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(900, 720);
        MinimumSize = new Size(760, 620);
        BackColor = palette.Background;
        Font = new Font("Segoe UI", 10F);

        var layout = new TableLayoutPanel
        {
            BackColor = palette.Background,
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Padding = new Padding(26),
            RowCount = 6
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 52));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60));

        var heading = new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI", 20F, FontStyle.Bold),
            ForeColor = palette.Text,
            Text = AppText.Get("ExportHeading")
        };
        var summary = new Label
        {
            AutoSize = true,
            ForeColor = palette.Muted,
            Text = AppText.Get("ExportSummary")
        };

        var settings = new TableLayoutPanel
        {
            ColumnCount = 4,
            Dock = DockStyle.Fill,
            Margin = new Padding(0),
            RowCount = 1
        };
        settings.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 64));
        settings.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        settings.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 64));
        settings.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));

        var titleLabel = CreateSettingsLabel(AppText.Get("ExportName"), palette.Text);
        _titleInput = new TextBox
        {
            AccessibleName = titleLabel.Text,
            Anchor = AnchorStyles.Left | AnchorStyles.Right,
            BackColor = palette.Content,
            BorderStyle = BorderStyle.FixedSingle,
            Font = new Font("Segoe UI", 10.5F),
            ForeColor = palette.Text,
            Margin = new Padding(0, 7, 18, 7),
            MaxLength = 48,
            Text = data.Title
        };
        var styleLabel = CreateSettingsLabel(AppText.Get("ExportStyle"), palette.Text);
        _styleSelector = new ComboBox
        {
            AccessibleName = styleLabel.Text,
            Anchor = AnchorStyles.Left | AnchorStyles.Right,
            BackColor = palette.Content,
            DropDownStyle = ComboBoxStyle.DropDownList,
            FlatStyle = FlatStyle.Flat,
            ForeColor = palette.Text,
            Margin = new Padding(0, 7, 0, 7)
        };
        _styleSelector.Items.AddRange([
            new StyleOption(AppText.Get("ExportViolet"), ShareCardStyle.Violet),
            new StyleOption(AppText.Get("ExportMidnight"), ShareCardStyle.Midnight),
            new StyleOption(AppText.Get("ExportPaper"), ShareCardStyle.Paper)
        ]);
        settings.Controls.Add(titleLabel, 0, 0);
        settings.Controls.Add(_titleInput, 1, 0);
        settings.Controls.Add(styleLabel, 2, 0);
        settings.Controls.Add(_styleSelector, 3, 0);

        var previewHost = new RoundedTableLayoutPanel
        {
            BackColor = palette.Surface,
            BorderColor = palette.Border,
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 8, 0, 10),
            Padding = new Padding(12),
            RowCount = 1
        };
        previewHost.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        previewHost.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _preview = new PictureBox
        {
            AccessibleName = AppText.Get("ExportPreview"),
            BackColor = palette.Content,
            Dock = DockStyle.Fill,
            SizeMode = PictureBoxSizeMode.Zoom,
            TabStop = false
        };
        previewHost.Controls.Add(_preview, 0, 0);

        _status = new Label
        {
            Anchor = AnchorStyles.Left,
            AutoSize = true,
            ForeColor = palette.Muted,
            Text = $"PNG {ShareCardRenderer.CardWidth} × {ShareCardRenderer.CardHeight} px"
        };

        var buttons = new TableLayoutPanel
        {
            ColumnCount = 4,
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 8, 0, 2),
            RowCount = 1
        };
        for (var column = 0; column < 4; column++)
            buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));

        var closeButton = UiStyles.CreateGridButton(AppText.Get("Close"), palette);
        var saveTextButton = UiStyles.CreateGridButton(AppText.Get("ExportSaveText"), palette);
        var copyImageButton = UiStyles.CreateGridButton(AppText.Get("ExportCopyImage"), palette);
        var saveImageButton = UiStyles.CreateGridButton(AppText.Get("ExportSaveImage"), palette, primary: true);
        closeButton.DialogResult = DialogResult.Cancel;
        closeButton.Click += (_, _) => Close();
        saveTextButton.Click += (_, _) => SaveText();
        copyImageButton.Click += (_, _) => CopyImage();
        saveImageButton.Click += (_, _) => SaveImage();
        buttons.Controls.Add(closeButton, 0, 0);
        buttons.Controls.Add(saveTextButton, 1, 0);
        buttons.Controls.Add(copyImageButton, 2, 0);
        buttons.Controls.Add(saveImageButton, 3, 0);

        layout.Controls.Add(heading, 0, 0);
        layout.Controls.Add(summary, 0, 1);
        layout.Controls.Add(settings, 0, 2);
        layout.Controls.Add(previewHost, 0, 3);
        layout.Controls.Add(_status, 0, 4);
        layout.Controls.Add(buttons, 0, 5);
        Controls.Add(layout);

        AcceptButton = saveImageButton;
        CancelButton = closeButton;
        _titleInput.TextChanged += (_, _) => RefreshPreview();
        _styleSelector.SelectedIndexChanged += (_, _) => RefreshPreview();
        _styleSelector.SelectedIndex = 0;
        FormClosed += (_, _) => DisposePreview();
    }

    public string? CompletionMessage { get; private set; }

    private ShareCardStyle SelectedStyle => _styleSelector.SelectedItem is StyleOption option
        ? option.Value
        : ShareCardStyle.Violet;

    private void RefreshPreview()
    {
        var previous = _cardImage;
        _cardImage = ShareCardRenderer.Render(_baseData with { Title = _titleInput.Text }, SelectedStyle);
        _preview.Image = _cardImage;
        previous?.Dispose();
    }

    private void SaveText()
    {
        using var dialog = new SaveFileDialog
        {
            AddExtension = true,
            DefaultExt = "txt",
            FileName = "cipherdeck-output.txt",
            Filter = AppText.Get("TextFilesFilter"),
            Title = AppText.Get("ExportSaveTextTitle")
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        try
        {
            File.WriteAllText(dialog.FileName, _baseData.Text, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            SetSuccess(AppText.Format("ExportTextSaved", Path.GetFileName(dialog.FileName)));
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            SetError(AppText.Get("ExportTextError"));
        }
    }

    private void SaveImage()
    {
        if (_cardImage is null)
            return;

        using var dialog = new SaveFileDialog
        {
            AddExtension = true,
            DefaultExt = "png",
            FileName = "cipherdeck-card.png",
            Filter = AppText.Get("ExportPngFilter"),
            Title = AppText.Get("ExportSaveImageTitle")
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        try
        {
            _cardImage.Save(dialog.FileName, ImageFormat.Png);
            SetSuccess(AppText.Format("ExportImageSaved", Path.GetFileName(dialog.FileName)));
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ExternalException)
        {
            SetError(AppText.Get("ExportImageError"));
        }
    }

    private void CopyImage()
    {
        if (_cardImage is null)
            return;

        try
        {
            Clipboard.SetImage(_cardImage);
            SetSuccess(AppText.Get("ExportImageCopied"));
        }
        catch (ExternalException)
        {
            SetError(AppText.Get("ClipboardBusy"));
        }
    }

    private void SetSuccess(string message)
    {
        CompletionMessage = message;
        _status.ForeColor = UiTheme.GetPalette(_darkTheme).Success;
        _status.Text = message;
    }

    private void SetError(string message)
    {
        _status.ForeColor = UiTheme.GetPalette(_darkTheme).Error;
        _status.Text = message;
    }

    private void DisposePreview()
    {
        _preview.Image = null;
        _cardImage?.Dispose();
        _cardImage = null;
    }

    private static Label CreateSettingsLabel(string text, Color color) => new()
    {
        Anchor = AnchorStyles.Left,
        AutoSize = true,
        Font = new Font("Segoe UI", 9F, FontStyle.Bold),
        ForeColor = color,
        Text = text
    };

    private sealed record StyleOption(string Name, ShareCardStyle Value)
    {
        public override string ToString() => Name;
    }
}
