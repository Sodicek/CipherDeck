# CipherDeck

> Classic ciphers. Modern interface.

[![Build and test](https://github.com/Sodicek/CipherDeck/actions/workflows/ci.yml/badge.svg)](https://github.com/Sodicek/CipherDeck/actions/workflows/ci.yml)
[![Latest release](https://img.shields.io/github/v/release/Sodicek/CipherDeck)](https://github.com/Sodicek/CipherDeck/releases/latest)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

🇬🇧 [Read this in English](README.en.md)

<p align="center">
  <img src="CipherDeck.App/Assets/cipherdeck-logo.png" alt="CipherDeck logo" width="180">
</p>

CipherDeck je moderní desktopová aplikace pro zkoušení a pochopení klasických šifer. Nabízí jednoduché rozhraní, okamžitý převod textu a oddělené, automaticky testované šifrovací algoritmy.

<p align="center">
  <img src="docs/screenshots/main-window-dark.png" alt="Hlavní okno CipherDecku v tmavém motivu" width="640">
</p>

## Co umí v0.6

- text pozpátku s korektní podporou Unicode znaků,
- Caesarovu šifru s volitelným posunem,
- šifru Atbash,
- Vigenèrovu šifru s textovým klíčem,
- transpoziční šifru Rail Fence,
- přeskakování znaků s nastavitelným krokem,
- šifrování i odšifrování,
- prohození vstupu s výstupem,
- kopírování výsledku a počítadlo znaků,
- živý náhled výsledku při psaní,
- trvalou historii posledních 30 ručně provedených operací s možností opětovného načtení a vymazání,
- světlý a tmavý motiv s uložením volby i naposledy použité šifry,
- nápovědu a popisy všech podporovaných šifer přímo v aplikaci,
- import a export textových souborů v UTF-8,
- vlastní ikonu aplikace,
- analýzu četnosti písmen s grafem a podrobnou tabulkou,
- Learn Mode s vysvětlením každé šifry krok za krokem,
- generátor vhodných číselných a textových klíčů,
- klávesové zkratky `Ctrl+Enter`, `Ctrl+O` a `Ctrl+S`.

> [!WARNING]
> Klasické šifry nejsou bezpečné pro ochranu hesel ani citlivých dat. CipherDeck slouží pro výuku a zábavu.

## Spuštění

Projekt vyžaduje Windows a [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

Hotovou verzi bez nutnosti instalovat .NET najdeš v [nejnovějším GitHub Release](https://github.com/Sodicek/CipherDeck/releases/latest).

```powershell
dotnet run --project CipherDeck.App/CipherDeck.App.csproj
```

## Testy

```powershell
dotnet test CipherDeck.sln
```

## Vytvoření samostatné Windows aplikace

```powershell
dotnet publish CipherDeck.App/CipherDeck.App.csproj -p:PublishProfile=win-x64
```

## Struktura projektu

- `CipherDeck.App` – desktopové rozhraní CipherDecku,
- `CipherDeck.Core` – šifrovací algoritmy nezávislé na UI,
- `CipherDeck.Tests` – automatické testy,
- `ToDo.md` – roadmapa dalších verzí.

## Roadmapa

Další plánované funkce zahrnují šifrovací výzvy, automatický odhad jednoduché šifry a plnou lokalizaci rozhraní do angličtiny.

Podrobný plán je v [ToDo.md](ToDo.md).

## Přispívání

Návrhy a opravy jsou vítány. Před posláním pull requestu:

1. spusť `dotnet test CipherDeck.sln` a ověř, že všechny testy prochází,
2. u nové šifry dodrž rozhraní `ICipher` a přidej k ní automatické testy,
3. v popisu pull requestu stručně shrň, co a proč se mění.

Chybu nebo nápad na novou funkci nahlas přes [GitHub Issues](https://github.com/Sodicek/CipherDeck/issues).

## Licence

Projekt je dostupný pod licencí [MIT](LICENSE).
