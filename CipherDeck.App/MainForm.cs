using CipherDeck.Core;
using CipherDeck.Core.Analysis;
using CipherDeck.Core.Detection;

namespace CipherDeck;

public partial class MainForm : Form
{
    private readonly System.Windows.Forms.Timer _previewTimer;
    private readonly AppPreferences _preferences;
    private readonly bool _persistPreferences;
    private readonly TransformationSessionService _transformationSession = new();
    private readonly HistoryService _history;
    private readonly TextFileService _textFiles = new();
    private bool _darkTheme;
    private bool _changingLanguage;

    public MainForm() : this(AppPreferences.Load(), new HistoryService(), persistPreferences: true)
    {
    }

    internal MainForm(AppPreferences preferences, HistoryService history, bool persistPreferences = false)
    {
        _preferences = preferences ?? throw new ArgumentNullException(nameof(preferences));
        _history = history ?? throw new ArgumentNullException(nameof(history));
        _persistPreferences = persistPreferences;
        AppLanguage.Apply(_preferences.LanguageCode);
        _darkTheme = _preferences.DarkTheme;
        _previewTimer = new System.Windows.Forms.Timer { Interval = 280 };

        InitializeComponent();
        LoadApplicationIcon();
        cipherSelector.DisplayMember = nameof(ICipher.Name);
        foreach (var cipher in CipherCatalog.All)
            cipherSelector.Items.Add(cipher);

        _previewTimer.Tick += PreviewTimer_Tick;
        FormClosed += (_, _) =>
        {
            _previewTimer.Dispose();
            _transformationSession.Dispose();
        };
        cipherSelector.SelectedIndex = FindSavedCipherIndex();
        encryptMode.Checked = true;
        ApplyLocalization();
        Shown += (_, _) => inputText.Focus();
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
            shiftValue.AccessibleName = cipher.KeyLabel;
            shiftValue.Minimum = cipher.MinimumNumericKey;
            shiftValue.Maximum = cipher.MaximumNumericKey;
            if (!_changingLanguage)
                shiftValue.Value = cipher.DefaultNumericKey;
        }

        if (cipher?.KeyType == CipherKeyType.Text)
        {
            textKeyLabel.Text = cipher.KeyLabel;
            textKeyInput.AccessibleName = cipher.KeyLabel;
            if (!_changingLanguage)
                textKeyInput.Text = cipher.DefaultTextKey;
        }

