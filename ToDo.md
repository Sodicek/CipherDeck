# CipherDeck

> Classic ciphers. Modern interface.

Moderní desktopová aplikace pro zkoušení, vysvětlování a porovnávání klasických šifer.

## Vize

CipherDeck nemá být jen formulář se dvěma tlačítky. Každá šifra bude mít vlastní přehlednou kartu, nastavení, krátké vysvětlení a okamžitý náhled výsledku. Aplikace bude vhodná pro výuku, hraní se šiframi i řešení jednoduchých šifrovacích úloh.

> [!IMPORTANT]
> Klasické šifry v aplikaci nejsou bezpečné pro ochranu skutečných hesel nebo citlivých dat. Tato informace musí být viditelná v aplikaci i v README.

## 1. Funkční MVP

- [x] Přejmenovat aplikaci, řešení a hlavní okno na `CipherDeck`.
- [x] Nahradit checkboxy přehledným výběrem jedné šifry.
- [x] Umožnit přepínání režimu **Zašifrovat / Odšifrovat**.
- [x] Přidat vstupní a výstupní textové pole s podporou delšího textu.
- [x] Přidat tlačítka **Kopírovat**, **Vymazat** a **Prohodit vstup s výstupem**.
- [x] Zobrazovat srozumitelné chyby při prázdném vstupu nebo neplatném klíči.
- [x] Odstranit nefunkční stahování návodu a zahrnout nápovědu přímo do aplikace.

### První podporované šifry

- [x] Text pozpátku.
- [x] Caesarova šifra s nastavitelným posunem.
- [x] Atbashova šifra.
- [x] Vigenèrova šifra s textovým klíčem.
- [x] Rail Fence neboli „hradby“.
- [x] Přeskakování o zvolený počet znaků.

### Pravidla pro algoritmy

- [x] Každá šifra musí umět šifrování i odšifrování.
- [x] Algoritmy oddělit od uživatelského rozhraní.
- [x] Sjednotit je společným rozhraním, například `ICipher`.
- [x] Jasně určit chování mezer, interpunkce, čísel a diakritiky.
- [x] Zachovat velikost písmen, pokud to daná šifra umožňuje.
- [x] Ke každé šifře přidat název, popis a požadované parametry.

## 2. Modernizace projektu

- [x] Převést projekt z .NET Framework 4.7.2 na aktuální podporovanou verzi .NET.
- [x] Zapnout nullable reference types a analyzátory kódu.
- [ ] Používat konzistentní anglické názvy tříd, metod a souborů.
- [x] Rozdělit projekt na vrstvy `UI`, `Core` a `Tests`.
- [x] Odstranit nepoužívané importy, proměnné a prázdné metody.
- [x] Přidat logo a ikonu aplikace.
- [x] Nastavit správné údaje o produktu a verzi.
- [x] Ukládat poslední zvolenou šifru do uživatelského nastavení.
- [x] Ukládat zvolený motiv do uživatelského nastavení.

## 3. Vzhled a použitelnost

- [x] Navrhnout jednoduché moderní rozhraní bez výchozího vzhledu WinForms.
- [x] Vytvořit světlý a tmavý motiv.
- [x] Použít responzivní rozložení, které zvládne změnu velikosti okna.
- [x] Zobrazovat nastavení dynamicky podle vybrané šifry.
- [x] Přidat počítadlo znaků a nenápadnou stavovou zprávu po dokončení operace.
- [x] Zajistit ovládání klávesnicí a logické pořadí tabulátoru.
- [x] Doplnit tooltipy a dobře čitelné kontrasty.
- [x] Přidat obrazovku **O aplikaci** s verzí a nápovědou.
- [x] Doplnit do obrazovky **O aplikaci** odkaz na GitHub.

## 4. Funkce pro „top appku“

- [x] Živý náhled výsledku při psaní.
- [x] Trvalá historie posledních 30 operací s možností návratu a vymazání.
- [x] Import a export textových souborů v UTF-8.
- [x] Režim krok za krokem, který vizuálně vysvětlí průběh šifry.
- [x] Náhodný generátor vhodných klíčů.
- [x] Analýza četnosti písmen a jednoduchý sloupcový graf.
- [x] Detekce pravděpodobného typu jednoduché šifry.
- [x] Výzvy k rozluštění s několika úrovněmi obtížnosti.
- [x] Export výsledku jako sdílitelná kartička nebo obrázek.
- [ ] Lokalizace minimálně do češtiny a angličtiny.

## 5. Testování a kvalita

- [x] Založit samostatný projekt s automatickými testy.
- [x] Otestovat každý algoritmus na známých příkladech.
- [x] Ověřit pravidlo `Decrypt(Encrypt(text, key), key) == text`.
- [x] Přidat testy prázdného vstupu, Unicode, diakritiky a velmi dlouhého textu.
- [x] Otestovat neplatné a hraniční hodnoty klíčů.
- [x] Zajistit sestavení bez chyb a varování.
- [x] Nastavit automatické sestavení a testy přes GitHub Actions.
- [ ] Provést ruční kontrolu UI při různém DPI a velikosti okna.

