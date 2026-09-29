#!/usr/bin/env python3
"""DiscoSdk ↔ Discord API coverage checker.

Cross-references three inventories and fails when they disagree:

  1. Discord  — everything the official docs (github.com/discord/discord-api-docs, pinned in
                baseline.json) expose to bots: REST routes, gateway events, opcodes, close codes,
                intents, permission flags and typed enums (components, interactions, commands,
                channels) plus webhook event types.
  2. SDK      — what Src/ actually implements: DiscordRoute templates paired with their HttpMethod,
                dispatcher event names, and the C# enums that mirror Discord's typed tables.
  3. PRDs     — the "Capabilities" table (section 6) of every docs/prd/api-*.md and sdk-*.md, which claims a
                status for each Discord item.

Subcommands:
  check              validate PRDs against Discord + SDK (exit 1 on any error)
  summary            rewrite the coverage block in docs/README.md (between coverage markers)
  scaffold <page>    print capability rows for a Discord docs page, e.g. `resources/channel`
  inventory          dump the Discord and SDK inventories (debugging aid)

Only the Python standard library is used. Docs are downloaded from raw.githubusercontent.com at the
pinned sha and cached in the system temp dir; pass --docs-path to use a local checkout instead.
"""
from __future__ import annotations

import argparse
import json
import os
import re
import sys
import tempfile
import urllib.request
from dataclasses import dataclass, field
from pathlib import Path

REPO = Path(__file__).resolve().parents[2]
DOCS_DIR = REPO / "docs"
PRD_DIR = DOCS_DIR / "prd"
SRC_DIR = REPO / "Src"
BASELINE_FILE = Path(__file__).resolve().parent / "baseline.json"
DOCS_URL = "https://docs.discord.com/developers/"
RAW_URL = "https://raw.githubusercontent.com/{repo}/{sha}/{path}"

STATUSES = ("Implemented", "Partial", "Missing", "Excluded", "Deprecated")

# Navigation scope: which docs.json pages are "capabilities a bot can use". Everything under the
# Reference tab, the Interactions/Components groups of the bots tab, and the Teams topic. Guides,
# tutorials, Activities and the Social SDK client docs are narrative and out of scope by rule.
SCOPE_PREFIXES = (
    ("Reference",),
    ("Bots & Companion Apps", "Interactions"),
    ("Bots & Companion Apps", "Components"),
)
SCOPE_PAGES = ("developers/topics/teams",)

# Typed tables extracted from specific Discord pages: category -> (page, heading, key-kind).
#   key-kind "value": key is the numeric value (e.g. component:18)
#   key-kind "bit":   key is the NAME, matched against SDK flag enums by bit index
#   key-kind "text":  key is the textual Value column (webhook events)
TABLES = {
    "op": ("topics/opcodes-and-status-codes", "Gateway Opcodes", "value"),
    "close": ("topics/opcodes-and-status-codes", "Gateway Close Event Codes", "value"),
    "voice-op": ("topics/opcodes-and-status-codes", "Voice Opcodes", "value"),
    "voice-close": ("topics/opcodes-and-status-codes", "Voice Close Event Codes", "value"),
    "perm": ("topics/permissions", "Bitwise Permission Flags", "bit"),
    "component": ("components/reference", "Component Types", "value"),
    "itype": ("interactions/receiving-and-responding", "Interaction Type", "value"),
    "callback": ("interactions/receiving-and-responding", "Interaction Callback Type", "value"),
    "context": ("interactions/receiving-and-responding", "Interaction Context Types", "value"),
    "cmdtype": ("interactions/application-commands", "Application Command Types", "value"),
    "option": ("interactions/application-commands", "Application Command Option Type", "value"),
    "chtype": ("resources/channel", "Channel Types", "value"),
    "webhook-event": ("events/webhook-events", "Event Types", "text"),
    "audit": ("resources/audit-log", "Audit Log Events", "value"),
    "msgtype": ("resources/message", "Message Types", "value"),
    "automod-trigger": ("resources/auto-moderation", "Trigger Types", "value"),
    "automod-event": ("resources/auto-moderation", "Event Types", "value"),
    "automod-action": ("resources/auto-moderation", "Action Types", "value"),
    "sticker-format": ("resources/sticker", "Sticker Format Types", "value"),
    "webhook-type": ("resources/webhook", "Webhook Types", "value"),
    "invite-target": ("resources/invite", "Invite Target Types", "value"),
    "scheduled-entity": ("resources/guild-scheduled-event", "Guild Scheduled Event Entity Types", "value"),
}

