# Voice support — pending

DiscoSdk does **not** support Discord's voice / video subsystem yet. This is intentional
for the current milestone and will be implemented in a future phase.

## What's missing

- Voice connection (UDP / Opus / RTP audio transport)
- Voice gateway (the secondary WebSocket used for voice sessions)
- `Update Voice State` outbound gateway command (`op 4` — joins/leaves a voice channel)
- Voice gateway events: `VOICE_STATE_UPDATE`, `VOICE_SERVER_UPDATE`, `VOICE_CHANNEL_EFFECT_SEND`
- Soundboard endpoints and `GUILD_SOUNDBOARD_SOUND_*` events

## What this affects on the public surface today

The following methods exist but **depend on the voice connection** to be useful:

- `IGuildStageChannel.RequestToSpeak()` — bot must be joined to the stage as audience first
- `IGuildStageChannel.CancelRequestToSpeak()` — same prerequisite

The REST calls succeed against Discord, but without an active voice session attached to
the channel they're no-ops in practice (Discord sees no audience-mode bot to act on).

Detecting the bot being promoted to speaker also depends on `VOICE_STATE_UPDATE`, which
is not dispatched today.

## What works without voice

- All stage instance REST + gateway events (open/modify/close, lifecycle dispatch)
- Guild scheduled events (any entity type — Stage, Voice, External)
- All non-voice gateway events

## Roadmap

Voice will land in a follow-up phase. Until then, treat any voice-dependent method as
a stub.
