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

## Follow-up work for v1.0.0-rc.1

### Separate UI coordination from controls

`MainForm` currently coordinates transformation, history, settings, import, export, analysis, detection, theming, and status messages. Extract application services for transformation sessions, history, and text-file I/O. Keep the form responsible for presenting state and handling controls.

Several secondary forms repeat layout and dialog-button setup. Small shared helpers can reduce duplication, but only after localization makes the common patterns clear.

### Improve test project boundaries

The single test project targets Windows because it references the WinForms application for renderer and persistence tests. Split platform-neutral Core tests from Windows UI/rendering tests so algorithm checks can run on Linux and Windows in CI.

Add a small automated UI smoke suite for startup, language switching, keyboard navigation, and minimum-size layouts. Continue manual DPI checks at 100%, 125%, 150%, and 200% until those scenarios are reliable in automation.

### Maintenance and release work

- Upgrade the test SDK and xUnit runner in a dedicated tooling change because their latest releases are major-version migrations.
- Add repeatable installer creation and installation/update/uninstallation checks for v1.0.0-rc.1.
- Automate release packaging, checksums, and release-note publication after the v0.10 feature set is complete.

## Recommended order

1. Run keyboard, minimum-size, and DPI review in both languages.
2. Extract application services from `MainForm`.
3. Split test boundaries and upgrade the test tooling.
4. Prepare the v1.0.0-rc.1 installer and release automation.
