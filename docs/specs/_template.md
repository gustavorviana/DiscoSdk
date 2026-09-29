# SPEC-API-XXX — <Area name>

| | |
|---|---|
| **Status** | Draft |
| **PRD** | [PRD-API-XXX](../prd/api-XXX-slug.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | [resources/xxx](https://docs.discord.com/developers/resources/xxx) |
| **Last updated** | YYYY-MM-DD |

<!-- Naming mirrors the PRD: SPEC-API-NNN (specs/api-NNN-<slug>.md) or SPEC-SDK-NN (specs/sdk-NN-<slug>.md). -->

## 1. Summary
One paragraph describing the technical solution.

## 2. Projects and dependencies
Which projects are created or changed, what they depend on (project references, packages from
`Directory.Packages.props`).

## 3. Models and contracts
Wire payloads and DTOs (`DiscoSdk.Hosting/Models`), public interfaces (`DiscoSdk/Models`), JSON
converters, cache structures. Note nullability and field coverage gaps against the Discord object.

## 4. Components
Main types per project, with their public signatures:
- `DiscoSdk` — contracts (interfaces, builders, enums, exceptions).
- `DiscoSdk.Hosting` — implementation (REST clients, wrappers, surfaces, managers, dispatcher).

## 5. Public API
How a bot author reaches the feature: entry points on `IDiscordClient`/entities, builders,
`IRestAction` shapes, handler interfaces and contexts. Include a short usage example.

## 6. Discord surface
REST routes, gateway opcodes/events, required intents (flag privileged ones), bot permissions and
OAuth2 scopes involved.

## 7. Flows
Step-by-step sequence of the main path and the error path.

## 8. Concurrency and lifecycle
Threading model, cancellation, disposal, per-shard behaviour, ordering guarantees.

## 9. Errors and edge cases
| Situation | Expected behaviour |
|---|---|

## 10. Observability
Log messages (level), `Meter "DiscoSdk"` instruments, activity spans, redaction.

## 11. Tests
| Test class | Covers |
|---|---|
| `XxxTests` | XX-F01, XX-N01 |

## 12. Implementation plan
Steps in order; each step compiles and can become one commit. For areas that are already implemented,
rename this section **History** and list the key commits instead.

## 13. Decisions and rejected alternatives
Why it was done this way.
