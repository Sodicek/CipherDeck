# CipherDeck v0.10 code review

Review date: 2026-09-19  
Reviewed baseline: v0.9.1 plus the merged v0.10 foundation through PR #10

## Scope and verification

The review covered all application, core, and test source files, project and publish settings, persistence, localization boundaries, Unicode handling, UI event flow, GitHub Actions, and NuGet dependencies.

Baseline verification completed with a warning-free Release build, formatting verification, 101 passing tests, and no known vulnerable direct or transitive NuGet packages. The repository contains roughly 4,600 lines of C#.

## Fixed during the review

- Challenges and detection results now carry stable cipher IDs instead of relying on translated display names.
- Challenge answers use ordinal case-insensitive comparison, so behavior does not change under cultures such as Turkish.
- Cipher detection recognizes Unicode letters represented by surrogate pairs.
- Rail Fence and Skip decryption explanations build their diagrams from the restored plaintext.
- Share-card font sizing counts Unicode text elements instead of UTF-16 code units.
- The language preference has a validated Czech default and a single place for applying the UI culture.

Each behavior change has a regression test.

## Completed after the review

### Complete localization

All WinForms labels, messages, tooltips, help text, challenge text, detection explanations, Learn Mode steps, and share cards now use matching Czech and English resources. The persisted `EN/CZ` switch refreshes the main form without losing input or keys, and history display names resolve through stable cipher IDs.

Resource-completeness tests compare the actual Czech and English resource key sets, avoiding a manually maintained list that could drift.

### Keep long operations responsive

Encryption, frequency analysis, Learn Mode, and cipher detection now run outside the UI thread. Built-in ciphers and analysis services accept cancellation tokens, while a latest-operation runner cancels and discards obsolete live-preview work before it can overwrite a newer result.

### Separate UI and Core test projects

Platform-independent cipher, analysis, challenge, detection, learning, localization, and cancellation tests now live in `CipherDeck.Core.Tests` targeting plain `net8.0`. Windows UI, renderer, persistence, and application-localization tests remain in `CipherDeck.Tests`. CI runs the Core suite independently on Linux and the full desktop solution on Windows.

### Separate application services from the main form

Transformation sessions, cancellation, history mutations, persistence coordination, and bounded UTF-8 imports now live in focused application services. `MainForm` presents their results and coordinates controls instead of implementing those workflows directly. Service-level tests cover successful transformations, validation failures, history limits and save failures, and text-file import boundaries.

### Update test tooling

Both test projects now use Microsoft.NET.Test.Sdk 18.10.1 and xunit.runner.visualstudio 4.0.0 while retaining the xUnit v2 framework. The full suite was verified with the same .NET 8 SDK used by CI, and the dependency audit reports no outdated or known vulnerable packages.

## Follow-up work for v1.0.0-rc.1

### Reuse secondary-form layout carefully

Several secondary forms repeat layout and dialog-button setup. Small shared helpers can reduce duplication, but only after localization makes the common patterns clear.

### Expand UI verification

Add a small automated UI smoke suite for startup, language switching, keyboard navigation, and minimum-size layouts. Continue manual DPI checks at 100%, 125%, 150%, and 200% until those scenarios are reliable in automation.

### Release work

- Add repeatable installer creation and installation/update/uninstallation checks for v1.0.0-rc.1.
- Automate release packaging, checksums, and release-note publication for the v1 release-candidate flow.

## Recommended order

1. Run keyboard, minimum-size, and DPI review in both languages.
2. Prepare the v1.0.0-rc.1 installer and release automation.