# SDK enums mirroring Discord tables: category -> (C# enum name, "value" | "bit").
SDK_ENUMS = {
    "op": ("OpCodes", "value"),
    "perm": ("DiscordPermission", "bit"),
    "intent": ("DiscordIntent", "bit"),
    "component": ("ComponentType", "value"),
    "itype": ("InteractionType", "value"),
    "callback": ("InteractionCallbackType", "value"),
    "context": ("InteractionContextType", "value"),
    "cmdtype": ("ApplicationCommandType", "value"),
    "option": ("SlashCommandOptionType", "value"),
    "chtype": ("ChannelType", "value"),
    "audit": ("AuditLogActionType", "value"),
    "msgtype": ("MessageType", "value"),
    "automod-trigger": ("AutoModerationTriggerType", "value"),
    "automod-event": ("AutoModerationEventType", "value"),
    "automod-action": ("AutoModerationActionType", "value"),
    "sticker-format": ("StickerFormatType", "value"),
    "webhook-type": ("WebhookType", "value"),
    "invite-target": ("InviteTargetType", "value"),
    "scheduled-entity": ("ScheduledEventEntityType", "value"),
}

# Categories with no automatic SDK detection: status is asserted by hand and must cite evidence.
MANUAL_CATEGORIES = {"close", "voice-op", "voice-close", "webhook-event", "cdn", "format", "oauth2"}

RECEIVE_EVENT_SKIP = {"Hello", "Reconnect", "Invalid Session", "Client Status Object", "Activity Object"}
HTTP_METHODS = ("GET", "POST", "PUT", "PATCH", "DELETE")


# --------------------------------------------------------------------------------------------
# Discord inventory
# --------------------------------------------------------------------------------------------

@dataclass
class DiscordItem:
    key: str
    category: str
    page: str
    title: str
    deprecated: bool = False


class DiscordDocs:
    def __init__(self, docs_path: str | None, baseline: dict):
        self.local = Path(docs_path) if docs_path else None
        self.baseline = baseline
        self.cache = Path(tempfile.gettempdir()) / "discosdk-coverage" / baseline["sha"]

    def read(self, rel: str) -> str:
        if self.local:
            return (self.local / rel).read_text(encoding="utf-8")
        target = self.cache / rel
        if not target.exists():
            url = RAW_URL.format(repo=self.baseline["repo"], sha=self.baseline["sha"], path=rel)
            with urllib.request.urlopen(url, timeout=60) as res:
                data = res.read().decode("utf-8")
            target.parent.mkdir(parents=True, exist_ok=True)
            target.write_text(data, encoding="utf-8")
        return target.read_text(encoding="utf-8")

    def page(self, page: str) -> str:
        return self.read(f"developers/{page}.mdx")

    def scoped_pages(self) -> list[str]:
        nav = json.loads(self.read("docs.json"))
        found: list[str] = []

        def walk(node, labels):
            if isinstance(node, dict):
                label = node.get("group") or node.get("tab") or node.get("dropdown") or node.get("anchor")
                path = labels + [label] if label else labels
                for k in ("navigation", "tabs", "dropdowns", "anchors", "groups", "pages"):
                    if k in node:
                        walk(node[k], path)
            elif isinstance(node, list):
                for child in node:
                    if isinstance(child, str):
                        if child in SCOPE_PAGES or any(tuple(labels[: len(p)]) == p for p in SCOPE_PREFIXES):
                            found.append(child)
                    else:
                        walk(child, labels)

        walk(nav, [])
        return sorted({p.removeprefix("developers/") for p in found})


