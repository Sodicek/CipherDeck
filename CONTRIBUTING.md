# Contributing to CipherDeck

Keep each change focused and explain the user-visible problem it solves.

## Branches and commits

- Create a branch from `main`, using `codex/<short-description>` for agent work.
- Commit one logical change at a time. Keep unrelated work in separate commits.
- Write concise English commit subjects using `type(scope): description`.
- Use `feat`, `fix`, `docs`, `test`, `refactor`, `ci`, or `chore` as appropriate.
- Describe the change, for example `fix(history): preserve data when saving fails`.
- Use the same convention for PR titles so squash commits remain readable.
- Preserve published commit history and release tags. Correct release notes and PR descriptions in place.

## Verification

For application changes, run:

```powershell
dotnet restore CipherDeck.sln
dotnet build CipherDeck.sln --configuration Release --no-restore -warnaserror
dotnet test CipherDeck.sln --configuration Release --no-build
dotnet format CipherDeck.sln --verify-no-changes --no-restore
```

Add regression tests for changed behavior. For UI changes, check the affected screens and describe any untested DPI or accessibility scenarios. Documentation changes require checking links, Markdown formatting, and consistency with the implementation.

## Pull requests and releases

Explain the problem, resulting behavior, and verification in the PR description. Wait for required CI checks before merging.

Keep release notes in `docs/releases/<version>.md`. Use real newlines and blank lines before lists. Publish Markdown from files with `gh release create --notes-file` or `gh release edit --notes-file`. For PR descriptions, use `gh pr create --body-file` or `gh pr edit --body-file`.

Do not pass multiline prose as a shell argument containing literal `\n`. After publishing, read back the body and confirm its formatting, version, download asset, and checksum.

Documentation-only changes do not require an application version bump or a new binary release.
