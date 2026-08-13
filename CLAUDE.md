# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project overview

Larcanum.ShellToolkit is a .NET library family (net10.0, C# 14) that makes shelling out to external
processes convenient, Bash-pipe-style, from .NET code. Three packages, one solution:

- **ShellToolkit** (`src/ShellToolkit/`) — core library. Key abstractions:
  - `ICommand` / `Command` — a single external command (`CommandPath` + `Arguments`).
  - `IPipeline` / `Pipeline` — chain of commands/steps via `.Pipe(...)`, like bash `|`.
  - `IPipelineStep` — one pipeline stage (`ProcessPipelineStep` for local OS processes; `SshPipelineStep`
    for remote, in the SSH project).
  - `ICommandRunner` — executes a bound pipeline (`.Bind(pipeline)` → `IBoundCommand` →
    `.ExecAsync()` / `.CaptureAsync()`). Local: `CommandRunner.Create()`.
  - `CommandResult` — captured stdout/stderr/exit code.
- **ShellToolkit.SSH** — `SshCommandRunner` implements `ICommandRunner` over SSH.NET, so the same
  Command/Pipeline API runs remotely.
- **ShellToolkit.Terminal** — helpers for building CLI apps on `System.CommandLine` (`Launcher`,
  `CliLogger`, typed `IArguments<T>` binding via compiled expression trees, `ICommand<TArg>`).

## Build & test

The solution is **not at the repo root** — always reference it explicitly:

```shell
dotnet restore src/ShellToolkit.slnx
dotnet build --no-restore src/ShellToolkit.slnx
dotnet test --no-build --verbosity normal src/ShellToolkit.slnx
```

This matches `.github/workflows/build.yml`. Tests are xUnit, in `ShellToolkit.UnitTests`.

## Code style

- `.editorconfig` at repo root is authoritative (`root = true`). Notable: **CRLF line endings for
  `*.cs`**, 4-space indent, UTF-8 no BOM.
- `Nullable` and `ImplicitUsings` are enabled everywhere.
- The codebase uses C# 14 `extension(...)` member block syntax (e.g. in `ICommandRunner.cs`,
  `IPipeline.cs`) instead of classic `static class XExtensions` with `this` params. This is
  intentional — don't "correct" it to the older extension-method style.

## Gotchas

- **Strong-name signing**: all projects sign with `src/ShellToolkit.snk`, which is committed
  to the repo (the `.gitignore` pattern `#*.snk` is a comment, not a real ignore rule). `ShellToolkit.csproj`
  grants `InternalsVisibleTo` to `ShellToolkit.UnitTests` via a hardcoded `PublicKey=...` token — if the
  `.snk` is ever regenerated, that token must be updated too.
- **Shared README**: `README.md` lives at the repo root but is content-linked (`Pack="true"`) into all
  three `.csproj` files for NuGet packaging. Editing it changes the listing for all three packages.
- **Versioning**: no GitVersion config — versioning comes from `Larcanum.GitInfo` (git-tag-derived) at
  pack time. Releases are triggered by pushing a `v*.*.*` tag (`.github/workflows/publish.yml`), which
  packs and pushes all three packages to NuGet.

## Git conventions

- Branches: `feature/<kebab-case-description>`.
- Commits: short, sentence-case, imperative/noun-phrase subjects, no conventional-commit prefixes.
  Multi-step refactors are split into numbered "part 1/2/3" commits, merged into `main` via PR (merge
  commits, not squash).
