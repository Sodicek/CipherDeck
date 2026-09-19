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

## Remaining work before v0.10

### Complete localization

Most WinForms labels, messages, tooltips, help text, challenge text, detection explanations, and Learn Mode steps are still hard-coded in Czech. Move them into Czech and English resources, add the language switch, refresh the main form without losing input or keys, and resolve history display names through stable cipher IDs.

Resource-completeness tests should compare the actual Czech and English resource key sets. A manually maintained list of expected keys can drift as strings are added.

### Keep long operations responsive

Encryption, frequency analysis, Learn Mode, and especially the 25-pass Caesar detection run synchronously on the UI thread. A multi-megabyte imported file can freeze the window. Introduce cancellable background execution for expensive actions and cancel stale live-preview work when the input changes again.

### Separate UI coordination from controls

`MainForm` currently coordinates transformation, history, settings, import, export, analysis, detection, theming, and status messages. Extract application services for transformation sessions, history, and text-file I/O. Keep the form responsible for presenting state and handling controls.

Several secondary forms repeat layout and dialog-button setup. Small shared helpers can reduce duplication, but only after localization makes the common patterns clear.

### Improve test project boundaries

The single test project targets Windows because it references the WinForms application for renderer and persistence tests. Split platform-neutral Core tests from Windows UI/rendering tests so algorithm checks can run on Linux and Windows in CI.

Add a small automated UI smoke suite for startup, language switching, keyboard navigation, and minimum-size layouts. Continue manual DPI checks at 100%, 125%, 150%, and 200% until those scenarios are reliable in automation.

### Maintenance and release work

- Upgrade the test SDK and xUnit runner in a dedicated tooling change because their latest releases are major-version migrations.
- Add repeatable installer creation and installation/update/uninstallation checks for v0.11.
- Automate release packaging, checksums, and release-note publication after the v0.10 feature set is complete.

## Recommended order

1. Finish all Czech and English resources and resource-key validation.
2. Add the persisted language switch and refresh every open main-window label safely.
3. Localize history, challenges, detection, Learn Mode, export cards, help, and runtime errors.
4. Run keyboard, minimum-size, and DPI review in both languages.
5. Move expensive operations off the UI thread.
6. Split test boundaries and prepare the v0.11 installer pipeline.
