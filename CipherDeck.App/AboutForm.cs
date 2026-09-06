namespace CipherDeck;

internal sealed class AboutForm : Form
{
    public AboutForm(bool darkTheme)
    {
        var background = darkTheme ? Color.FromArgb(15, 23, 42) : Color.FromArgb(241, 245, 249);
        var panel = darkTheme ? Color.FromArgb(30, 41, 59) : Color.White;
        var text = darkTheme ? Color.FromArgb(241, 245, 249) : Color.FromArgb(15, 23, 42);
        var secondary = darkTheme ? Color.FromArgb(203, 213, 225) : Color.FromArgb(51, 65, 85);

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
            Text = "CipherDeck  v0.4"
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

        var closeButton = new Button
        {
            Anchor = AnchorStyles.Right,
            BackColor = Color.FromArgb(124, 58, 237),
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.White,
            Height = 36,
            Text = "Rozumím",
            UseVisualStyleBackColor = false,
            Width = 120
        };
        closeButton.Click += (_, _) => Close();

        layout.Controls.Add(header, 0, 0);
        layout.Controls.Add(help, 0, 1);
        layout.Controls.Add(closeButton, 0, 2);
        Controls.Add(layout);
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
        Ctrl+S exportuje výsledek nebo vstupní text.

        SOUBORY A HISTORIE

        Textové soubory se načítají a ukládají v UTF-8. Historie obsahuje nejvýše
        30 posledních ručně provedených operací a zůstane dostupná i po restartu.
        Kdykoliv ji můžeš trvale vymazat v okně Historie.

        BEZPEČNOSTNÍ UPOZORNĚNÍ

        Klasické šifry nejsou bezpečné pro ochranu hesel ani citlivých dat.
        CipherDeck je vzdělávací a zábavní aplikace.
        """;
}