def strip_md(text: str) -> str:
    text = re.sub(r"\[([^\]]*)\]\([^)]*\)", r"\1", text)
    return text.replace("\\", "").replace("`", "").replace("*", "").strip()


def normalize_path(path: str) -> str:
    path = strip_md(path).split("?")[0].strip()
    path = re.sub(r"\{[^}]*\}", "{}", path)
    if not path.startswith("/"):
        path = "/" + path
    return path.rstrip("/") or "/"


def parse_table(text: str, heading: str) -> list[list[str]]:
    lines = text.splitlines()
    for i, line in enumerate(lines):
        if re.match(r"^#{2,6}\s+" + re.escape(heading) + r"\s*$", line):
            rows, started = [], False
            for row in lines[i + 1:]:
                if row.startswith("|"):
                    started = True
                    cells = [c.strip() for c in row.strip().strip("|").split("|")]
                    if not all(re.fullmatch(r":?-+:?", c) for c in cells):
                        rows.append(cells)
                elif started:
                    break
                elif row.startswith("#"):
                    break
            return rows[1:]  # drop header row
    raise LookupError(f"table '{heading}' not found")


def section_tables(text: str, heading: str) -> list[list[list[str]]]:
    """All tables inside a '## heading' section (header row included), in document order."""
    section = re.split(r"^## " + re.escape(heading) + r"\s*$", text, maxsplit=1, flags=re.M)[1]
    section = re.split(r"^## ", section, maxsplit=1, flags=re.M)[0]
    tables, current = [], []
    for line in section.splitlines():
        if line.startswith("|"):
            cells = [c.strip() for c in line.strip().strip("|").split("|")]
            if not all(re.fullmatch(r":?-+:?", c) for c in cells):
                current.append(cells)
        elif current:
            tables.append(current)
            current = []
    if current:
        tables.append(current)
    return tables


def slug(name: str) -> str:
    return re.sub(r"[^A-Za-z0-9]+", "_", strip_md(name)).upper().strip("_")


