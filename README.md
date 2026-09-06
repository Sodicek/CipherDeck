# CipherDeck

> Classic ciphers. Modern interface.

[![Build and test](https://github.com/Sodicek/CipherDeck/actions/workflows/ci.yml/badge.svg)](https://github.com/Sodicek/CipherDeck/actions/workflows/ci.yml)
[![Latest release](https://img.shields.io/github/v/release/Sodicek/CipherDeck)](https://github.com/Sodicek/CipherDeck/releases/latest)

<p align="center">
  <img src="CipherDeck.App/Assets/cipherdeck-logo.png" alt="CipherDeck logo" width="180">
</p>

CipherDeck je moderní desktopová aplikace pro zkoušení a pochopení klasických šifer. Nabízí jednoduché rozhraní, okamžitý převod textu a oddělené, automaticky testované šifrovací algoritmy.

## Co umí v0.5

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

Další plánované funkce zahrnují interaktivní vysvětlení algoritmů, šifrovací výzvy a českou i anglickou lokalizaci.

Podrobný plán je v [ToDo.md](ToDo.md).
