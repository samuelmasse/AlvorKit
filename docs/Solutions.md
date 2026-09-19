# Repository solutions

Every repository, including AlvorKit, has one generated, gitignored
`<checkout-name>.slnx` at its root. Project files are the source of truth. There
is no checked-in solution, template, project-list manifest, or aggregate solution.

## Generate and build

From AlvorKit:

```powershell
dotnet run --project scripts/AlvorKit.Script.Solution -- --repo-root .
dotnet build AlvorKit.slnx
```

From a sibling game:

```powershell
dotnet run --project ../AlvorKit/scripts/AlvorKit.Script.Solution -- --repo-root .
dotnet build MyGame.slnx
```

Use the checkout directory name in the build command. With no explicit root,
the generator finds the current Git checkout. Project discovery requires Git
metadata; run `git init` first in a newly scaffolded directory or source archive.
The generator requires Git and the .NET 10 SDK and bootstraps from its own project.
It evaluates MSBuild without running build targets or native builds.

Generate after selecting the local binding and maths mode. Existing active
projects under `out/bindgen` and `out/mathgen` enter the evaluated graph through
references. Without those projects, consumers use their declared packages.
Generation neither creates those projects nor changes the chosen mode.

When updating an existing checkout from the old solution workflow, remove its
previously generated `*.Dev.slnx` files once before generating the new solution.
The generator rejects old or authored solutions instead of overwriting them.

## Changing projects

When adding, moving, renaming, or deleting a project:

1. Edit the authored `.csproj` files, references, imports, and affected paths.
2. Run the generation command above for each affected repository. Do not assume
   a watcher is running or wait for it before issuing solution-based commands.
3. Build the affected projects or solution as appropriate for the change.

Never edit solution XML, use `dotnet sln add/remove`, or maintain membership
through IDE solution commands. Fix discovery inputs or evaluated references
when membership is wrong. Deleting an entry from the generated solution cannot
exclude a project; the next generation restores it.

## Membership and presentation

Root projects come from `git ls-files --cached --others --exclude-standard`:
tracked files and non-ignored untracked files that still exist on disk.
This honors nested `.gitignore`, `.git/info/exclude`, and global ignore rules;
tracked files remain included even when an ignore rule matches them.
Eligible `.csproj` files are at the repository root or under
`src`, `lib`, `scripts`, `tests`, `demos`, and `bench`. Project symlinks are skipped.
Native packaging projects and scaffold resources are outside these source areas.

In AlvorKit, reusable libraries belong in `src`, including `AlvorKit.Testing`;
`tests` contains runnable test projects. Discovery also supports existing game
layouts under `lib`.

The installed SDK evaluates roots and transitive `ProjectReference` dependencies
in Debug and Release. Imports and conditional references are evaluated by
MSBuild. Required generated and sibling projects are included even outside the
discovery areas. Unrelated engine demos and tests do not enter a game solution.
Configuration-specific dependencies have matching solution build mappings.
These solutions support Debug/Release with the default Any CPU platform; add
explicit generator support before introducing other solution configurations.

Set `<WorkspaceProject>false</WorkspaceProject>` only for an intentional
independent-root exclusion. A reference to that project still includes it.
Set `<WorkspaceStartup>true</WorkspaceStartup>` on at most one local executable.
Otherwise, a single local executable becomes the startup project automatically.
Multiple executables without a selection leave startup selection to the IDE.
Projects under `src` appear one level up: at the solution root for local source,
or directly under `Engine` for sibling engine source. Other solution folders use
capitalized labels, such as `Tests` and `Scripts`. Project paths retain their
filesystem casing. Output is sorted and written atomically only when its
contents change.

## Continuous updates

Add `--watch` to keep one repository synchronized, or watch a parent directory:

```powershell
dotnet run --project AlvorKit/scripts/AlvorKit.Script.Solution -- --parent-directory . --watch
```

Parent discovery includes immediate Git checkouts containing managed projects;
it creates a separate solution inside each one. No aggregate file is created.
Repository additions, deletions, and directory moves trigger discovery. Explicit
`--repo-root` watches also survive deleting and recreating that path. Git ignore rules
and index updates refresh the affected repository's Git project list. An
unchanged project list does not trigger graph evaluation.

New checkouts remain pending while Git initializes its metadata. During a clone
or checkout, an existing Git index lock suspends generation for that repository
and removes its previous solution. Removing the lock triggers discovery and
generation immediately through filesystem notifications. Git changes during
evaluation discard that document before publication. A stranded Git lock keeps
the repository pending; the watcher never deletes Git locks or retries on a timer.

Each solution watches the filesystem inputs read while MSBuild evaluates its
projects: project files, imports, existence conditions, and wildcard queries.
This includes external and generated dependencies, imports with custom file
extensions, and files that do not exist yet. A shared input change reevaluates
its consumers; an unrelated repository stays idle. Ordinary C# content edits
do not trigger evaluation. Changes to wildcard membership can trigger it.

Notifications are coalesced for 200 ms, bounded at one second during continuous
activity. Changes arriving during evaluation remain queued. There is no periodic
scan, polling timer, or timed retry. Subscriptions are installed before reading
inputs and replaced after evaluation; MSBuild's XML-cache reads are checked
for changes during subscription setup. Shared native watches sit above the
repository trees so Windows can move project directories and checkouts. An
input-path index routes notifications to the affected solutions. Per-user leases
under the operating system's local application-data directory prevent overlapping
watchers from owning the same checkout, including across parent and explicit
repository modes. No persistent lock files or project file streams are held inside
managed checkouts; deleting a repository releases its subscriptions and lease.
Ordinary evaluation and publication still perform short-lived file IO.
A failed OS watch, including a notification
buffer overflow, exits with an error instead of continuing with incomplete
notifications. Restart it to establish a fresh set of subscriptions.

An invalid graph is reported explicitly and its previous generated solution is
removed. Known project inputs remain observed so correcting them regenerates
the solution. If MSBuild cannot load an import, the watcher stops with the import
diagnostic: MSBuild does not expose the unresolved imported path reliably, so
the watcher cannot establish complete subscriptions. Fix the import and restart
the watcher. It never invents missing references or retains stale output as a
successful result. Moving a project still requires updating its authored project
references.

`--watch` runs in the invoking process until stopped. It does not register a
startup task or service, and cloning a repository installs no watcher. Running
at sign-in requires separate, optional machine configuration. One-shot
generation remains the normal command for agents and CI.

Stop and restart the watcher after changing the generator implementation. A
persistent process also needs restarting after SDK or environment changes. A
persistent host should run a published copy outside the repository's regular
`bin` directories so IDE builds can replace tooling assemblies. Generated
solutions can be deleted and recreated at any time; do not commit them or edit
their XML.

## CI and other commands

CI selects its generated-source mode, invokes the same generator once, then
restores and builds the resulting solution. It never starts a watcher or relies
on developer-generated output. Generate again after changing project inputs.
`--check` evaluates and compares without writing, returning a failure for stale
or invalid output. `--list-only` previews repository discovery without evaluation.

For example, validate an existing solution from AlvorKit without updating it:

```powershell
dotnet run --project scripts/AlvorKit.Script.Solution -- --repo-root . --check
```

This checks freshness and graph validity; it does not compile the solution.

Full repository lint uses the solution but restricts formatting to local source
areas, so a game cannot format sibling engine files. Use
`dotnet test <solution> --no-build --no-restore` after building to run every test
project in the solution. Unrelated engine test projects are not included in game solutions.
Root discovery uses Git checkout markers, and engine resource discovery uses
the authored `AlvorKit.Packages.props`; neither needs a solution file.
