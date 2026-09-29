# PRD-API-XXX — <Area name>

| | |
|---|---|
| **Status** | Draft |
| **Spec** | [SPEC-API-XXX](../specs/api-XXX-slug.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | [resources/xxx](https://docs.discord.com/developers/resources/xxx) — baseline in [`tools/baseline.json`](../tools/baseline.json) |
| **Last updated** | YYYY-MM-DD |

<!--
Naming: Discord API series → "PRD-API-NNN", file prd/api-NNN-<slug>.md.
        SDK framework series → "PRD-SDK-NN", file prd/sdk-NN-<slug>.md.
Status lifecycle: Draft → In review → Approved → Implemented → Obsolete.
"Discord docs" is machine-read by tools/coverage.py: list every docs.discord.com page this PRD owns.
SDK-series PRDs write "—" there.
-->

## 1. Problem
What pain does this solve for a bot author, and when does it show up?

## 2. Goals
- What must be true when this is done.

## 3. Out of scope
- What we are **not** doing now, even if it looks related. Link the PRD that owns it, if any.

## 4. Usage scenarios
Short stories: "As a bot author, when X happens, I expect Y."

## 5. Desired developer experience
The code a bot author should be able to write. Keep it to the public surface (`IDiscordClient`, entity
interfaces, builders, `IRestAction`, handlers).

```csharp
// example
```

## 6. Capabilities
One row per Discord capability. `tools/coverage.py check` validates this table against the Discord docs
and `Src/`.

| ID | Capability | Discord key | Priority | Status | SDK evidence |
|---|---|---|---|---|---|
| XX-F01 | ... | `GET /channels/{}` | Must | Implemented | `IChannel.Edit()` → `ChannelClient.EditAsync` |

- **Discord key**: one or more backticked keys. Use `METHOD /path` (placeholders written as `{}`),
  `event:NAME`, `op:N`, `close:N`, `intent:NAME`, `perm:NAME`, `component:N`, `itype:N`, `callback:N`,
  `context:N`, `cmdtype:N`, `option:N`, `chtype:N`, `webhook-event:NAME`, `voice-op:N`, `voice-close:N`,
  `cdn:NAME`, `format:NAME`, `locale:CODE` or `oauth2:NAME`. Use `—` for SDK-level behaviour that has no
  single Discord key.
- **Priority**: Must / Should / Could.
- **Status**: Implemented · Partial · Missing · Excluded (give the reason) · Deprecated (by Discord).
- **SDK evidence**: the public entry point first, then the internal implementation (`→`). Backticked
  symbols must exist in `Src/`.

## 7. Non-functional requirements
| ID | Requirement |
|---|---|
| XX-N01 | Allocation, throughput, thread-safety, Discord limits, security... |

## 8. Configuration
| Option | Default | Range | Config / builder API |
|---|---|---|---|

## 9. Compatibility
Is a breaking change acceptable? What gets `[Obsolete]`, and for how long? Which Discord deprecations
affect this area?

## 10. Acceptance criteria
- [ ] Verifiable, ideally by an automated test (name it) or a manual script in `Src/TomoriBot`.

## 11. Open questions
- Pending decisions, each with its provisional value.