        statusLabel.Text = cipher is null ? AppText.Get("MainSelectCipher") : AppText.Format("MainReadyCipher", cipher.Name);
        if (cipher is not null &&
            !_changingLanguage &&
            (_preferences.SelectedCipherId != cipher.Id || _preferences.SelectedCipherName != cipher.Name))
        {
            _preferences.SelectedCipherId = cipher.Id;
            _preferences.SelectedCipherName = cipher.Name;
            SavePreferences();
        }
        if (!_changingLanguage)
            SchedulePreview();
    }

    private async void TransformButton_Click(object? sender, EventArgs e)
    {
        transformButton.Enabled = false;
        try
        {
            await PerformTransformAsync(addToHistory: true, showEmptyError: true);
        }
        finally
        {
            if (!IsDisposed)
                transformButton.Enabled = true;
        }
    }

    private async Task<bool> PerformTransformAsync(bool addToHistory, bool showEmptyError)
    {
        _transformationSession.Cancel();
        if (SelectedCipher is not { } cipher)
        {
            if (showEmptyError)
                SetError(AppText.Get("ErrorChooseCipher"));
            return false;
        }

        if (string.IsNullOrEmpty(inputText.Text))
        {
            outputText.Clear();
            if (showEmptyError)
            {
                SetError(AppText.Get("ErrorEnterText"));
                inputText.Focus();
            }
            else
            {
                SetReadyStatus();
            }
            return false;
        }

        var key = GetCurrentKey(cipher);
        var isEncryption = encryptMode.Checked;
        var input = inputText.Text;
        if (addToHistory)
            SetWorking(AppText.Get("StatusWorking"));

        var operation = await _transformationSession.TransformAsync(
            new TransformationRequest(cipher, input, key, isEncryption));

        if (!operation.IsCurrent || IsDisposed)
            return false;

        if (operation.Value.ErrorMessage is { } errorMessage)
        {
            outputText.Clear();
            SetError(errorMessage);
            return false;
        }

        var result = operation.Value.Output!;
        outputText.Text = result;

        if (addToHistory)
        {
            var entry = new HistoryEntry(
                DateTime.Now,
                cipher.Name,
                isEncryption,
                input,
                result,
                key,
                cipher.Id);

            var historyResult = _history.Add(entry);
            if (historyResult == HistoryAddResult.EntryTooLarge)
            {
                SetWarning(AppText.Get("WarningHistoryTooLong"));
                return true;
            }

            UpdateHistoryButton();
            if (historyResult == HistoryAddResult.SaveFailed)
            {
                SetWarning(AppText.Get("WarningHistorySave"));
                return true;
            }
        }

        SetSuccess(addToHistory
            ? AppText.Format(isEncryption ? "StatusEncrypted" : "StatusDecrypted", cipher.Name)
            : AppText.Format("StatusLivePreview", cipher.Name));
        return true;
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
            SetError(AppText.Get("ErrorOutputEmpty"));
            return;
        }

        inputText.Text = outputText.Text;
        outputText.Clear();
        if (encryptMode.Checked)
            decryptMode.Checked = true;
        else
            encryptMode.Checked = true;
        SetSuccess(AppText.Get("StatusSwapped"));
        inputText.Focus();
    }

    private void CopyButton_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(outputText.Text))
        {
            SetError(AppText.Get("ErrorNothingToCopy"));
            return;
        }

        try
        {
            Clipboard.SetText(outputText.Text);
            SetSuccess(AppText.Get("StatusCopied"));
        }
        catch (System.Runtime.InteropServices.ExternalException)
        {
            SetError(AppText.Get("ClipboardBusy"));
        }
    }

    private void ClearButton_Click(object? sender, EventArgs e)
    {
        _previewTimer.Stop();
        _transformationSession.Cancel();
        inputText.Clear();
        outputText.Clear();
        SetSuccess(AppText.Get("StatusCleared"));
        inputText.Focus();
    }

    private void HistoryButton_Click(object? sender, EventArgs e)
    {
        using var historyForm = new HistoryForm(_history.Entries, _darkTheme);
        var result = historyForm.ShowDialog(this);
        if (historyForm.ClearRequested)
        {
            var historySaved = _history.Clear();
            UpdateHistoryButton();
            if (historySaved)
                SetSuccess(AppText.Get("StatusHistoryCleared"));
            else
                SetError(AppText.Get("ErrorHistoryFile"));
            return;
        }

        if (result != DialogResult.OK || historyForm.SelectedEntry is not { } entry)
            return;

        for (var index = 0; index < cipherSelector.Items.Count; index++)
        {
            if (cipherSelector.Items[index] is ICipher cipher && cipher.Id == _history.ResolveCipher(entry)?.Id)
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
        SetSuccess(AppText.Get("StatusHistoryLoaded"));
    }

    private async void ImportButton_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            CheckFileExists = true,
            Filter = AppText.Get("TextFilesFilter"),
            Title = AppText.Get("ImportTitle")
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        importButton.Enabled = false;
        try
        {
            var result = await _textFiles.ReadUtf8Async(dialog.FileName);
            if (IsDisposed)
                return;

            if (result.Status == TextFileReadStatus.TooLarge)
            {
                SetError(AppText.Get("ErrorImportTooLarge"));
                return;
            }

            if (result.Status == TextFileReadStatus.Failed)
            {
                SetError(AppText.Get("ErrorImport"));
                return;
            }

            inputText.Text = result.Text;
            SetSuccess(AppText.Format("StatusImported", Path.GetFileName(dialog.FileName)));
            inputText.Focus();
        }
        finally
        {
            if (!IsDisposed)
                importButton.Enabled = true;
        }
    }

    private void ExportButton_Click(object? sender, EventArgs e)
    {
        var hasOutput = !string.IsNullOrEmpty(outputText.Text);
        var textToExport = hasOutput ? outputText.Text : inputText.Text;
        if (string.IsNullOrEmpty(textToExport))
        {
            SetError(AppText.Get("ErrorNothingToExport"));
            return;
        }

        var operationLabel = hasOutput
            ? AppText.Get(encryptMode.Checked ? "OperationEncrypted" : "OperationDecrypted")
            : AppText.Get("OperationInput");
        var title = AppText.Get(hasOutput
            ? encryptMode.Checked ? "ShareEncryptedTitle" : "ShareDecryptedTitle"
            : "ShareInputTitle");
        var data = new ShareCardData(title, textToExport, SelectedCipher?.Name ?? "CipherDeck", operationLabel);
        using var exportForm = new ExportForm(data, _darkTheme);
        exportForm.ShowDialog(this);
        if (exportForm.CompletionMessage is { } completionMessage)
            SetSuccess(completionMessage);
    }

    private void HelpButton_Click(object? sender, EventArgs e)
    {
        using var aboutForm = new AboutForm(_darkTheme);
        aboutForm.ShowDialog(this);
    }

    private async void AnalysisButton_Click(object? sender, EventArgs e)
    {
        var textToAnalyze = string.IsNullOrEmpty(outputText.Text) ? inputText.Text : outputText.Text;
        SetWorking(AppText.Get("StatusAnalyzing"));
        analysisButton.Enabled = false;
        LatestOperationResult<IReadOnlyList<LetterFrequency>> operation;
        try
        {
            operation = await _transformationSession.AnalyzeAsync(textToAnalyze);
        }
        finally
        {
            if (!IsDisposed)
                analysisButton.Enabled = true;
        }

        if (!operation.IsCurrent || IsDisposed)
            return;

        if (operation.Value.Count == 0)
        {
            SetError(AppText.Get("ErrorAnalysisNeedsLetters"));
            return;
        }

        using var analysisForm = new AnalysisForm(operation.Value, _darkTheme);
        analysisForm.ShowDialog(this);
    }

    private async void ExplainButton_Click(object? sender, EventArgs e)
    {
        explainButton.Enabled = false;
        try
        {
            if (!await PerformTransformAsync(addToHistory: false, showEmptyError: true) ||
                SelectedCipher is not { } cipher)
                return;

            var input = inputText.Text;
            var encrypt = encryptMode.Checked;
            var key = GetCurrentKey(cipher);
            SetWorking(AppText.Get("StatusExplaining"));
            var operation = await _transformationSession.ExplainAsync(cipher, input, encrypt, key);
            if (!operation.IsCurrent || IsDisposed)
                return;

            using var explanationForm = new ExplanationForm(operation.Value, _darkTheme);
            explanationForm.ShowDialog(this);
        }
        finally
        {
            if (!IsDisposed)
                explainButton.Enabled = true;
        }
    }

    private void ChallengeButton_Click(object? sender, EventArgs e)
    {
        using var challengeForm = new ChallengeForm(_darkTheme);
        challengeForm.ShowDialog(this);
    }

    private async void DetectButton_Click(object? sender, EventArgs e)
    {
        var textToDetect = string.IsNullOrEmpty(outputText.Text) ? inputText.Text : outputText.Text;
        SetWorking(AppText.Get("StatusDetecting"));
        detectButton.Enabled = false;
        LatestOperationResult<IReadOnlyList<CipherDetection>> operation;
        try
        {
            operation = await _transformationSession.DetectAsync(textToDetect);
        }
        finally
        {
            if (!IsDisposed)
                detectButton.Enabled = true;
        }

        if (!operation.IsCurrent || IsDisposed)
            return;

        if (operation.Value.Count == 0)
        {
            SetError(AppText.Get("ErrorDetectionNeedsLetters"));
            return;
        }

        using var detectionForm = new DetectionForm(operation.Value, _darkTheme);
        if (detectionForm.ShowDialog(this) != DialogResult.OK || detectionForm.SelectedDetection is not { } detection)
            return;

        outputText.Text = detection.SuggestedPlainText;
        SetSuccess(AppText.Format("StatusDetectionUsed", detection.CipherName, detection.Confidence));
    }

    private void GenerateKeyButton_Click(object? sender, EventArgs e)
    {
        if (SelectedCipher is not { } cipher || CipherKeyGenerator.Generate(cipher) is not { } key)
            return;

        if (key.Number is { } number)
            shiftValue.Value = Math.Clamp(number, (int)shiftValue.Minimum, (int)shiftValue.Maximum);
        if (key.Text is { } text)
            textKeyInput.Text = text;

        SetSuccess(AppText.Format("StatusKeyGenerated", cipher.Name));
    }

    private void ThemeButton_Click(object? sender, EventArgs e)
    {
        _darkTheme = !_darkTheme;
        _preferences.DarkTheme = _darkTheme;
        SavePreferences();
        ApplyTheme();
        SetSuccess(AppText.Get(_darkTheme ? "StatusDarkTheme" : "StatusLightTheme"));
    }

    private void LanguageButton_Click(object? sender, EventArgs e)
    {
        var selectedCipherId = SelectedCipher?.Id;
        var numericKey = (int)shiftValue.Value;
        var textKey = textKeyInput.Text;

        _previewTimer.Stop();
        _transformationSession.Cancel();
        _preferences.LanguageCode = AppLanguage.Normalize(_preferences.LanguageCode) == AppLanguage.Czech
            ? AppLanguage.English
            : AppLanguage.Czech;
        AppLanguage.Apply(_preferences.LanguageCode);
        SavePreferences();

        _changingLanguage = true;
        cipherSelector.BeginUpdate();
        cipherSelector.Items.Clear();
        foreach (var cipher in CipherCatalog.All)
            cipherSelector.Items.Add(cipher);
        cipherSelector.SelectedIndex = Enumerable.Range(0, cipherSelector.Items.Count)
            .FirstOrDefault(index => cipherSelector.Items[index] is ICipher cipher && cipher.Id == selectedCipherId);
        cipherSelector.EndUpdate();
        _changingLanguage = false;

        if (SelectedCipher?.KeyType == CipherKeyType.Number)
            shiftValue.Value = Math.Clamp(numericKey, (int)shiftValue.Minimum, (int)shiftValue.Maximum);
        if (SelectedCipher?.KeyType == CipherKeyType.Text)
            textKeyInput.Text = textKey;

        ApplyLocalization();
        SetSuccess(AppText.Get("StatusLanguageChanged"));
        SchedulePreview();
    }

    private void LivePreview_CheckedChanged(object? sender, EventArgs e)
    {
        if (livePreview.Checked)
            SchedulePreview();
        else
        {
            _previewTimer.Stop();
            _transformationSession.Cancel();
            SetReadyStatus();
        }
    }

    private void PreviewSettingChanged(object? sender, EventArgs e) => SchedulePreview();

    private void InputText_TextChanged(object? sender, EventArgs e)
    {
        UpdateCharacterCount();
        SchedulePreview();
    }

    private void SchedulePreview()
    {
        _previewTimer.Stop();
        _transformationSession.Cancel();
        if (!livePreview.Checked)
        {
            SetReadyStatus();
            return;
        }

        _previewTimer.Start();
    }

    private async void PreviewTimer_Tick(object? sender, EventArgs e)
    {
        _previewTimer.Stop();
        await PerformTransformAsync(addToHistory: false, showEmptyError: false);
    }

    private void MainForm_KeyDown(object? sender, KeyEventArgs e)
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

    private void UpdateCharacterCount() => characterCount.Text = AppText.Format("MainCharacters", inputText.TextLength);

    private void LoadApplicationIcon()
    {
        using var stream = typeof(MainForm).Assembly.GetManifestResourceStream("CipherDeck.AppIcon.ico");
        if (stream is null)
            return;

        using var icon = new Icon(stream);
        Icon = (Icon)icon.Clone();
    }

    private int FindSavedCipherIndex()
    {
        var selectedCipherId = _preferences.SelectedCipherId
            ?? CipherCatalog.FindByName(_preferences.SelectedCipherName)?.Id;

        for (var index = 0; index < cipherSelector.Items.Count; index++)
        {
            if (cipherSelector.Items[index] is ICipher cipher && cipher.Id == selectedCipherId)
                return index;
        }

        return 0;
    }

    private void UpdateHistoryButton() => historyButton.Text = AppText.Format("MainHistory", _history.Entries.Count);

    private void SavePreferences()
    {
        if (_persistPreferences)
            _preferences.Save();
    }

    private void ApplyLocalization()
    {
        Text = AppText.Format("MainWindowTitle", AppInfo.DisplayVersion);
        subtitleLabel.Text = AppText.Get("Tagline");
        challengeButton.Text = AppText.Get("MainChallenges");
        analysisButton.Text = AppText.Get("MainAnalysis");
        helpButton.Text = AppText.Get("MainHelp");
        cipherLabel.Text = AppText.Get("MainCipher");
        cipherSelector.AccessibleName = cipherLabel.Text;
        encryptMode.Text = AppText.Get("MainEncrypt");
        decryptMode.Text = AppText.Get("MainDecrypt");
        inputGroup.Text = AppText.Get("MainInput");
        outputGroup.Text = AppText.Get("MainOutput");
        inputText.AccessibleName = inputGroup.Text.Trim();
        outputText.AccessibleName = outputGroup.Text.Trim();
        importButton.Text = AppText.Get("MainImport");
        exportButton.Text = AppText.Get("MainExport");
        transformButton.Text = AppText.Get("MainTransform");
        swapButton.Text = AppText.Get("MainSwap");
        copyButton.Text = AppText.Get("MainCopy");
        clearButton.Text = AppText.Get("MainClear");
        explainButton.Text = AppText.Get("MainExplain");
        detectButton.Text = AppText.Get("MainDetect");
        livePreview.Text = AppText.Get("MainLivePreview");
        randomNumericKeyButton.AccessibleName = AppText.Get("MainGenerateKey");
        randomTextKeyButton.AccessibleName = AppText.Get("MainGenerateKey");
        themeButton.AccessibleName = AppText.Get("TipTheme");
        languageButton.AccessibleName = AppText.Get("TipLanguage");
        languageButton.Text = AppLanguage.Normalize(_preferences.LanguageCode) == AppLanguage.Czech
            ? AppText.Get("MainLanguageEnglish")
            : AppText.Get("MainLanguageCzech");
        cipherDescription.Text = SelectedCipher?.Description ?? string.Empty;
        if (SelectedCipher is { KeyType: CipherKeyType.Number } numericCipher)
        {
            keyLabel.Text = numericCipher.KeyLabel;
            shiftValue.AccessibleName = numericCipher.KeyLabel;
        }
        if (SelectedCipher is { KeyType: CipherKeyType.Text } textCipher)
        {
            textKeyLabel.Text = textCipher.KeyLabel;
            textKeyInput.AccessibleName = textCipher.KeyLabel;
        }
        UpdateHistoryButton();
        UpdateCharacterCount();
        ApplyToolTips();
        ApplyTheme();
    }

    private void ApplyToolTips()
    {
        toolTip.SetToolTip(challengeButton, AppText.Get("TipChallenges"));
        toolTip.SetToolTip(historyButton, AppText.Get("TipHistory"));
        toolTip.SetToolTip(analysisButton, AppText.Get("TipAnalysis"));
        toolTip.SetToolTip(helpButton, AppText.Get("TipHelp"));
        toolTip.SetToolTip(themeButton, AppText.Get("TipTheme"));
        toolTip.SetToolTip(languageButton, AppText.Get("TipLanguage"));
        toolTip.SetToolTip(transformButton, AppText.Get("TipTransform"));
        toolTip.SetToolTip(swapButton, AppText.Get("TipSwap"));
        toolTip.SetToolTip(copyButton, AppText.Get("TipCopy"));
        toolTip.SetToolTip(clearButton, AppText.Get("TipClear"));
        toolTip.SetToolTip(importButton, AppText.Get("TipImport"));
        toolTip.SetToolTip(exportButton, AppText.Get("TipExport"));
        toolTip.SetToolTip(explainButton, AppText.Get("TipExplain"));
        toolTip.SetToolTip(detectButton, AppText.Get("TipDetect"));
        toolTip.SetToolTip(livePreview, AppText.Get("TipLivePreview"));
    }

    private void ApplyTheme()
    {
        var palette = UiTheme.GetPalette(_darkTheme);

        BackColor = palette.Background;
        mainLayout.BackColor = palette.Background;
        optionsPanel.BackColor = palette.Surface;
        optionsPanel.BorderColor = palette.Border;
        inputGroup.BackColor = palette.Content;
        inputGroup.BorderColor = palette.Border;
        inputGroup.ForeColor = palette.Text;
        outputGroup.BackColor = palette.Content;
        outputGroup.BorderColor = palette.Border;
        outputGroup.ForeColor = palette.Text;
        inputText.BackColor = palette.Content;
        inputText.ForeColor = palette.Text;
        outputText.BackColor = palette.Content;
        outputText.ForeColor = palette.OutputText;
        cipherSelector.BackColor = palette.Content;
        cipherSelector.ForeColor = palette.Text;
        shiftValue.BackColor = palette.Content;
        shiftValue.ForeColor = palette.Text;
        textKeyInput.BackColor = palette.Content;
        textKeyInput.ForeColor = palette.Text;

        titleLabel.ForeColor = palette.Text;
        subtitleLabel.ForeColor = palette.Muted;
        cipherLabel.ForeColor = palette.Text;
        cipherDescription.ForeColor = palette.Muted;
        encryptMode.ForeColor = palette.Text;
        decryptMode.ForeColor = palette.Text;
        keyLabel.ForeColor = palette.Text;
        textKeyLabel.ForeColor = palette.Text;
        characterCount.ForeColor = palette.Muted;
        livePreview.ForeColor = palette.Text;

        foreach (var button in new[] { swapButton, copyButton, clearButton, importButton, exportButton, explainButton, detectButton, randomNumericKeyButton, randomTextKeyButton, challengeButton, historyButton, analysisButton, helpButton, themeButton, languageButton })
            UiStyles.ApplyButtonTheme(button, palette);

        UiStyles.ApplyButtonTheme(transformButton, palette, primary: true);
        themeButton.Text = AppText.Get(_darkTheme ? "MainThemeLight" : "MainThemeDark");
        optionsPanel.Invalidate();
        inputGroup.Invalidate();
        outputGroup.Invalidate();
    }

    private void SetSuccess(string message)
    {
        statusLabel.ForeColor = UiTheme.GetPalette(_darkTheme).Success;
        statusLabel.Text = message;
    }

    private void SetError(string message)
    {
        statusLabel.ForeColor = UiTheme.GetPalette(_darkTheme).Error;
        statusLabel.Text = message;
    }

    private void SetWarning(string message)
    {
        statusLabel.ForeColor = UiTheme.GetPalette(_darkTheme).Accent;
        statusLabel.Text = message;
    }

    private void SetWorking(string message)
    {
        statusLabel.ForeColor = UiTheme.GetPalette(_darkTheme).Muted;
        statusLabel.Text = message;
    }

    private void SetReadyStatus()
    {
        statusLabel.ForeColor = UiTheme.GetPalette(_darkTheme).Success;
        statusLabel.Text = SelectedCipher is { } cipher
            ? AppText.Format("MainReadyCipher", cipher.Name)
            : AppText.Get("MainSelectCipher");
    }

}
