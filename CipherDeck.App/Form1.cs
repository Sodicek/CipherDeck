using CipherDeck.Core;
using CipherDeck.Core.Analysis;
using System.Text;

namespace CipherDeck;

public partial class Form1 : Form
{
    private readonly List<HistoryEntry> _history = [];
    private readonly System.Windows.Forms.Timer _previewTimer;
    private readonly AppPreferences _preferences;
    private bool _darkTheme;

    public Form1()
    {
        _preferences = AppPreferences.Load();
        _darkTheme = _preferences.DarkTheme;
        _previewTimer = new System.Windows.Forms.Timer { Interval = 280 };

        InitializeComponent();
        LoadApplicationIcon();
        cipherSelector.DisplayMember = nameof(ICipher.Name);
        foreach (var cipher in CipherCatalog.All)
            cipherSelector.Items.Add(cipher);

        _history.AddRange(HistoryStore.Load().Take(30));
        _previewTimer.Tick += PreviewTimer_Tick;
        FormClosed += (_, _) => _previewTimer.Dispose();
        cipherSelector.SelectedIndex = FindSavedCipherIndex();
        encryptMode.Checked = true;
        ApplyTheme();
        UpdateCharacterCount();
        UpdateHistoryButton();
    }

    private ICipher? SelectedCipher => cipherSelector.SelectedItem as ICipher;

    private void CipherSelector_SelectedIndexChanged(object? sender, EventArgs e)
    {
        var cipher = SelectedCipher;
        cipherDescription.Text = cipher?.Description ?? string.Empty;
        keyPanel.Visible = cipher?.KeyType == CipherKeyType.Number;
        textKeyPanel.Visible = cipher?.KeyType == CipherKeyType.Text;

        if (cipher?.KeyType == CipherKeyType.Number)
        {
            keyLabel.Text = cipher.KeyLabel;
            shiftValue.Minimum = cipher.MinimumNumericKey;
            shiftValue.Maximum = cipher.MaximumNumericKey;
            shiftValue.Value = cipher.DefaultNumericKey;
        }

        if (cipher?.KeyType == CipherKeyType.Text)
        {
            textKeyLabel.Text = cipher.KeyLabel;
            textKeyInput.Text = cipher.DefaultTextKey;
        }

        statusLabel.Text = cipher is null ? "Vyber šifru." : $"Připraveno · {cipher.Name}";
        if (cipher is not null && _preferences.SelectedCipherName != cipher.Name)
        {
            _preferences.SelectedCipherName = cipher.Name;
            _preferences.Save();
        }
        SchedulePreview();
    }

    private void TransformButton_Click(object? sender, EventArgs e) => PerformTransform(addToHistory: true, showEmptyError: true);

    private bool PerformTransform(bool addToHistory, bool showEmptyError)
    {
        if (SelectedCipher is not { } cipher)
        {
            if (showEmptyError)
                SetError("Nejdřív vyber šifru.");
            return false;
        }

        if (string.IsNullOrEmpty(inputText.Text))
        {
            outputText.Clear();
            if (showEmptyError)
            {
                SetError("Napiš nebo vlož text, který chceš zpracovat.");
                inputText.Focus();
            }
            return false;
        }

        try
        {
            var key = GetCurrentKey(cipher);
            var isEncryption = encryptMode.Checked;
            var result = isEncryption
                ? cipher.Encrypt(inputText.Text, key)
                : cipher.Decrypt(inputText.Text, key);
            outputText.Text = result;

            if (addToHistory)
            {
                _history.Insert(0, new HistoryEntry(
                    DateTime.Now,
                    cipher.Name,
                    isEncryption,
                    inputText.Text,
                    result,
                    key));

                if (_history.Count > 30)
                    _history.RemoveAt(_history.Count - 1);
                HistoryStore.Save(_history);
                UpdateHistoryButton();
            }

            SetSuccess(addToHistory
                ? $"{(isEncryption ? "Zašifrováno" : "Odšifrováno")} pomocí: {cipher.Name}"
                : $"Živý náhled · {cipher.Name}");
            return true;
        }
        catch (ArgumentException exception)
        {
            outputText.Clear();
            SetError(exception.Message);
            return false;
        }
    }

    private CipherKey? GetCurrentKey(ICipher cipher) => cipher.KeyType switch
    {
        CipherKeyType.Number => new CipherKey(Number: (int)shiftValue.Value),
        CipherKeyType.Text => new CipherKey(Text: textKeyInput.Text),
        _ => null
    };