def discord_inventory(docs: DiscordDocs, pages: list[str]) -> dict[str, DiscordItem]:
    items: dict[str, DiscordItem] = {}

    def add(item: DiscordItem):
        if item.key in items:  # same route documented twice (e.g. Start Thread variants)
            items[item.key].title += f" / {item.title}"
            items[item.key].deprecated &= item.deprecated
        else:
            items[item.key] = item

    for page in pages:
        text = docs.page(page)
        heading = ""
        lines = text.splitlines()
        for i, line in enumerate(lines):
            h = re.match(r"^#{1,3}\s+(.*)$", line)
            if h:
                heading = strip_md(h.group(1))
            for m in re.finditer(r'<Route method="(\w+)">(.*?)</Route>', line):
                key = f"{m.group(1).upper()} {normalize_path(m.group(2))}"
                # Deprecation is stated either in the heading or in a Danger/Warning right below the route.
                nearby = " ".join(lines[i + 1:i + 6]).lower()
                deprecated = "deprecated" in heading.lower() or "deprecated in favor" in nearby or "has been disabled" in nearby
                add(DiscordItem(key, "route", page, heading, deprecated))

    # OAuth2 URLs (token endpoints are plain URLs, not <Route> tags).
    for cells in parse_table(docs.page("topics/oauth2"), "OAuth2 URLs"):
        url = strip_md(cells[0])
        if "/api/" in url:
            add(DiscordItem("POST " + normalize_path(url.split("/api", 1)[1]), "route", "topics/oauth2", strip_md(cells[1])))
        else:
            add(DiscordItem("oauth2:" + slug(url.rsplit("/", 1)[-1]), "oauth2", "topics/oauth2", strip_md(cells[1])))

    # Reference page: message formatting, CDN endpoints and locales.
    ref = docs.page("reference")
    for cells in section_tables(ref, "Message Formatting")[0][1:]:
        add(DiscordItem("format:" + slug(cells[0]), "format", "reference", strip_md(cells[0])))
    cdn = next(t for t in section_tables(ref, "Image Formatting") if any("Path" in c for c in t[0]))
    for cells in cdn[1:]:
        add(DiscordItem("cdn:" + slug(cells[0]), "cdn", "reference", strip_md(cells[0])))
    for cells in section_tables(ref, "Locales")[0][1:]:
        add(DiscordItem("locale:" + strip_md(cells[0]), "locale", "reference", strip_md(cells[1])))

    # Gateway receive events.
    text = docs.page("events/gateway-events")
    section = text.split("## Receive Events", 1)[1]
    for h in re.findall(r"^####\s+(.+)$", section, re.M):
        name = strip_md(h)
        if name in RECEIVE_EVENT_SKIP:
            continue
        add(DiscordItem("event:" + re.sub(r"[^A-Za-z0-9]+", "_", name).upper().strip("_"),
                        "event", "events/gateway-events", name))

    # Intents (code block in events/gateway).
    text = docs.page("events/gateway")
    block = text.split("### List of Intents", 1)[1].split("```", 2)[1]
    for name, bit in re.findall(r"^([A-Z_]+) \(1 << (\d+)\)", block, re.M):
        add(DiscordItem(f"intent:{name}", "intent", "events/gateway", f"{name} (1 << {bit})"))

    # Typed tables.
    for cat, (page, heading, kind) in TABLES.items():
        for cells in parse_table(docs.page(page), heading):
            plain = [strip_md(c) for c in cells]
            if kind == "text":
                name, key = plain[0], plain[1]
            elif kind == "bit":
                name = plain[0].strip()
                bit = re.search(r"1 << (\d+)", cells[1])
                key, name = name, f"{name} (1 << {bit.group(1) if bit else '?'})"
            else:
                value = next((c for c in plain if re.fullmatch(r"\d+", c)), None)
                name = next((c for c in plain if not re.fullmatch(r"\d+", c)), "")
                if value is None:
                    continue
                key = value
            add(DiscordItem(f"{cat}:{key}", cat, page, name, "deprecated" in name.lower()))
    return items


def discord_bits(docs: DiscordDocs) -> dict[str, dict[str, int]]:
    """NAME -> bit index for the flag categories, used to match SDK flag enums."""
    bits = {"perm": {}, "intent": {}}
    for cells in parse_table(docs.page("topics/permissions"), "Bitwise Permission Flags"):
        m = re.search(r"1 << (\d+)", cells[1])
        if m:
            bits["perm"][strip_md(cells[0])] = int(m.group(1))
    block = docs.page("events/gateway").split("### List of Intents", 1)[1].split("```", 2)[1]
    for name, bit in re.findall(r"^([A-Z_]+) \(1 << (\d+)\)", block, re.M):
        bits["intent"][name] = int(bit)
    return bits


# --------------------------------------------------------------------------------------------
# SDK inventory
# --------------------------------------------------------------------------------------------

@dataclass
class SdkInventory:
    routes: dict[str, list[str]] = field(default_factory=dict)   # key -> ["Class.Method", ...]
    events: dict[str, list[str]] = field(default_factory=dict)   # NAME -> ["IHandler", ...]
    enums: dict[str, dict[int, str]] = field(default_factory=dict)  # enum -> value/bit -> member
    types: set[str] = field(default_factory=set)
    words: set[str] = field(default_factory=set)
    locales: set[str] = field(default_factory=set)


def _interpolated(literal: str) -> str:
    out, i = [], 0
    while i < len(literal):
        two = literal[i:i + 2]
        if two in ("{{", "}}"):
            out.append(two[0])
            i += 2
        elif literal[i] == "{":
            i = literal.index("}", i) + 1  # interpolation hole, e.g. {query}
        else:
            out.append(literal[i])
            i += 1
    return "".join(out)


