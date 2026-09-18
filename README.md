# VOCABULARY FLOATING WIDGET

Portable always-on-top Windows widget that rotates **HSK Chinese → Vietnamese** vocabulary and quizzes you with four multiple-choice answers. One self-contained `.exe`, no installer, no Administrator rights.

## Run (Windows)

Publish a single file:

```bash
dotnet publish VocabularyWidget/VocabularyWidget.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true
```

Then copy `VocabularyWidget/bin/Release/net8.0-windows/win-x64/publish/VocabularyWidget.exe` anywhere and run it. `words.json` and `settings.json` are written next to the executable.

Requires Windows 10/11 x64. The machine does **not** need a separate .NET runtime.

## Behavior

- 260×110, no title bar, always on top, hidden from the taskbar (system tray).
- Appears at the bottom-right; drag to move. Left-click the text to start a **4-answer quiz**; drag does not start a quiz.
- Every cycle (default 5 minutes) shows either the **Chinese word** or the **Vietnamese definition**.
- Click a choice (or press 1–4). Esc cancels. Correct → green border, “Chính xác!”, next card after 1.5s. Wrong → red border, reveal answer, same card stays.
- Right-click: manage words, skip to next card, set interval (1 / 3 / 5 / 10 minutes), exit.
- Manage window: add / edit / delete, import `.csv` (`word,definition`) or `.txt` / tab-separated HSK lists.
- Cards you miss are shown more often.
- Shipped lexicon: ~496 HSK cards (Chinese → Vietnamese) in `words.json`.

## Browser preview

This repo also includes `preview/index.html`, a behavior replica of the same state machine (for Linux review and demos). Open the file in a browser, or:

```bash
python3 -m http.server 8765 --directory preview
```

Then visit `http://localhost:8765/`.

## Develop

```bash
dotnet test VocabularyWidget.sln
```

WinForms UI lives in `VocabularyWidget/`; domain logic (JSON, matching, import, spaced-repetition weights) lives in `VocabularyWidget.Core/` so it can be tested on non-Windows agents.

## Data

`words.json` is a JSON array of HSK cards:

```json
{
  "Id": "1",
  "Word": "爱",
  "Definition": "Yêu; thương; yêu quý",
  "ReviewCount": 0,
  "CorrectCount": 0
}
```
# VOCABULARY-FLOATING-WIDGET
