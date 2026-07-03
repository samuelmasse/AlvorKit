# Agent Coordination

This document holds the coordination details that are too operational for the
root `AGENTS.md`. The root policy still controls when these workflows apply.

## When To Read This

Read this document when the user asks for lease-backed coordination, staging,
committing, cleanup across multiple agents, or help handling overlapping work.
Do not create or refresh advisory leases in Working Mode unless the user asks
for lease-backed coordination.

## Complaints

When an agent has a complaint about the tools it uses, or believes another agent
is working at the same time in a way that disturbs its task, write a concise
complaint under `out/complaints/`. Use a descriptive Markdown filename when
possible. These complaints are later input for agent quality-of-life
improvements.

## Commit Mode Inventory

Use Commit Mode only when the user explicitly asks for cleanup, final
verification, staging, committing, pushing, opening a PR, or making work ready
to commit.

Before editing or staging in Commit Mode:

- Identify the requested scope.
- Inspect status and diffs for the intended paths.
- Read the relevant changed files.
- Preserve concurrent work and ask when ownership or intent is unclear.

When staging or committing:

- Identify the exact files or globs intended for staging.
- Inspect status and diffs for those paths before staging.
- Stage only the intended paths; avoid broad commands such as `git add .`.
- Recheck status and the staged diff before committing.
- If unrelated or surprising changes appear in the same paths, pause and ask or
  clearly separate the requested changes.

## Advisory Leases

Advisory leases under `out/agents/` are coordination hints, not hard locks. If
an active lease overlaps your intended write paths, avoid the overlap when
practical or leave a short conflict note explaining why it is unavoidable.

Read-only exploration does not need a lease. When lease-backed coordination is
requested, create a lease before editing files, generating code, running
repo-wide or broad scoped formatters, refreshing generated bindings, performing
cleanup, staging files, or doing other work that could disturb another agent.

Use the lease helper instead of hand-editing JSON:

```powershell
dotnet run --project scripts\AlvorKit.Script.AgentLease -- start --agent <id> --task "Short task" --path "src/Foo/**"
dotnet run --project scripts\AlvorKit.Script.AgentLease -- touch --agent <id>
dotnet run --project scripts\AlvorKit.Script.AgentLease -- list
dotnet run --project scripts\AlvorKit.Script.AgentLease -- check --agent <id> --path "src/Foo/**"
dotnet run --project scripts\AlvorKit.Script.AgentLease -- conflict --agent <id> --task "Short task" --path "src/Foo.cs" --reason "Brief reason"
dotnet run --project scripts\AlvorKit.Script.AgentLease -- done --agent <id>
```

You may set `ALVORKIT_AGENT_ID` instead of passing `--agent` on every command.
When starting without an explicit agent id, the helper generates one and prints
it; reuse that id for `touch`, `check`, `conflict`, and `done`.

Lease files are JSON at `out/agents/<agent-id>.json`. Use repository-relative
paths and globs such as `src/Foo.cs`, `scripts/AlvorKit.Script.Lint/**`,
`*.slnx`, `*`, or `repo-wide`. Keep path lists specific enough for overlap
checks to be useful. Valid modes are `write`, `generate`, `format`, `test`,
`cleanup`, and `review`.

When using leases for generated or verification artifacts under `out/`, claim the
exact artifact directory once it is known, such as
`out/bindgen-review/<case>-<suffix>/**` or
`out/coverage/runs/<run-id>/**`. Do not claim shared output roots such as
`out/bindgen-review/**` or `out/coverage/**` when agents can work in separate
run directories. If a tool prints the directory after startup, begin with the
source paths and then refresh the lease with the concrete output path.

Leases expire five minutes after their last update by default. When using one,
refresh it before editing again after any long-running command, and refresh it
once in a while during longer work. Use `--timeout-minutes <n>` when a
longer-running operation needs a larger stale window. Delete the lease with
`done` when work finishes; stale leases expire automatically if cleanup is
missed.

If staging is requested with lease-backed coordination, run `check` for the
exact files or globs you intend to stage and confirm your current lease still
covers them. If overlap is unavoidable, write a conflict note; the helper stores
it under `out/agents/conflicts/`.
