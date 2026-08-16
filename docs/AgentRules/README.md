# Agent Policy Index

`AGENTS.md` is the constitution and dispatcher. This directory is the canonical
home for detailed, declarative repository policy. Focused operational guides
remain directly under `docs/`.

## Policy Model

Every policy declares its scope, triggers, hard stops, overrides, rules, and
verification expectations. Applicable policies accumulate. A closer path rule
may override a scoped default only when it names the override; invariants and
approval gates remain in force.

Rule strengths are:

- **Invariant:** an unconditional repository contract.
- **Approval gate:** stop before the gated mutation and request exact approval.
- **Scoped default:** applies in its declared scope unless explicitly overridden.
- **Workflow:** the procedure for performing an authorized operation.

`RuleManifest.json` is the machine-readable inventory used by repository lint.

## Modules

- `CSharp.md`: shared C# source-shape and formatting policy.
- `GameCodeDesign.md`: game assembly, service, state, lifecycle, and design policy.
- `RuntimePerformance.md`: allocation, hot-path, resource, and native-boundary policy.
- `Facades.md`: facade API, layout, debug, test, and benchmark policy.
- `Hashing.md`: closed hashing policy and extension gate.
- `ProjectsAndDependencies.md`: solutions, projects, packages, scopes, ECS, GL,
  maths, and menus.
- `GeneratedBindings.md`: generator, template, binding, and native-double policy.
- `Documentation.md`: public and generated documentation policy.

## Maintenance

Change one canonical policy rather than copying a rule into multiple routers.
Keep only short gate summaries in root files. When a policy changes, update the
manifest, relevant focused guide, lint enforcement when possible, and discovery
evaluation prompts. Machine-checkable rules may leave prompt text only after an
equivalent repository check exists.
