# AGENTS.md — MaxFunkTetris2024

## 1. Mission

`MaxFunkTetris2024` is a console Tetris game written in C# on .NET 8.

The project contains:

- deterministic Tetris game logic;
- a screen/state machine for menus, gameplay, pause, settings, help, leaderboard, and game over;
- keyboard-only controls;
- console rendering with minimal flicker;
- local persistence for settings and highscores;
- xUnit-based automated tests and test documentation.

Core goals:

- keep gameplay behavior stable and testable;
- preserve console UI correctness;
- keep runtime dependencies minimal;
- protect local user artifacts from accidental commits or format breakage.

## 2. Scope

These instructions apply to the whole repository.

Use this file as repository guidance for Codex/Jules/code-review agents. Keep changes small, reviewable, and task-scoped. Do not use this file to justify broad refactors or unsolicited product changes.

## 3. Source of truth

Primary files and directories:

- `Program.cs` — application bootstrap and runtime entry point.
- `MaxFunkTetris2024.csproj` — main .NET 8 executable project.
- `MaxFunkTetris2024.sln` — solution file.
- `Core/Game.cs` — main game loop, state machine, input/update/render orchestration, scoring, speed, modes, leaderboard behavior.
- `Core/GameBoard.cs` — board grid, collisions, merging pieces, clearing and shifting lines.
- `Core/Tetromino.cs` — tetromino shapes, position, rotation, save/restore state.
- `Core/GameSettings.cs` — loading and saving local `settings.json`.
- `Core/Enums.cs` — `GameState`, `Difficulty`, `GameMode`, `PowerUpType`.
- `UI/GameRenderer.cs` — game field and active-piece rendering.
- `UI/UiFrameRenderer.cs` — frames, panels, borders, help/static UI.
- `UI/UiStatsRenderer.cs` — stats, next piece, inventory, leaderboard/sidebar-like information.
- `UI/UiLayout.cs` — UI coordinates, widths, heights, layout constants.
- `UI/UiTheme.cs` — console colors and symbols.
- `MaxFunkTetris2024.Tests/` — xUnit unit/integration/smoke/UI/state-machine tests.
- `docs/TestPlan.md`, `docs/TestTasks.md`, `docs/CaseAnalyses.md` — testing plan, tasks, and case analyses.
- `README.md` — project description and user-facing build/run/test documentation.

## 4. Runtime and entry points

Main runtime entry point:

```text
Program.Main(string[] args)
```

Runtime flow:

```text
Program.Main -> new Game(15, 20) -> Game.Run()
```

`Program.Main` is expected to:

- set console encoding where needed;
- validate or handle minimum console size;
- create the game instance;
- start the game loop.

Main game loop:

```text
Game.Run()
```

Expected frame flow:

```text
HandleInput() -> Update() -> Render()
```

Do not change the runtime contract `Program -> Game(15,20) -> Run()` without explicit approval.

## 5. External integrations

Expected integrations are local/platform integrations only:

- .NET 8 runtime and SDK.
- .NET Console API (`Console.*`) for keyboard input, output, cursor positioning, colors, and window size.
- Local filesystem:
  - `settings.json` for user settings;
  - `highscores.txt` for local score persistence.
- NuGet test stack:
  - `xunit`;
  - `xunit.runner.visualstudio`;
  - `Microsoft.NET.Test.Sdk`;
  - `coverlet.msbuild`;
  - `coverlet.collector`.

No network APIs, HTTP clients, databases, queues, external services, or cloud SDKs are expected in normal runtime behavior. Do not add them unless explicitly requested.

## 6. Build, test, and quality commands

Run from repository root.

Restore dependencies:

```bash
dotnet restore
```

Build solution:

```bash
dotnet build MaxFunkTetris2024.sln
```

Run application:

```bash
dotnet run
```

Run tests:

```bash
dotnet test MaxFunkTetris2024.Tests/MaxFunkTetris2024.Tests.csproj
```

Publish release build:

```bash
dotnet publish -c Release
```

Optional coverage, if supported by the current test project packages:

```bash
dotnet test MaxFunkTetris2024.Tests/MaxFunkTetris2024.Tests.csproj --collect:"XPlat Code Coverage"
```

Lint/format:

- No dedicated linter, StyleCop, Roslyn analyzer, or formatter configuration is assumed unless present in the repository.
- Use `dotnet format` only by explicit request or repository policy.

Environment caveat:

- If `dotnet` is missing in the execution environment, report this as an environment blocker.
- Do not skip, delete, or weaken tests because the execution environment lacks the .NET SDK.

## 7. Non-negotiable behavior

### Game state machine

Do not casually change state-machine semantics for:

- `MainMenu`
- `Playing`
- `Paused`
- `Help`
- `Settings`
- `Leaderboard`
- `GameOver`

State transitions must remain predictable and covered by tests when changed.

### Console rendering

- Preserve minimal-flicker rendering.
- Do not add uncontrolled or per-frame `Console.Clear()` calls to active gameplay rendering.
- Use clearing only in intentional screen/overlay transitions where the existing design already does so.
- Preserve cursor-positioning safety patterns and defensive handling for console/headless environments.
- UI/layout changes must be manually smoke-tested in a console window of at least `80x30` where practical.

### Input and controls

- Do not change public keyboard behavior without explicit approval and documentation updates.
- Input handling must not make tests brittle in headless environments.
- Preserve defensive patterns around `Console.KeyAvailable`, `Console.ReadKey`, and cursor operations.

### Persistence

Do not change these file names or formats casually:

- `settings.json`
- `highscores.txt`

Expected persistence behavior:

- `settings.json` stores local user preferences and bindings/settings.
- `highscores.txt` stores local highscores, expected as integer-like score entries line by line unless repository code says otherwise.

Any persistence-format change requires:

- migration strategy;
- tests;
- documentation update;
- rollback/compatibility notes.

### Target framework and dependency footprint

- Preserve `net8.0` unless explicitly asked to migrate.
- Keep dependencies minimal.
- Do not add packages without a clear reason and explicit approval.

## 8. Risky modules

Treat changes in these areas as high-risk.

### `Core/Game.cs`

Main orchestrator / large stateful object.

Risks:

- breaking game loop behavior;
- breaking state transitions;
- breaking input/update/render order;
- altering scoring, level, speed, difficulty, or mode behavior;
- breaking leaderboard or settings interactions;
- introducing console/headless failures.

Any meaningful change here should include tests and a short PR risk note.

### `Core/GameBoard.cs`

Board grid, collision, merge, line clearing, and line shifting.

Risks:

- off-by-one errors;
- incorrect row clearing;
- incorrect collision detection;
- corrupted board state;
- regressions in game-over conditions.

### `Core/Tetromino.cs`

Shapes, rotations, state save/restore.

Risks:

- invalid rotation matrices;
- incorrect shape dimensions;
- broken restore behavior;
- piece position regressions.

### `UI/*`

Console layout, frame rendering, stats rendering, theme, cursor positioning.

Risks:

- flicker;
- alignment breakage;
- cursor exceptions;
- unreadable UI at expected console sizes;
- tests passing while manual UI is visually broken.

### `Core/GameSettings.cs`

Local settings persistence.

Risks:

- silent failure hiding configuration problems;
- broken compatibility with existing `settings.json`;
- accidental commit of local user settings;
- missing default fallback behavior.

### Leaderboard/highscore logic

Risks:

- broken top-score ordering;
- incompatible `highscores.txt` format;
- local user data loss.

## 9. Sensitive data and local artifacts

No API secrets, tokens, passwords, connection strings, or cloud credentials are expected in this repository.

Local user artifacts:

- `settings.json`
- `highscores.txt`

Rules:

- Do not commit user-local runtime artifacts unless explicitly requested as test fixtures.
- Do not hardcode machine-specific absolute paths.
- Do not add credentials or secrets.
- Treat user settings and highscores as local/personal runtime data.

Recommended `.gitignore` behavior, if not already present:

```gitignore
settings.json
highscores.txt
bin/
obj/
TestResults/
coverage/
```

Do not edit `.gitignore` unless the task includes repository hygiene or artifact management.

## 10. Change-management rules

- Keep PRs small and reviewable.
- Avoid mixing refactor, behavior change, formatting, dependencies, and tests in one PR.
- Preserve existing C# style and naming in touched areas.
- Prefer behavior-preserving refactors only when they are explicitly requested.
- Add or update tests for game-logic changes.
- For UI changes, include manual verification notes in the PR description.
- For persistence changes, include migration/compatibility notes.
- Do not weaken tests, CI, assertions, or runtime safeguards to make a PR pass.
- Do not add new dependencies unless explicitly justified.
- Update README/docs when commands, controls, persistence behavior, or gameplay behavior changes.