    private void SwapButton_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(outputText.Text))
        {
            SetError("Výstup je zatím prázdný.");
            return;
        }

        inputText.Text = outputText.Text;
        outputText.Clear();
        if (encryptMode.Checked)
            decryptMode.Checked = true;
        else
            encryptMode.Checked = true;
        SetSuccess("Výstup byl přesunut zpět na vstup.");
        inputText.Focus();
    }

    private void CopyButton_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(outputText.Text))
        {
            SetError("Není co zkopírovat.");
            return;
        }

        Clipboard.SetText(outputText.Text);
        SetSuccess("Výsledek je zkopírovaný ve schránce.");
    }

    private void ClearButton_Click(object? sender, EventArgs e)
    {
        _previewTimer.Stop();
        inputText.Clear();
        outputText.Clear();
        SetSuccess("Textová pole byla vymazána.");
        inputText.Focus();
    }

    private void HistoryButton_Click(object? sender, EventArgs e)
    {
        using var historyForm = new HistoryForm(_history, _darkTheme);
        var result = historyForm.ShowDialog(this);
        if (historyForm.ClearRequested)
        {
            _history.Clear();
            HistoryStore.Save(_history);
            UpdateHistoryButton();
            SetSuccess("Historie byla vymazána.");
            return;
        }

        if (result != DialogResult.OK || historyForm.SelectedEntry is not { } entry)
            return;

        for (var index = 0; index < cipherSelector.Items.Count; index++)
        {
            if (cipherSelector.Items[index] is ICipher cipher && cipher.Name == entry.CipherName)
            {
                cipherSelector.SelectedIndex = index;
                break;
            }
        }

        encryptMode.Checked = entry.IsEncryption;
        decryptMode.Checked = !entry.IsEncryption;
        if (entry.Key?.Number is { } number)
            shiftValue.Value = Math.Clamp(number, (int)shiftValue.Minimum, (int)shiftValue.Maximum);
        if (entry.Key?.Text is { } text)
            textKeyInput.Text = text;
        inputText.Text = entry.Input;
        outputText.Text = entry.Output;
        SetSuccess("Operace byla načtena z historie.");
    }

    private void ImportButton_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            CheckFileExists = true,
            Filter = "Textové soubory (*.txt)|*.txt|Všechny soubory (*.*)|*.*",
            Title = "Importovat text do CipherDecku"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        try
        {
            if (new FileInfo(dialog.FileName).Length > 5 * 1024 * 1024)
            {
                SetError("Soubor je příliš velký. Limit importu je 5 MB.");
                return;
            }

            inputText.Text = File.ReadAllText(dialog.FileName, Encoding.UTF8);
            SetSuccess($"Importováno: {Path.GetFileName(dialog.FileName)}");
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            SetError("Soubor se nepodařilo načíst.");
        }
    }

    private void ExportButton_Click(object? sender, EventArgs e)
    {
        var textToExport = string.IsNullOrEmpty(outputText.Text) ? inputText.Text : outputText.Text;
        if (string.IsNullOrEmpty(textToExport))
        {
            SetError("Není co exportovat.");
            return;
        }

        using var dialog = new SaveFileDialog
        {
            AddExtension = true,
            DefaultExt = "txt",
            FileName = "cipherdeck-output.txt",
            Filter = "Textové soubory (*.txt)|*.txt|Všechny soubory (*.*)|*.*",
            Title = "Exportovat výsledek z CipherDecku"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        try
        {
            File.WriteAllText(dialog.FileName, textToExport, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            SetSuccess($"Exportováno: {Path.GetFileName(dialog.FileName)}");
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            SetError("Soubor se nepodařilo uložit.");
        }
    }

    private void HelpButton_Click(object? sender, EventArgs e)
    {
        using var aboutForm = new AboutForm(_darkTheme);
        aboutForm.ShowDialog(this);
    }

    private void AnalysisButton_Click(object? sender, EventArgs e)
    {
        var textToAnalyze = string.IsNullOrEmpty(outputText.Text) ? inputText.Text : outputText.Text;
        if (FrequencyAnalyzer.AnalyzeLetters(textToAnalyze).Count == 0)
        {
            SetError("Pro analýzu je potřeba text obsahující alespoň jedno písmeno.");
            return;
        }

        using var analysisForm = new AnalysisForm(textToAnalyze, _darkTheme);
        analysisForm.ShowDialog(this);
    }

    private void ThemeButton_Click(object? sender, EventArgs e)
    {
        _darkTheme = !_darkTheme;
        _preferences.DarkTheme = _darkTheme;
        _preferences.Save();
        ApplyTheme();
        SetSuccess(_darkTheme ? "Zapnutý tmavý motiv." : "Zapnutý světlý motiv.");
    }

    private void LivePreview_CheckedChanged(object? sender, EventArgs e)
    {
        if (livePreview.Checked)
            SchedulePreview();
        else
            _previewTimer.Stop();
    }

    private void PreviewSettingChanged(object? sender, EventArgs e) => SchedulePreview();

    private void InputText_TextChanged(object? sender, EventArgs e)
    {
        UpdateCharacterCount();
        SchedulePreview();
    }

    private void SchedulePreview()
    {
        if (!livePreview.Checked)
            return;

        _previewTimer.Stop();
        _previewTimer.Start();
    }

    private void PreviewTimer_Tick(object? sender, EventArgs e)
    {
        _previewTimer.Stop();
        PerformTransform(addToHistory: false, showEmptyError: false);
    }

    private void Form1_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Control && e.KeyCode == Keys.Enter)
        {
            transformButton.PerformClick();
            e.SuppressKeyPress = true;
        }
        else if (e.Control && e.KeyCode == Keys.O)
        {
            importButton.PerformClick();
            e.SuppressKeyPress = true;
        }
        else if (e.Control && e.KeyCode == Keys.S)
        {
            exportButton.PerformClick();
            e.SuppressKeyPress = true;
        }
    }

    private void UpdateCharacterCount() => characterCount.Text = $"{inputText.TextLength:N0} znaků";

    private void LoadApplicationIcon()
    {
        using var stream = typeof(Form1).Assembly.GetManifestResourceStream("CipherDeck.AppIcon.ico");
        if (stream is null)
            return;

        using var icon = new Icon(stream);
        Icon = (Icon)icon.Clone();
    }

    private int FindSavedCipherIndex()
    {
        for (var index = 0; index < cipherSelector.Items.Count; index++)
        {
            if (cipherSelector.Items[index] is ICipher cipher && cipher.Name == _preferences.SelectedCipherName)
                return index;
        }

        return 0;
    }

    private void UpdateHistoryButton() => historyButton.Text = $"Historie ({_history.Count})";

    private void ApplyTheme()
    {
        var background = _darkTheme ? Color.FromArgb(15, 23, 42) : Color.FromArgb(241, 245, 249);
        var panel = _darkTheme ? Color.FromArgb(30, 41, 59) : Color.White;
        var input = _darkTheme ? Color.FromArgb(17, 24, 39) : Color.FromArgb(248, 250, 252);
        var text = _darkTheme ? Color.FromArgb(241, 245, 249) : Color.FromArgb(15, 23, 42);
        var secondary = _darkTheme ? Color.FromArgb(148, 163, 184) : Color.FromArgb(71, 85, 105);

        BackColor = background;
        mainLayout.BackColor = background;
        optionsPanel.BackColor = panel;
        inputGroup.ForeColor = text;
        outputGroup.ForeColor = text;
        inputText.BackColor = input;
        inputText.ForeColor = text;
        outputText.BackColor = input;
        outputText.ForeColor = _darkTheme ? Color.FromArgb(196, 181, 253) : Color.FromArgb(91, 33, 182);
        cipherSelector.BackColor = input;
        cipherSelector.ForeColor = text;
        shiftValue.BackColor = input;
        shiftValue.ForeColor = text;
        textKeyInput.BackColor = input;
        textKeyInput.ForeColor = text;

        titleLabel.ForeColor = text;
        subtitleLabel.ForeColor = secondary;
        cipherLabel.ForeColor = text;
        cipherDescription.ForeColor = secondary;
        encryptMode.ForeColor = text;
        decryptMode.ForeColor = text;
        keyLabel.ForeColor = text;
        textKeyLabel.ForeColor = text;
        characterCount.ForeColor = secondary;
        livePreview.ForeColor = text;

        foreach (var button in new[] { swapButton, copyButton, clearButton, importButton, exportButton, historyButton, analysisButton, helpButton, themeButton })
        {
            button.BackColor = panel;
            button.ForeColor = text;
        }

        transformButton.BackColor = Color.FromArgb(124, 58, 237);
        transformButton.ForeColor = Color.White;
        themeButton.Text = _darkTheme ? "☀  Světlý" : "☾  Tmavý";
    }

    private void SetSuccess(string message)
    {
        statusLabel.ForeColor = _darkTheme ? Color.FromArgb(134, 239, 172) : Color.FromArgb(22, 101, 52);
        statusLabel.Text = message;
    }

    private void SetError(string message)
    {
        statusLabel.ForeColor = _darkTheme ? Color.FromArgb(253, 164, 175) : Color.FromArgb(190, 18, 60);
        statusLabel.Text = message;
    }
}
