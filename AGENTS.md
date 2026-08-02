# Repository Instructions

## Scope And Preflight

These instructions apply repository-wide. A closer `AGENTS.md` adds rules only
for its descendant directory. Topic policies under `docs/AgentRules/` remain
applicable when their path or semantic trigger matches the work.

Before designing or editing:

1. Read this repository's `README.md`, the requested files, and nearby project
   and source files.
2. Identify every repository and intended read or write path in the proposed
   solution. For each path, read every `AGENTS.md` from that repository's root
   through the target's parent directory, even when the client did not load a
   descendant file automatically.
3. Inspect project markers such as `FACADE.md`, project references, consumers,
   tests, generators, and templates that define the affected contract.
4. Use the router below to read every matching policy. Path and semantic
   triggers are cumulative, not alternatives.
5. Repeat this preflight before design when the scope expands or enters another
   repository. When applicability is uncertain, read the policy.

Rules have four strengths: **invariant**, **approval gate**, **scoped default**,
and **workflow**. An approval gate requires stopping before the gated mutation.
A scoped default applies unless a more specific policy explicitly overrides it.
Workflow guidance controls how to perform an authorized operation. Conflicts
resolve in favor of the more specific applicable rule; explicit rule IDs and
documented overrides take precedence over general prose.

## Core Invariants

### CORE-COMPAT-001: Unshipped development

AlvorKit and inheriting game repositories are unshipped unless a game root
explicitly declares otherwise and names its compatibility surfaces. Existing
repository-owned APIs, ABIs, commands, configuration, saves, serialized data,
protocols, generated output, and behavior are not compatibility surfaces merely
because they exist. Prefer the cleanest current design, change producers and
consumers together, and delete superseded APIs and implementations. Do not add
migrations, compatibility shims, legacy aliases or overloads, adapters,
dual-read/write paths, version bridges, or deprecation layers.

### CORE-FALLBACK-001: No fallback designs

Implement one correct design for the supported contract. Do not pair it with a
slower, legacy, approximate, reduced-fidelity, best-effort, default-result,
catch-and-retry, stale, partial, or otherwise inferior fallback. Strengthen the
representation or ask for a missing product decision. Separately specified
platforms and backends are first-class modes, not fallbacks.

## Approval And Authorization Gates

- **FACADE-API-001:** Before changing an established public API or documented
  public behavior in a project containing `FACADE.md`, read
  `docs/AgentRules/Facades.md` and obtain approval for the exact declaration and
  consumer change.
- **HASH-EXTEND-001:** Before adding a non-approved hashing mechanism, read
  `docs/AgentRules/Hashing.md`, explain why every approved mechanism fails the
  requirement, and obtain explicit approval.
- **NATIVE-BUILD-001:** Never run `AlvorKit.Script.NativeBuild`, build native
  runtime packages, or install native build dependencies on a developer machine
  without an explicit user request and permission for that run.

Repository-local gates may add stricter requirements, including proposal gates
for creating a facade. One approval satisfies multiple gates only when it
explicitly approves every gate's exact API and scope.

## Working Mode And Commit Mode

Use **Working Mode** by default. Make the requested change or investigation;
run targeted builds, tests, visual checks, or generated-output checks when they
help the task. Do not create leases, run broad lint/coverage/test gates, stage,
commit, push, open a PR, or call work commit-ready unless the user asks. Report
normally expected Commit Mode checks that were skipped.

Use **Commit Mode** only when the user explicitly asks for cleanup, final
verification, staging, committing, pushing, a PR, or commit readiness. Inventory
the intended scope and diffs, preserve concurrent work, stage only explicit
paths, recheck the staged diff, and avoid broad commands such as `git add .`.
Read `docs/AgentCoordination.md` when lease-backed coordination is requested.

## Policy Router

Read these policies when any listed trigger matches:

- `docs/AgentRules/CSharp.md`: any hand-authored or generated C# or C# template.
- `docs/AgentRules/RuntimePerformance.md`: runtime loops, polling, simulation,
  rendering, resources, native boundaries, allocation, disposal, or teardown.
- `docs/AgentRules/GeneratedBindings.md`: generators, templates, bindings,
  generated documentation, generated projects, or native test doubles.
- `docs/AgentRules/Hashing.md`: hashes, checksums, fingerprints, deterministic
  sampling, table mapping, or cryptographic hashing.
- `docs/AgentRules/ProjectsAndDependencies.md`: new or reorganized projects,
  package references, launchable projects, DI scopes, ECS, GL ownership, maths,
  or menus.
- `docs/AgentRules/Documentation.md`: public or generated documentation.
- `docs/AgentRules/GameCodeDesign.md`: game/runtime design in a game repository.
- `docs/AgentRules/Facades.md`: a `FACADE.md` project, its paired debug project,
  its consumers, tests, or benchmarks; also any proposed new facade.

Focused canonical guides such as `ECS.md`, `Maths.md`, `GlOwnership.md`,
`ProjectSplitModel.md`, and `GameScopeOrganization.md` remain required when the
router policy names them. `docs/AgentRules/RuleManifest.json` is the machine-
readable inventory of modules, gates, triggers, and overrides.

## Repository Areas And Workflows

Read the closest scoped instructions under `src/`, `scripts/`, `demos/`,
`tests/`, `native/`, and `res/templates/` before changing descendants.

- Use `scripts/AlvorKit.Script.AlvorSense` and read `docs/AlvorSense.md` for
  engine-native visual automation.
- Read `docs/AgentLiveDevelopment.md` before using LiveCode, frozen inspection,
  Source Update, or combined live workflows. Agent-authored live files belong
  under `tmp/live/<workspace-id>/`; never expose capability tokens.
- Read `docs/GeneratedOutputChecks.md` when generated-output review applies.
- Read `docs/AgentVerification.md` for lint, timing, coverage, and artifacts.
- Sibling game repositories route to `docs/GameRepositoryInstructions.md` and
  keep game-specific invariants in their own root file. Do not assume another
  sibling game exists.