MEMBER_RE = re.compile(r"^\s*(?:public|private|internal|protected)\b[^;={]*?\b(\w+)\s*(?:<[^>]*>)?\s*\(", re.M)
TEMPLATE_RE = re.compile(r'new\s+(?:DiscordRoute|StringBuilder)\(\s*(\$?)"((?:[^"\\]|\\.)*)"')


def sdk_inventory() -> SdkInventory:
    inv = SdkInventory()
    files = sorted(SRC_DIR.rglob("*.cs"))
    for f in files:
        text = f.read_text(encoding="utf-8-sig")
        inv.types.update(re.findall(r"\b(?:class|interface|enum|struct|record)\s+(\w+)", text))
        inv.words.update(re.findall(r"\b\w+\b", text))

        cls = re.search(r"\b(?:class|record|struct)\s+(\w+)", text)
        owner = cls.group(1) if cls else f.stem
        members = list(MEMBER_RE.finditer(text))
        for idx, m in enumerate(members):
            body = text[m.end(): members[idx + 1].start() if idx + 1 < len(members) else len(text)]
            if "DiscordRoute(" not in body:
                continue
            templates = []
            for t in TEMPLATE_RE.finditer(body):
                raw = _interpolated(t.group(2)) if t.group(1) else t.group(2)
                if raw.startswith("?") or "/" not in raw and not re.match(r"^[a-z-]+$", raw):
                    continue
                templates.append(normalize_path(raw))
            methods = sorted(set(re.findall(r"HttpMethod\.(Get|Post|Put|Patch|Delete)\b", body)))
            for path in set(templates):
                for method in methods:
                    inv.routes.setdefault(f"{method.upper()} {path}", []).append(f"{owner}.{m.group(1)}")

        for enum_name, kind in {v for v in SDK_ENUMS.values()}:
            em = re.search(r"\benum\s+" + enum_name + r"\b[^{]*\{(.*?)\}", text, re.S)
            if not em:
                continue
            values = inv.enums.setdefault(enum_name, {})
            for member, expr in re.findall(r"^\s*(\w+)\s*=\s*([^,\n]+)", em.group(1), re.M):
                bit = re.search(r"<<\s*(\d+)", expr)
                num = re.match(r"\s*(\d+)", expr)
                if kind == "bit" and bit:
                    values[int(bit.group(1))] = member
                elif kind == "value" and num:
                    values[int(num.group(1))] = member

    locales = (SRC_DIR / "DiscoSdk/DiscordLocales.cs").read_text(encoding="utf-8-sig")
    inv.locales = set(re.findall(r'"([a-z]{2}(?:-[A-Za-z0-9]+)?)"', locales))

    dispatcher = (SRC_DIR / "DiscoSdk.Hosting/Gateway/Events/DiscordEventDispatcher.cs").read_text(encoding="utf-8-sig")
    for name, process in re.findall(r'case\s+"([A-Z][A-Z_]+)"\s*:\s*\n\s*await\s+(\w+)', dispatcher):
        body = re.search(r"Task\s+" + process + r"\s*\(.*?\n    \}", dispatcher, re.S)
        handlers = re.findall(r"HandleAllAsync<(\w+)", body.group(0)) if body else []
        inv.events[name] = sorted(set(handlers)) or [process]
    for f in (SRC_DIR / "DiscoSdk.Hosting/Gateway").rglob("*.cs"):
        for name in re.findall(r'EventType,\s*"([A-Z][A-Z_]+)"', f.read_text(encoding="utf-8-sig")):
            inv.events.setdefault(name, [f.stem])
    return inv


