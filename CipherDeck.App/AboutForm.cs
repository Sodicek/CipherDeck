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

        Text = "O aplikaci CipherDeck";
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
            Text = "CipherDeck  v0.9.1"
        };
        var tagline = new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI", 10F),
            ForeColor = secondary,
            Location = new Point(3, 45),
            Text = "Classic ciphers. Modern interface."
        };
        var header = new Panel { Dock = DockStyle.Fill };
        header.Controls.Add(heading);
        header.Controls.Add(tagline);

        var help = new RichTextBox
        {
            BackColor = panel,
            BorderStyle = BorderStyle.None,
            Dock = DockStyle.Fill,
            ForeColor = secondary,
            ReadOnly = true,
            Text = HelpText
        };

        var closeButton = UiStyles.CreateGridButton("Rozumím", palette, primary: true);
        closeButton.DialogResult = DialogResult.Cancel;
        closeButton.Click += (_, _) => Close();

        var githubButton = UiStyles.CreateGridButton("Otevřít GitHub", palette);
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
                "Odkaz se nepodařilo otevřít. Repo najdeš na github.com/Sodicek/CipherDeck.",
                "CipherDeck · GitHub",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }

    private const string HelpText = """
        JAK APLIKACI POUŽÍVAT

        1. Vyber šifru a režim Zašifrovat nebo Odšifrovat.
        2. Pokud je potřeba, nastav číselný nebo textový klíč.
        3. Napiš text do levého pole. Živý náhled zobrazí výsledek automaticky.
        4. Tlačítkem PROVÉST uložíš operaci také do historie.

        PODPOROVANÉ ŠIFRY

        • Pozpátku — obrátí pořadí znaků.
        • Caesarova šifra — posouvá latinská písmena o zadaný počet míst.
        • Atbash — zrcadlí latinskou abecedu.
        • Vigenèrova šifra — používá opakující se textový klíč A–Z.
        • Rail Fence — zapisuje text cikcak do několika řádků.
        • Přeskakování — čte text po sloupcích se zvoleným krokem.

        KLÁVESOVÁ ZKRATKA

        Ctrl+Enter provede operaci a přidá ji do historie.
        Ctrl+O importuje textový soubor.
        Ctrl+S otevře exportní centrum pro TXT a obrázkové kartičky.

        ANALÝZA ČETNOSTI

        Tlačítko Analýza zobrazí graf a tabulku četnosti písmen. Pokud existuje
        výstup, analyzuje se výstupní text; jinak se použije vstup. To se hodí
        jako první vodítko při luštění jednoduchých substitučních šifer.

        LEARN MODE

        Tlačítko Vysvětlit rozloží aktuální operaci do názorných kroků. Šipkami
        můžeš projít princip algoritmu, mezivýsledky i finální text. Kruhové
        tlačítko vedle klíče vygeneruje vhodný náhodný klíč.

        VÝZVY A ODHAD ŠIFRY

        Ve Výzvách můžeš luštit náhodné zprávy ve třech obtížnostech. Odhad šifry
        vyzkouší obrácení textu, Atbash a všech 25 Caesarových posunů. Výsledek je
        pouze jazyková heuristika, proto zobrazené procento není zárukou správnosti.

        EXPORTNÍ CENTRUM

        Výsledek můžeš dál uložit jako obyčejný TXT nebo jako sdílitelnou PNG
        kartičku v rozměru 1200 × 630. Před exportem lze upravit nadpis a vybrat
        fialový, půlnoční nebo světlý vzhled. Obrázek jde také rovnou zkopírovat.

        SOUBORY A HISTORIE

        Textové soubory se načítají a ukládají v UTF-8. Historie obsahuje nejvýše
        30 posledních ručně provedených operací a zůstane dostupná i po restartu.
        Kdykoliv ji můžeš trvale vymazat v okně Historie.

        BEZPEČNOSTNÍ UPOZORNĚNÍ

        Klasické šifry nejsou bezpečné pro ochranu hesel ani citlivých dat.
        CipherDeck je vzdělávací a zábavní aplikace.
        """;
}
