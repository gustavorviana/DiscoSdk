# PRD-API-016 — Polls

| | |
|---|---|
| **Status** | Implemented |
| **Spec** | [SPEC-API-016](../specs/api-016-polls.md) |
| **Projects** | `DiscoSdk`, `DiscoSdk.Hosting` |
| **Discord docs** | [resources/poll](https://docs.discord.com/developers/resources/poll) — baseline in [`tools/baseline.json`](../tools/baseline.json) |
| **Last updated** | 2026-09-29 |

## 1. Problem
Polls are part of messages: a question, up to 10 answers with optional emoji, a duration of 1–768 hours and multi-select. Bots create polls, read live results, list the voters of each answer and end polls early. Votes also arrive as gateway events.

## 2. Goals
- A fluent `PollBuilder` that validates Discord's limits.
- Poll results (per-answer counts, finalised state) and expiry on received messages.
- Voter listing and early expiry.

## 3. Out of scope
- Poll vote gateway events: [PRD-API-004](api-004-gateway-events.md) (GE-F77, GE-F78).
- Message sending in general: [PRD-API-015](api-015-messages.md).

## 4. Usage scenarios
- As a bot author, I post "Next meetup?" with three dated answers for 24 hours.
- When the poll ends, I read the results and announce the winner.

## 5. Desired developer experience

```csharp
await channel.SendMessage()
    .SetPoll(new PollBuilder("Next meetup?")
        .AddAnswer("Friday").AddAnswer("Saturday")
        .SetDurationHours(24)
        .Build())
    .ExecuteAsync();
```

## 6. Capabilities
| ID | Capability | Discord key | Priority | Status | SDK evidence |
|---|---|---|---|---|---|
| PL-F01 | Get Answer Voters | `GET /channels/{}/polls/{}/answers/{}` | Must | Implemented | `ITextBasedChannel.RetrievePollVotersById()` → `MessageClient.GetPollVotersAsync` |
| PL-F02 | End Poll | `POST /channels/{}/polls/{}/expire` | Must | Implemented | `ITextBasedChannel.EndPollByIdAsync()` → `MessageClient.EndPollAsync` |
| PL-F03 | Poll create request (question, answers, duration, multi-select, layout) | — | Must | Partial | `PollBuilder` (`AddAnswer`, `SetDurationHours`, `AllowMultiSelect`, `SetLayout`) → `ICreateMessageBuilderBaseAction.SetPoll`; `SetDurationHours` rejects values above 168 h, while Discord allows up to 32 days (768 h) |
| PL-F04 | Poll media (text + emoji) for question and answers | — | Must | Implemented | `PollBuilder.SetQuestionEmoji`, `PollEmoji`, `PollText` |
| PL-F05 | Poll object on received messages (question, answers, expiry, multi-select, layout) | — | Must | Partial | `IMessageBase.Poll` (`Poll`) carries question, answers, multi-select and layout, but not `expiry` |
| PL-F06 | Poll results (`is_finalized`, per-answer counts, `me_voted`) | — | Should | Missing | `Poll` has no `results` |
| PL-F07 | Layout types | — | Could | Implemented | `PollLayoutType` |

## 7. Non-functional requirements
| ID | Requirement |
|---|---|
| PL-N01 | Builder validates the answer count (≤ 10), question (≤ 300) and answer (≤ 55) lengths, and duration before sending. The duration upper bound must match Discord's 768 h (PL-F03). |

## 8. Configuration
None.

## 9. Compatibility
Additive changes only; no breaking changes planned.

## 10. Acceptance criteria
- [x] Builder output matches Discord's schema and validates limits (`PollBuilderTests`).
- [x] Voters and end poll (`MessageClientTests`).
- [ ] Results and expiry are exposed (PL-F05, PL-F06).

## 11. Open questions
- None.
