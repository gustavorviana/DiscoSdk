# SPEC-API-016 — Polls

| | |
|---|---|
| **Status** | Implemented |
| **PRD** | [PRD-API-016](../prd/api-016-polls.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | [resources/poll](https://docs.discord.com/developers/resources/poll) |
| **Last updated** | 2026-09-29 |

## 1. Summary
`PollBuilder` produces a `Poll` (question, answers, duration, multi-select, layout) that is attached to a message through `SetPoll`. Received messages expose the poll through `IMessageBase.Poll`. Voters come from `MessageClient.GetPollVotersAsync` through `PollVotersPaginationAction`, and `MessageClient.EndPollAsync` ends a poll.

## 2. Projects and dependencies
`DiscoSdk/Models/Messages/Pools/*` (`Poll`, `PollAnswer`, `PollText`, `PollEmoji`, `PollLayoutType`, `PollBuilder`), and `DiscoSdk.Hosting` (`MessageClient`, `PollVotersPaginationAction`).

## 3. Models and contracts
`Poll` holds `question`, `answers`, `duration`, `allow_multiselect` and `layout_type`. It does not map `expiry` or `results` (PL-F05, PL-F06).

## 4. Components
| Type | Responsibility |
|---|---|
| `PollBuilder` | Validates the question (≤ 300), answers (1–10, ≤ 55 characters each) and duration (1–168 h, below Discord's 768 h). |
| `PollVotersPaginationAction` | `after` / `limit` over `GET …/polls/{id}/answers/{answer}`. |
| `MessageClient.EndPollAsync` | `POST …/polls/{id}/expire`. |

## 5. Public API
`PollBuilder`, `ICreateMessageBuilderBaseAction.SetPoll`, `ITextBasedChannel.RetrievePollVotersById`, `ITextBasedChannel.EndPollByIdAsync`, `IMessageBase.Poll`.

## 6. Discord surface
Two routes. Vote events need the `GUILD_MESSAGE_POLLS` or `DIRECT_MESSAGE_POLLS` intent.

## 7. Flows
Create: builder → `Poll` → `SetPoll` → message create (SPEC-API-015).

## 8. Concurrency and lifecycle
Stateless request builders; wrappers are snapshots of the returned models.

## 9. Errors and edge cases
| Situation | Expected behaviour |
|---|---|
| Duration over 168 h | `ArgumentOutOfRangeException` (**stricter than Discord**, see PL-F03). |
| More than 10 answers | `InvalidOperationException`. |
| Ending someone else's poll | Discord 403 → `InsufficientPermissionException`. |

## 10. Observability
REST metrics only (SPEC-SDK-05).

## 11. Tests
| Test class | Covers |
|---|---|
| `PollBuilderTests` | PL-F03, PL-F04, PL-F07, PL-N01 |
| `MessageClientTests` | PL-F01, PL-F02 |
| `PollVoteDispatchTests` | vote events (PRD-API-004) |

Implemented or Partial requirements without a covering test: PL-F05.

## 12. History
| Commit | Change |
|---|---|
| `459f51c` | Poll voters and end poll wiring. |

Next steps: raise the duration cap to 768 h, and add `expiry` and `results` to `Poll`.

## 13. Decisions and rejected alternatives
- **Builder produces a plain model**: the same poll can be reused, and validation stays local.