def sdk_evidence(item: DiscordItem, sdk: SdkInventory, bits: dict) -> list[str] | None:
    """Return SDK evidence for a Discord item, [] if not found, None if not auto-detectable."""
    if item.category in MANUAL_CATEGORIES:
        return None
    if item.category == "route":
        return sdk.routes.get(item.key, [])
    if item.category == "event":
        return sdk.events.get(item.key.split(":", 1)[1], [])
    if item.category == "locale":
        code = item.key.split(":", 1)[1]
        return ["DiscordLocales"] if code in sdk.locales else []
    enum_name, kind = SDK_ENUMS[item.category]
    values = sdk.enums.get(enum_name, {})
    raw = item.key.split(":", 1)[1]
    index = bits[item.category].get(raw) if kind == "bit" else int(raw)
    member = values.get(index) if index is not None else None
    return [f"{enum_name}.{member}"] if member else []


# --------------------------------------------------------------------------------------------
# PRD inventory
# --------------------------------------------------------------------------------------------

@dataclass
class Row:
    prd: str
    id: str
    capability: str
    keys: list[str]
    status: str
    evidence: str
    line: int


@dataclass
class Prd:
    path: Path
    number: str
    title: str
    pages: list[str]
    rows: list[Row]


KEY_RE = re.compile(r"^(?:(?:GET|POST|PUT|PATCH|DELETE) /\S*|[a-z0-9-]+:[A-Za-z0-9_.-]+)$")


def read_prds() -> list[Prd]:
    prds = []
    paths = sorted(PRD_DIR.glob("api-[0-9][0-9][0-9]-*.md")) + sorted(PRD_DIR.glob("sdk-[0-9][0-9]-*.md"))
    for path in paths:
        text = path.read_text(encoding="utf-8")
        title = text.splitlines()[0].lstrip("# ").strip()
        header = next((l for l in text.splitlines() if l.startswith("| **Discord docs**")), "")
        pages = re.findall(re.escape(DOCS_URL) + r"([a-z0-9/-]+)", header)
        rows, in_caps, cols = [], False, None
        for n, line in enumerate(text.splitlines(), 1):
            if line.startswith("## "):
                in_caps = line.startswith("## 6.")
                cols = None
                continue
            if not in_caps or not line.startswith("|"):
                continue
            cells = [c.strip() for c in line.strip().strip("|").split("|")]
            if cols is None:
                cols = [c.lower() for c in cells]
                continue
            if all(re.fullmatch(r":?-+:?", c) for c in cells):
                continue
            rec = dict(zip(cols, cells))
            keys = re.findall(r"`([^`]+)`", rec.get("discord key", ""))
            rows.append(Row(path.name, rec.get("id", ""), rec.get("capability", ""), keys,
                            rec.get("status", "").strip("* "), rec.get("sdk evidence", ""), n))
        number = title.split("—", 1)[0].strip()  # e.g. "PRD-API-014"
        if path.name.startswith("api-") and not pages:
            raise SystemExit(f"{path.name}: api-series PRD without a 'Discord docs' header row")
        prds.append(Prd(path, number, title, pages, rows))
    return prds


def excluded_pages() -> list[str]:
    text = (DOCS_DIR / "README.md").read_text(encoding="utf-8")
    section = text.split("<!-- excluded:start -->", 1)[-1].split("<!-- excluded:end -->", 1)[0]
    return re.findall(re.escape(DOCS_URL) + r"([a-z0-9/-]+)", section)


# --------------------------------------------------------------------------------------------
# Commands
# --------------------------------------------------------------------------------------------

def load(args):
    baseline = json.loads(BASELINE_FILE.read_text(encoding="utf-8"))
    if args.sha:
        baseline["sha"] = args.sha
    docs = DiscordDocs(args.docs_path, baseline)
    pages = docs.scoped_pages()
    return docs, pages, discord_inventory(docs, pages), sdk_inventory(), discord_bits(docs)


