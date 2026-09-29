# Coverage tools

`coverage.py` keeps the PRD capability tables honest. It needs only Python 3.10+ and the standard library.

```bash
python3 docs/tools/coverage.py check                      # validate PRDs (exit 1 on errors)
python3 docs/tools/coverage.py summary                    # regenerate the summary table in docs/README.md
python3 docs/tools/coverage.py scaffold resources/channel --prefix CH   # draft rows for a docs page
python3 docs/tools/coverage.py inventory                  # dump Discord + SDK inventories
```

Add `--docs-path <checkout>` to read a local clone of
[`discord/discord-api-docs`](https://github.com/discord/discord-api-docs) instead of downloading pages
from `raw.githubusercontent.com` (downloads are cached under the system temp directory, per sha).

## What it checks

| Check | Fails when |
|---|---|
| Page ownership | An in-scope docs page is not listed in any PRD header (`Discord docs` row) nor in the excluded table of `docs/README.md`. |
| Completeness | A Discord item (route, gateway event, opcode, close code, intent, permission, typed enum value, CDN endpoint, message format, locale, webhook event) has no PRD row. |
| Uniqueness | The same Discord key appears in two rows. |
| Staleness | A row cites a key that no longer exists in the docs (allowed only on `Deprecated` rows, to flag SDK leftovers). |
| Status | `Implemented` rows whose keys are not all found in `Src/`, `Partial` rows with none found, `Missing` rows that the SDK actually implements. |
| Evidence | A backticked `Type` or `Type.Member` in the evidence column does not exist in `Src/`. |

SDK detection is automatic for REST routes (`new DiscordRoute("…")` paired with the `HttpMethod` in the
same member), gateway events (`DiscordEventDispatcher` cases), locales (`DiscordLocales`) and the enums
that mirror Discord tables (`OpCodes`, `DiscordIntent`, `DiscordPermission`, `ComponentType`,
`InteractionType`, `InteractionCallbackType`, `InteractionContextType`, `ApplicationCommandType`,
`SlashCommandOptionType`, `ChannelType`). Close codes, voice opcodes, webhook events, CDN endpoints,
message formats and OAuth2 URLs are asserted by hand and must cite evidence.

It also warns about SDK routes that match no documented route (typos, or endpoints Discord removed).

## Updating the Discord baseline

1. Put the new `discord/discord-api-docs` commit sha and date in `baseline.json`.
2. Run `check`. New Discord items show up as `unmapped`, removed ones as stale keys.
3. Add or update rows (use `scaffold` for new pages), then run `summary` and commit.
