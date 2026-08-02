# Agent Rule Migration Ledger

This ledger records the canonical destination of every section that previously
lived in the large AlvorKit root or shared game-repository guide. The move is
organizational unless a resolution is explicitly recorded below.

| Previous section                                                          | Canonical destination                                                                                       |
| ------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------- |
| Scope; Development Status; Fallback Designs; Game Repository Instructions | root `AGENTS.md` and `GameRepositoryInstructions.md`                                                        |
| Working/Commit Mode; Coordination                                         | root `AGENTS.md`, `docs/AgentCoordination.md`, and `docs/AgentVerification.md`                              |
| Visual Automation; Live Runtime Debugging; Visual Checks                  | `docs/AlvorSense.md` and `docs/AgentLiveDevelopment.md`                                                     |
| Line Length; C# Defaults                                                  | `docs/AgentRules/CSharp.md`                                                                                 |
| Generated Output; Native Test Doubles; Package Version Properties         | `docs/AgentRules/GeneratedBindings.md`                                                                      |
| VS Code Launch; Project Split; Game Scopes; ECS; GL; Maths; Menus         | `docs/AgentRules/ProjectsAndDependencies.md` and the named focused guides                                   |
| Hashing Policy                                                            | `docs/AgentRules/Hashing.md`                                                                                |
| Documentation                                                             | `docs/AgentRules/Documentation.md`                                                                          |
| Runtime Allocation Discipline; Hot-Path Data Layout                       | `docs/AgentRules/RuntimePerformance.md`                                                                     |
| Game Code Design Style                                                    | `docs/AgentRules/GameCodeDesign.md`                                                                         |
| Facade Projects; Debug Facades; Tests; Benchmarks                         | `docs/AgentRules/Facades.md`                                                                                |
| Tests And Verification Gates                                              | `docs/AgentVerification.md`, scoped test instructions, and game defaults in `GameRepositoryInstructions.md` |

## Deliberate Resolutions

- Runtime deletion, disposal, unload, and scope teardown are allocation-
  sensitive. Only genuinely cold final process-shutdown orchestration may
  allocate when the cost is intentional. This resolves the former contradiction
  between root and `src/AGENTS.md` in favor of the stronger runtime contract.
- The repository-owned `sealed` prohibition is preserved unchanged. Existing
  declarations are not silently grandfathered or migrated by this policy-only
  change; enforcement or removal of that invariant requires a separate explicit
  decision.
- The facade API gate remains distinct from any game-local facade proposal gate.
  Approval satisfies both only when the exact API and implementation/cutover
  scope are explicitly approved.