## 11. Testing policy

Minimum for logic changes:

```bash
dotnet build MaxFunkTetris2024.sln
dotnet test MaxFunkTetris2024.Tests/MaxFunkTetris2024.Tests.csproj
```

For game-board changes, cover:

- collisions;
- line clearing;
- merge behavior;
- game-over edge cases;
- board boundaries.

For tetromino changes, cover:

- shape generation;
- rotation behavior;
- boundary interactions;
- save/restore state.

For state-machine changes, cover:

- menu transitions;
- pause/resume;
- settings/help/leaderboard transitions;
- game over flow.

For UI changes:

- run tests where available;
- manually smoke-test with `dotnet run`;
- verify a console window of at least `80x30`;
- document before/after visual impact.

For persistence changes, cover:

- missing files;
- malformed files;
- read/write failures;
- backward compatibility with previous formats.

## 12. Review guidelines

Treat the following as P1 issues:

- State-machine transitions are changed without tests or rationale.
- Active gameplay rendering introduces uncontrolled per-frame `Console.Clear()`.
- Console APIs are used without defensive handling where tests/headless environments may fail.
- `GameBoard` collision, merge, or line-clearing behavior changes without tests.
- `Tetromino` rotation/state behavior changes without tests.
- `settings.json` or `highscores.txt` format changes without migration notes and tests.
- Target framework changes from `net8.0` without explicit approval.
- New runtime dependencies are added without explicit need.
- Tests are deleted, skipped, or weakened to make a PR pass.
- Local user artifacts are committed accidentally.
- Broad refactor of `Core/Game.cs` changes behavior without a narrow test plan.

Treat the following as P2 issues:

- UI layout/theme changes without manual smoke-test notes.
- Documentation not updated after controls, commands, gameplay behavior, or persistence changes.
- Large formatting-only churn mixed with functional changes.
- Duplicate logic added instead of using existing helpers.
- Silent catch blocks expanded in ways that reduce diagnosability.
- New comments explain obvious code rather than non-obvious constraints.

## 13. Security and reliability review focus

When reviewing console/UI changes, check:

- no uncontrolled `Console.Clear()` in the active gameplay frame loop;
- cursor positioning is bounded or defensively handled;
- headless/test environments do not fail unexpectedly;
- minimum console-size assumptions remain documented.

When reviewing game-logic changes, check:

- state transitions are intentional and tested;
- score/level/speed changes are documented;
- collisions and line clears are covered by tests;
- difficulty and game-mode behavior remains stable.

When reviewing persistence changes, check:

- missing or malformed `settings.json` / `highscores.txt` is handled safely;
- existing user files remain compatible;
- local data is not committed;
- errors are not silently hidden when diagnostics would be useful.

When reviewing tests, check:

- tests are meaningful and not overly tied to console environment details;
- no tests rely on user-local runtime files;
- no tests require machine-specific paths.

## 14. Practical do / don’t

Do:

- keep game logic deterministic where practical;
- keep rendering stable and low-flicker;
- preserve local-first/offline behavior;
- write tests for core logic;
- manually verify UI changes;
- document controls and persistence changes;
- report environment blockers such as missing `dotnet`.

Don’t:

- add network/cloud/API integrations casually;
- add broad refactors to `Core/Game.cs` without explicit request;
- weaken tests due to missing SDK/tools in the execution environment;
- commit `settings.json` or `highscores.txt`;
- introduce global per-frame `Console.Clear()` during gameplay;
- change target framework or dependencies casually;
- mix UI layout rewrites with gameplay logic changes.

## 15. Expected response format for agent tasks

For analysis/review tasks, respond with:

1. Short summary
2. Files inspected
3. Findings by severity
4. Verification commands run
5. Known failures or environment blockers
6. Minimal next steps

For code-change tasks, include:

1. What changed
2. Why it changed
3. Tests run
4. Manual verification, if UI/console behavior changed
5. Risks and rollback notes
6. Documentation updates, if any

Do not create commits, branches, files, or PRs unless the user explicitly asks for implementation.