def cmd_check(args) -> int:
    docs, pages, discord, sdk, bits = load(args)
    prds = read_prds()
    errors, warnings = [], []

    mapped = {p for prd in prds for p in prd.pages}
    excluded = set(excluded_pages())
    for page in pages:
        if page not in mapped and page not in excluded:
            errors.append(f"docs page '{page}' is neither covered by a PRD header nor listed as excluded")
    for page in (mapped | excluded) - set(pages):
        warnings.append(f"page '{page}' is referenced but is not an in-scope docs page at {docs.baseline['sha'][:7]}")

    seen: dict[str, Row] = {}
    for prd in prds:
        for row in prd.rows:
            where = f"{row.prd}:{row.line} {row.id}"
            if row.status not in STATUSES:
                errors.append(f"{where}: unknown status '{row.status}'")
                continue
            if not row.keys and "—" not in row.evidence and row.status in ("Implemented", "Partial") and not row.evidence:
                errors.append(f"{where}: {row.status} row without Discord key needs SDK evidence")
            found_any, missing = False, []
            for key in row.keys:
                if not KEY_RE.match(key):
                    errors.append(f"{where}: malformed key '{key}'")
                    continue
                if key in seen:
                    errors.append(f"{where}: key '{key}' already mapped at {seen[key].prd}:{seen[key].line}")
                seen[key] = row
                item = discord.get(key)
                if item is None and row.status == "Deprecated":
                    continue  # endpoint Discord removed from the docs; kept to flag SDK leftovers
                if item is None:
                    errors.append(f"{where}: key '{key}' does not exist in Discord docs at {docs.baseline['sha'][:7]}")
                    continue
                if item.deprecated and row.status not in ("Deprecated", "Excluded"):
                    warnings.append(f"{where}: Discord marks {key} as deprecated/disabled; consider status Deprecated")
                ev = sdk_evidence(item, sdk, bits)
                if ev is None:
                    continue
                if ev:
                    found_any = True
                else:
                    missing.append(key)
            auto = [k for k in row.keys if k in discord and sdk_evidence(discord[k], sdk, bits) is not None]
            if not auto:
                continue
            if row.status == "Implemented" and missing:
                errors.append(f"{where}: marked Implemented but SDK has no {', '.join(missing)}")
            elif row.status == "Partial" and not found_any:
                errors.append(f"{where}: marked Partial but SDK implements none of its keys")
            elif row.status == "Missing" and found_any:
                errors.append(f"{where}: marked Missing but SDK implements {', '.join(k for k in auto if k not in missing)}")

            for token in re.findall(r"`([^`]+)`", row.evidence):
                sym = token.rstrip("()").split("(")[0]
                if not re.fullmatch(r"[A-Z]\w*(?:\.\w+)*", sym) or re.fullmatch(r"[A-Z0-9_]+", sym):
                    continue  # not a C# symbol, or a Discord constant such as SOUNDBOARD_SOUNDS
                parts = sym.split(".")
                if parts[0] not in sdk.types:
                    errors.append(f"{where}: evidence type '{parts[0]}' not found in Src/")
                elif len(parts) > 1 and parts[-1] not in sdk.words:
                    errors.append(f"{where}: evidence member '{sym}' not found in Src/")

    for key, item in sorted(discord.items()):
        if key not in seen:
            errors.append(f"unmapped Discord item {key} ({item.page} — {item.title})")

    known_routes = {k for k in discord if not ":" in k.split(" ")[0]} | set(seen)
    for key, owners in sorted(sdk.routes.items()):
        if key not in known_routes:
            warnings.append(f"SDK route {key} ({', '.join(owners)}) matches no documented route")

    for w in warnings:
        print(f"warning: {w}")
    for e in errors:
        print(f"error: {e}")
    total = sum(len(p.rows) for p in prds)
    print(f"\n{len(discord)} Discord items, {len(sdk.routes)} SDK routes, {total} PRD rows in {len(prds)} PRDs — "
          f"{len(errors)} error(s), {len(warnings)} warning(s). Baseline {docs.baseline['repo']}@{docs.baseline['sha'][:7]}.")
    return 1 if errors else 0


