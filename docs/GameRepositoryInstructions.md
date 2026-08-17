# AlvorKit Game Repository Instructions

## Scope And Preflight

These shared instructions apply to an AlvorKit game repository whose root
`AGENTS.md` routes here. Paths below assume the game repository root and an
AlvorKit sibling at `../AlvorKit`.

Before design or editing:

1. Read the game `README.md`, requested files, nearby projects and source, and
   every applicable local `AGENTS.md` from the game root to each target.
2. Identify all intended game and AlvorKit read/write paths. Read AlvorKit's
   root and applicable scoped instructions before designing a solution that
   changes AlvorKit; do not wait until the edit begins.
3. Inspect `FACADE.md`, consumers, tests, benchmarks, generators, templates,
   resource references, and documentation relevant to the contract.
4. Read every matching policy in the router below. Semantic and path triggers
   are cumulative. Repeat when the solution scope expands.

Do not assume another sibling game repository exists.

## Shared Invariants

- **CORE-COMPAT-001:** The game is unshipped unless its own root explicitly
  declares it shipped and names the compatibility surface. Change
  repository-owned producers and consumers together and delete superseded
  designs; do not add compatibility shims, legacy aliases or overloads,
  migrations, adapters, or dual paths.
- **CORE-FALLBACK-001:** Implement one correct design for the supported
  contract. Do not add slower, legacy, approximate, reduced-fidelity,
  best-effort, default-result, retry, stale, or partial fallback paths.
- **CORE-LANGUAGE-001:** Whenever repository-owned prose, identifiers, labels,
  or diagnostics use English, use American English exclusively. Preserve exact
  externally owned identifiers and verbatim quotations. The established
  AlvorKit `Maths` terminology is the sole repository-owned exception.
- **SOLUTION-PAIR-001:** When a primary `<Name>.slnx` has an existing sibling
  `<Name>.Dev.slnx`, make every solution change to both files in the same task.
  Never modify only one member of the existing pair.
- AlvorKit is owned source, not a fixed external dependency. Put engine, UI,
  injection, windowing, GL lifetime, maths, bindings, scripts, and reusable
  harness capability in AlvorKit rather than forcing a game-local workaround.
  Keep genuinely game-specific behavior in the game.
- Keep AlvorKit and game status, staging, and commits separate.

## Working And Verification Defaults

Use Working Mode unless the user explicitly requests cleanup, final
verification, staging, committing, pushing, a PR, or commit readiness. Targeted
builds and AlvorSense checks are allowed when useful. Broad lint, coverage,
timing gates, generated-output review, staging, and commits are explicit-request
or Commit Mode work.

- **GAME-TEST-DEFAULT-001:** Do not create, add, expand, or run game unit tests
  unless the user explicitly requests tests or an existing test is being
  diagnosed. Prefer targeted builds and behavior or AlvorSense verification.
- **FACADE-TEST-OVERRIDE-001:** A project containing `FACADE.md` requires
  dedicated behavioral unit tests through its supported production
  composition. This overrides `GAME-TEST-DEFAULT-001` for facade creation,
  behavior, and implementation changes.
- Game C# source files have a 350-line hard ceiling. Test files may be up to
  750 lines when related scenarios read better together.
- Add a checked-in VS Code launch configuration and referenced build task for
  directly launchable projects whose project files are tracked by Git. Ignored
  and untracked projects must not change checked-in `.vscode` configuration.

## Game Policy Router

Read all matching documents before design:

- `../AlvorKit/docs/AgentRules/CSharp.md`: every C# file and C# template.
- `../AlvorKit/docs/AgentRules/GameCodeDesign.md`: game source, runtime
  services, state, failure semantics, ownership, concurrency, or data layout.
- `../AlvorKit/docs/AgentRules/RuntimePerformance.md`: update/render/polling,
  simulation, resources, validation, native boundaries, allocation, cleanup,
  disposal, or teardown.
- `../AlvorKit/docs/AgentRules/Facades.md`: existing or proposed facades,
  paired debug facades, consumers, tests, and benchmarks.
- `../AlvorKit/docs/AgentRules/Hashing.md`: any hash, checksum, fingerprint,
  deterministic procedural sampling, or table mapping.
- `../AlvorKit/docs/AgentRules/ProjectsAndDependencies.md`: `.slnx` solution
  changes, project creation or reorganization, references, package roles,
  scopes, ECS, GL, maths, and menus.
- `../AlvorKit/docs/AgentRules/GeneratedBindings.md`: generators, templates,
  generated output or documentation, bindings, and native test doubles.
- `../AlvorKit/docs/AgentRules/Documentation.md`: public or generated docs.

Hard gates remain visible here:

- **FACADE-API-001:** Stop before changing an established facade public API or
  documented behavior; obtain approval for the exact declarations and consumer
  scope.
- A game-local facade proposal gate may also apply before creating or materially
  expanding a facade. Approval covers both only when exact API and scope are
  explicit.
- **HASH-EXTEND-001:** Stop and obtain approval before adding hashing machinery
  outside the approved closed set.
- **NATIVE-BUILD-001:** Do not run native package builds or install native build
  dependencies without an explicit user request and permission for that run.

## Canonical Guides

- `../AlvorKit/docs/ECS.md`: game Ents, components, handles, arenas, Indexed
  contexts, hooks, bags, ownership, iteration, and teardown. Use `Ent` and
  `Ents`; the word `Entity` is banned in code, prose, names, paths, and labels.
- `../AlvorKit/docs/ProjectSplitModel.md`: pure, frontend, menu, backend,
  server, protocol, and executable package boundaries.
- `../AlvorKit/docs/GameScopeOrganization.md`: DI scopes, loaders, states,
  controls, seeding, and dependency ordering.
- `../AlvorKit/docs/GlOwnership.md`: hierarchical `GlLayer` ownership.
- `../AlvorKit/docs/Maths.md` and `MathsReference.md`: mandatory AlvorKit maths
  types and `ScalarMath`; add the reference instead of inventing local vector,
  box, range, tuple, parallel-scalar, clamp, lerp, or min/max forms.
- `../AlvorKit/docs/MenuAuthoring.md`: AlvorKit UI menus.
- `../AlvorKit/docs/Logging.md`: engine-loop and custom-host logging.
- `../AlvorKit/docs/AlvorSense.md`: visual automation.
- `../AlvorKit/docs/AgentLiveDevelopment.md`: LiveCode, frozen inspection, and
  Source Update workflow and cleanup.
- `../AlvorKit/docs/AgentVerification.md`, `AgentCoordination.md`, and
  `GeneratedOutputChecks.md`: verification, coordination, and generation
  workflows.

Before adding a `ProjectReference`, preserve the package split. Pure packages
must not reference UI, GL, frontend, menu, audio, or windowing. Frontend may
depend on `AlvorKit.Engine`, but loop ownership belongs to executables, menus,
or another composition package rather than `AlvorKit.Engine.Loop`.

When AlvorSense applies, run it from the game root with `--workdir .` so ignored
artifacts stay in the game. It cannot drive real desktop windows or OS-level
focus/input; verify those manually or with purpose-built tooling.