## 6. GitHub prezentace

- [x] Inicializovat Git repozitář a vytvořit smysluplný `.gitignore`.
- [x] Napsat úvodní `README.md`; před vydáním doplnit anglickou variantu a screenshoty.
- [x] Do README vložit logo a seznam funkcí.
- [x] Do README doplnit screenshoty a krátkou ukázku použití.
- [x] Přidat anglickou variantu README (`README.en.md`).
- [x] Přidat instrukce pro spuštění, sestavení a přispívání.
- [x] Zvolit licenci, například MIT.
- [x] Přidat šablony pro bug report a návrh nové funkce.
- [ ] Používat issues, milestones a označené verze.
- [x] Vytvořit první GitHub Release s přenosnou sestavou aplikace.
- [x] Připravit lokální samostatný `win-x64` Release balíček pro budoucí GitHub Release.

## Doporučené milníky

### v0.1 — Working Core

- Funkční obousměrné šifrování.
- Minimálně tři šifry.
- Oddělené a otestované algoritmy.

### v0.5 — Great Desktop App

- Nové rozhraní, motivy a historie.
- Všech šest základních šifer.
- Import, export a kvalitní nápověda.

### v0.6 — Repo Polish

- Testy pokrývají prázdný vstup, Unicode, diakritiku, dlouhý text a hraniční hodnoty klíčů.
- Licence MIT a `LICENSE` soubor v repozitáři.
- README se screenshotem a anglickou variantou (`README.en.md`).
- Sekce Přispívání s pokyny pro pull requesty a hlášení chyb.
- Learn Mode vysvětluje všech šest šifer krok za krokem.
- Generátor vytváří vhodné číselné i textové klíče.

### v0.7 — Crack & Challenge

- Výzvy k rozluštění ve třech úrovních obtížnosti.
- Nápověda, odhalení řešení a okamžitá kontrola odpovědi.
- Heuristický odhad šifer Pozpátku, Atbash a Caesar.
- Seřazené návrhy s mírou jistoty a náhledem rozluštěného textu.

### v0.8 — Smooth UI

- Společný vizuální systém pro hlavní i vedlejší okna.
- Zaoblená tlačítka s hover, pressed, disabled a focus stavy.
- Jemnější karty editoru a nastavení s konzistentními okraji.
- Vyváženější rozložení akcí ve světlém i tmavém motivu.
- Stejně široká a vysoká tlačítka v pravidelných řádcích a sloupcích.

### v0.8.2 — Fix & Polish

- Výchozí fokus přímo v textovém vstupu.
- Logické pořadí tabulátoru v hlavním okně.
- Tooltipy pro důležité akce a jejich klávesové zkratky.
- Konzistentní Enter a Escape ve vedlejších oknech.
- Kontrola rozložení při minimální podporované velikosti okna.

### v0.9 — Share Cards

- Exportní centrum pro TXT a PNG na jednom místě.
- Živý náhled kartičky v rozměru 1200 × 630.
- Fialový, půlnoční a světlý vzhled kartičky.
- Vlastní nadpis a automatické označení šifry i směru operace.
- Uložení PNG a kopírování obrázku přímo do schránky.

### v0.9.1 — Stability Sweep

- Atomické ukládání historie a nastavení odolné proti přerušenému zápisu.
- Kontrola poškozených, nadměrných a neplatných lokálních dat.
- Bezpečné chování při obsazené schránce a nedostupném systémovém prohlížeči.
- Omezení velikosti ukládané historie bez omezení samotného zpracování textu.
- Unicode-safe zkracování textu pro PNG kartičky bez rozdělení emoji nebo diakritiky.
- CI kontroluje formátování a sestavení bez jediného varování.
- Aktualizovaný testovací balíček xUnit a 94 automatických testů.

### v1.0 — GitHub Release

- Dokončené testy a automatické sestavení.
- Česká a anglická lokalizace.
- Logo, screenshoty, dokumentace a instalační balíček.

## Definition of Done pro každou šifru

- [ ] Šifrování i odšifrování funguje pro běžné i hraniční vstupy.
- [ ] Parametry jsou validované a chyba uživateli řekne, co má opravit.
- [ ] Existují automatické testy.
- [ ] V aplikaci je stručné vysvětlení a příklad.
- [ ] Ovládání funguje myší i klávesnicí.
- [ ] Změna nepřidává žádná varování při sestavení.

## První pracovní session

- [x] Založit Git repozitář s hlavní větví `main`.
- [x] Převést projekt na moderní .NET.
- [x] Vytvořit `ICipher` a implementovat šifru Pozpátku.
- [x] Napsat první automatické testy.
- [x] Propojit algoritmus s novým přepínačem Zašifrovat/Odšifrovat.
- [ ] Udělat první commit: `feat: establish CipherDeck core`.