def cmd_summary(args) -> int:
    baseline = json.loads(BASELINE_FILE.read_text(encoding="utf-8"))
    prds = read_prds()
    lines = ["| PRD | Area | Implemented | Partial | Missing | Excluded | Deprecated | Coverage |",
             "|---|---|---:|---:|---:|---:|---:|---:|"]
    totals = dict.fromkeys(STATUSES, 0)
    for prd in prds:
        counts = dict.fromkeys(STATUSES, 0)
        for row in prd.rows:
            if row.status in counts:
                counts[row.status] += 1
        for s in STATUSES:
            totals[s] += counts[s]
        lines.append(_summary_line(f"[{prd.number}](prd/{prd.path.name})", prd.title.split("—", 1)[-1].strip(), counts))
    lines.append(_summary_line("**Total**", "", totals, bold=True))
    lines.append("")
    lines.append(f"Coverage = Implemented ÷ (rows − Excluded − Deprecated); Partial is not counted. "
                 f"Discord baseline `{baseline['repo']}@{baseline['sha'][:7]}` ({baseline['date']}).")
    block = "\n".join(lines)
    readme = DOCS_DIR / "README.md"
    text = readme.read_text(encoding="utf-8")
    start, end = "<!-- coverage:start -->", "<!-- coverage:end -->"
    new = re.sub(re.escape(start) + r".*?" + re.escape(end), f"{start}\n{block}\n{end}", text, flags=re.S)
    readme.write_text(new, encoding="utf-8")
    print(block)
    return 0


def _summary_line(prd, area, c, bold=False):
    denom = sum(c.values()) - c["Excluded"] - c["Deprecated"]
    pct = f"{100 * c['Implemented'] / denom:.0f}%" if denom else "—"
    cells = [str(c[s]) for s in STATUSES] + [pct]
    if bold:
        cells = [f"**{x}**" for x in cells]
    return f"| {prd} | {area} | " + " | ".join(cells) + " |"


def cmd_scaffold(args) -> int:
    docs, pages, discord, sdk, bits = load(args)
    prefix = args.prefix or "XX"
    n = 0
    print("| ID | Capability | Discord key | Priority | Status | SDK evidence |")
    print("|---|---|---|---|---|---|")
    for key, item in discord.items():
        if item.page != args.page or (args.category and item.category != args.category):
            continue
        n += 1
        ev = sdk_evidence(item, sdk, bits)
        if item.deprecated:
            status = "Deprecated"
        elif ev is None:
            status = "Missing"
        else:
            status = "Implemented" if ev else "Missing"
        evidence = ", ".join(f"`{e}`" for e in sorted(set(ev or []))) or "—"
        print(f"| {prefix}-F{n:02d} | {item.title} | `{key}` | Must | {status} | {evidence} |")
    return 0


def cmd_inventory(args) -> int:
    docs, pages, discord, sdk, bits = load(args)
    print("# in-scope pages")
    for p in pages:
        print(" ", p)
    print("# discord items")
    for key, item in discord.items():
        ev = sdk_evidence(item, sdk, bits)
        mark = "?" if ev is None else ("+" if ev else "-")
        print(f"  {mark} {key:70} {item.page} — {item.title}")
    print("# sdk routes")
    for key, owners in sorted(sdk.routes.items()):
        print(f"  {key:70} {', '.join(sorted(set(owners)))}")
    return 0


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--docs-path", help="local checkout of discord/discord-api-docs (skips download)")
    parser.add_argument("--sha", help="override the baseline sha for this run")
    sub = parser.add_subparsers(dest="cmd", required=True)
    sub.add_parser("check")
    sub.add_parser("summary")
    sc = sub.add_parser("scaffold")
    sc.add_argument("page", help="docs page, e.g. resources/channel")
    sc.add_argument("--prefix", help="requirement ID prefix, e.g. CH")
    sc.add_argument("--category", help="only this category (route, event, perm, ...)")
    sub.add_parser("inventory")
    args = parser.parse_args()
    return {"check": cmd_check, "summary": cmd_summary, "scaffold": cmd_scaffold, "inventory": cmd_inventory}[args.cmd](args)


if __name__ == "__main__":
    sys.exit(main())
