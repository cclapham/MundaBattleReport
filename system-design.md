# Necromunda Battle Report Generator — System Design

> Companion to `necromunda_battle_report_plan.md`. Resolves the open blockers, makes concrete decisions, and provides a build-ready architecture.

---

## Decisions Summary

| Question | Decision | Rationale |
|---|---|---|
| Munda Manager integration | API (Option 3) first, User Export (Option 4) as fallback | MM has a public API page — investigate it first |
| Frontend + Backend | Blazor Server (.NET) | Rich interactive components in C#; server-side model eliminates CORS concerns |
| Narrative generation | Claude API (AI) from Phase 1 | Cost is pennies per report; templates feel repetitive |
| Tech stack | .NET 9 + Blazor Server + EF Core + PostgreSQL | C# throughout; EF Core handles DB; Blazor's SignalR connection suits stateful battle logger |
| Edition scope | Necromunda 2E only | Focus; add 1E/Ash Wastes later if demanded |
| Game modes | Gang vs. Gang only in Phase 1 | Covers 90% of games; scenarios are optional metadata |

---

## Munda Manager Integration (Blocker Resolved)

### What We Know

- MM is open source: [github.com/joeseos/mundamanager](https://github.com/joeseos/mundamanager)
- Tech stack: Next.js 15 + Supabase (PostgreSQL) + TypeScript
- They have a public `/api-access` page (requires login to read)
- The gang data schema is fully documented in their README

### Fighter Data Shape (from MM source, mapped to C#)

```csharp
public class MundaFighter
{
    public string Id { get; set; } = "";
    public string FighterName { get; set; } = "";
    public string FighterType { get; set; } = "";   // Leader, Champion, Ganger, Juve
    public string FighterClass { get; set; } = "";  // House-specific class name
    public int Credits { get; set; }

    // Stats
    public int Movement { get; set; }
    public int WeaponSkill { get; set; }
    public int BallisticSkill { get; set; }
    public int Strength { get; set; }
    public int Toughness { get; set; }
    public int Wounds { get; set; }
    public int Initiative { get; set; }
    public int Attacks { get; set; }
    public int Leadership { get; set; }
    public int Cool { get; set; }
    public int Willpower { get; set; }
    public int Intelligence { get; set; }

    public List<MundaWeapon> Weapons { get; set; } = [];
    public List<MundaWargear> Wargear { get; set; } = [];
    public MundaEffects Effects { get; set; } = new();
}

public class MundaWeapon
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = ""; // melee | ranged
}
```

### Integration Strategy

**Step 1 (Do first):** Sign into Munda Manager and read `/api-access`. It likely provides API key generation and documented endpoints.

**Step 2:** Check if share links return public JSON. Supabase apps typically allow `SELECT` on shared resources without auth — inspect the network response when opening a share link.

**Step 3 (if public API exists):** Call the MM API from your ASP.NET Core backend using `HttpClient`. No CORS issue since this is server-side C#.

**Fallback (if no public API):** Use Option 4 — user exports their gang as JSON from MM. MM is open source, so you know the exact JSON schema. Deserialize against `MundaFighter` above.

**Do not build a scraper.** MM is a fan project; fragile scraping isn't worth it when a real API likely exists.

---

## Architecture

### Tech Stack

```
Framework:       .NET 9 — Blazor Server
UI components:   MudBlazor or Radzen (rich component libraries with good tables, forms, drag-drop)
ORM:             Entity Framework Core 9
Database:        PostgreSQL (local dev via Docker; Railway or Supabase Postgres for production)
Auth:            ASP.NET Core Identity (optional Phase 1, needed Phase 3)
Narrative:       Anthropic Claude API via HttpClient (claude-haiku-4-5)
PDF export:      QuestPDF (excellent .NET library, free for small projects)
Docx export:     DocumentFormat.OpenXml (OpenXML SDK)
Hosting:         Azure App Service, Railway, or Fly.io (Blazor Server needs a persistent connection)
```

**Why Blazor Server suits this project:**
- The battle logger is a stateful, interactive multi-step form — Blazor's component model handles this naturally in C#
- Server-side execution means `HttpClient` calls to MM API and Claude API never expose keys to the browser
- SignalR's persistent connection gives you real-time UI updates (spinner while narrative generates, streaming output)
- No CORS concerns anywhere — all external calls originate from the server
- QuestPDF and OpenXML are best-in-class .NET libraries; PDF/Docx generation in C# is significantly easier than in JS

**Hosting note:** Blazor Server requires a persistent WebSocket (SignalR). Vercel and similar serverless hosts don't work. Use Azure App Service (B1 plan ~$13/month), Railway, or Fly.io. All support .NET 9.

### System Diagram

```
┌─────────────────────────────────────────────────────────────┐
│              Browser — Blazor Server Components              │
│         (HTML rendered server-side, state in memory)         │
│                                                              │
│  [1. Paste MM Link / Upload JSON]                            │
│       ↓                                                      │
│  [2. Gang Review — confirm fighters, add nicknames]          │
│       ↓                                                      │
│  [3. Battle Metadata — scenario, location, tone]             │
│       ↓                                                      │
│  [4. Battle Logger — round/activation/action input]          │
│       ↓                                                      │
│  [5. Preview Narrative — streamed from Claude]               │
│       ↓                                                      │
│  [6. Export — HTML / PDF / Docx download]                    │
└──────────────────────┬──────────────────────────────────────┘
                       │ SignalR (Blazor Server circuit)
                       ↓
┌─────────────────────────────────────────────────────────────┐
│              ASP.NET Core + Blazor Server                    │
│                                                              │
│  GangImportService    — fetch + deserialize MM gang data     │
│  BattleService        — persist/retrieve battles via EF Core │
│  NarrativeService     — build prompt, call Claude API        │
│  ExportService        — render PDF (QuestPDF) / Docx         │
└──────────────────┬──────────────────────────────────────────┘
                   │
        ┌──────────┴──────────┐
        ↓                     ↓
┌──────────────┐    ┌────────────────────────┐
│ Munda Manager│    │   PostgreSQL            │
│ API / JSON   │    │   (EF Core)             │
│              │    │                         │
│  Gang data   │    │  gangs                  │
│  (read-only) │    │  battles                │
└──────────────┘    │  (Phase 3: campaigns)   │
                    └────────────────────────┘
                              │
                    ┌─────────┴────────┐
                    ↓                  
             ┌──────────────┐      
             │ Claude API   │      
             │ HttpClient   │      
             │ (narrative)  │      
             └──────────────┘
```

### Component Structure

```
Pages/
  Index.razor              — landing / gang import
  GangReview.razor         — review imported gang, add nicknames
  BattleSetup.razor        — metadata form (scenario, tone, etc.)
  BattleLogger.razor       — round-by-round activation entry
  NarrativePreview.razor   — rendered report, export buttons

Components/
  FighterCard.razor        — single fighter display (stats, weapons)
  FighterTable.razor       — gang roster table
  RoundPanel.razor         — one round's activations
  ActivationRow.razor      — single activation entry row
  FighterStatusBadge.razor — Active / Pinned / OOA chip
  NarrativeViewer.razor    — markdown renderer for prose output
  ExportToolbar.razor      — HTML / PDF / Docx download buttons

Services/
  GangImportService.cs     — MM API calls + JSON deserialization
  BattleService.cs         — EF Core CRUD for battles
  NarrativeService.cs      — Claude API integration
  ExportService.cs         — QuestPDF + OpenXML generation

Models/
  Gang.cs                  — EF Core entity
  Battle.cs                — EF Core entity
  Round.cs / Activation.cs — JSONB value objects (stored as JSON column)
  MundaFighter.cs          — deserialization target for MM data
```

---

## Data Model

### EF Core Entities

```csharp
public class Gang
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string House { get; set; } = "";
    public string? MmSource { get; set; }        // share link or null
    public string RawData { get; set; } = "[]";  // MM JSON, stored as text/jsonb
    public DateTime ImportedAt { get; set; } = DateTime.UtcNow;
    
    // Deserialized on demand — not a DB column
    [NotMapped]
    public List<MundaFighter> Fighters =>
        JsonSerializer.Deserialize<List<MundaFighter>>(RawData) ?? [];
}

public class Battle
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public string? Scenario { get; set; }
    public string? Location { get; set; }
    public string? Conditions { get; set; }
    public NarrativeTone Tone { get; set; } = NarrativeTone.Gritty;

    public Guid Gang1Id { get; set; }
    public Gang Gang1 { get; set; } = null!;
    public Guid Gang2Id { get; set; }
    public Gang Gang2 { get; set; } = null!;

    public BattleOutcome? Outcome { get; set; }

    // Stored as JSON columns (EF Core 8+ supports this natively)
    public List<Round> Rounds { get; set; } = [];
    public Dictionary<string, FighterStatus> FinalStatus { get; set; } = [];
    
    public string? GeneratedNarrative { get; set; }  // cached prose
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum NarrativeTone { Gritty, Action, Comedy }
public enum BattleOutcome { Gang1Wins, Gang2Wins, Draw }
public enum FighterStatus { Active, Pinned, SeriousInjury, OutOfAction }
```

### Round / Activation Value Objects

```csharp
public class Round
{
    public int RoundNumber { get; set; }
    public List<Activation> Activations { get; set; } = [];
}

public class Activation
{
    public string FighterId { get; set; } = "";
    public string FighterName { get; set; } = "";  // denormalized
    public Guid GangId { get; set; }
    public ActionType Action { get; set; }
    public string? Target { get; set; }
    public OutcomeType Outcome { get; set; }
    public string? InjuryRoll { get; set; }  // e.g. "Humiliated"
    public string? Notes { get; set; }
}

public enum ActionType { Move, Shoot, Charge, UseSkill, Overwatch, Recover, Other }
public enum OutcomeType { Success, Failure, Hit, Miss, Critical, Injury, OutOfAction }
```

### PostgreSQL schema (EF Core migrations will generate this)

```sql
CREATE TABLE gangs (
  id           UUID PRIMARY KEY,
  name         TEXT NOT NULL,
  house        TEXT NOT NULL,
  mm_source    TEXT,
  raw_data     JSONB NOT NULL DEFAULT '[]',
  imported_at  TIMESTAMPTZ NOT NULL
);

CREATE TABLE battles (
  id                  UUID PRIMARY KEY,
  date                DATE NOT NULL,
  scenario            TEXT,
  location            TEXT,
  conditions          TEXT,
  tone                TEXT NOT NULL DEFAULT 'Gritty',
  gang_1_id           UUID REFERENCES gangs(id),
  gang_2_id           UUID REFERENCES gangs(id),
  outcome             TEXT,
  rounds              JSONB NOT NULL DEFAULT '[]',
  final_status        JSONB NOT NULL DEFAULT '{}',
  generated_narrative TEXT,
  created_at          TIMESTAMPTZ NOT NULL
);
```

---

## Narrative Engine

### Claude API Integration (C#)

```csharp
public class NarrativeService(HttpClient http, IConfiguration config)
{
    public async IAsyncEnumerable<string> GenerateStreamingAsync(Battle battle)
    {
        var prompt = BuildPrompt(battle);

        var request = new
        {
            model = "claude-haiku-4-5-20251001",
            max_tokens = 4096,
            stream = true,
            system = BuildSystemPrompt(battle.Gang1.House, battle.Gang2.House, battle.Tone),
            messages = new[] { new { role = "user", content = prompt } }
        };

        var response = await http.PostAsJsonAsync("https://api.anthropic.com/v1/messages", request);
        // parse SSE stream, yield text deltas
        await foreach (var chunk in ParseSseStream(response))
            yield return chunk;
    }

    private string BuildPrompt(Battle battle)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Gang 1: {battle.Gang1.Name} ({battle.Gang1.House})");
        sb.AppendLine($"Gang 2: {battle.Gang2.Name} ({battle.Gang2.House})");
        if (battle.Scenario != null) sb.AppendLine($"Scenario: {battle.Scenario}");
        if (battle.Location != null) sb.AppendLine($"Location: {battle.Location}");
        sb.AppendLine();
        sb.AppendLine("ROUNDS:");

        foreach (var round in battle.Rounds)
        {
            sb.AppendLine($"Round {round.RoundNumber}:");
            foreach (var a in round.Activations)
            {
                var gang = a.GangId == battle.Gang1Id ? battle.Gang1 : battle.Gang2;
                var fighter = gang.Fighters.FirstOrDefault(f => f.Id == a.FighterId);
                var weapons = fighter?.Weapons.Select(w => w.Name).StringJoin(", ") ?? "unknown";
                var target = a.Target != null ? $" → {a.Target}" : "";
                var outcome = a.InjuryRoll != null ? $"{a.Outcome} ({a.InjuryRoll})" : a.Outcome.ToString();
                var notes = a.Notes != null ? $" [Note: {a.Notes}]" : "";
                sb.AppendLine($"  [{gang.Name}] {a.FighterName} ({fighter?.FighterType}, {weapons}): {a.Action}{target} → {outcome}{notes}");
            }
        }

        // final status
        sb.AppendLine("\nFINAL STATUS:");
        foreach (var (name, status) in battle.FinalStatus)
            sb.AppendLine($"  {name}: {status}");
        sb.AppendLine($"Outcome: {battle.Outcome}");

        return sb.ToString();
    }
}
```

### Streaming to Blazor Component

```razor
@* NarrativePreview.razor *@
<div class="narrative-output">
    @((MarkupString)_renderedMarkdown)
</div>

@code {
    private string _rawMarkdown = "";
    private string _renderedMarkdown = "";

    private async Task GenerateNarrative()
    {
        _rawMarkdown = "";
        await foreach (var chunk in NarrativeService.GenerateStreamingAsync(CurrentBattle))
        {
            _rawMarkdown += chunk;
            _renderedMarkdown = Markdig.Markdown.ToHtml(_rawMarkdown);
            StateHasChanged();  // Blazor re-renders incrementally via SignalR
        }
        // Cache the completed narrative
        await BattleService.SaveNarrativeAsync(CurrentBattle.Id, _rawMarkdown);
    }
}
```

Streaming works particularly well in Blazor Server because SignalR pushes each chunk to the browser immediately — the report writes itself out in real time.

---

## Export

### PDF with QuestPDF

```csharp
public class ExportService
{
    public byte[] GeneratePdf(Battle battle, string narrative)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);

                page.Header().Text($"{battle.Gang1.Name} vs {battle.Gang2.Name}")
                    .FontSize(18).Bold();

                page.Content().Column(col =>
                {
                    col.Item().Text($"Scenario: {battle.Scenario ?? "Unknown"}");
                    col.Item().Text($"Date: {battle.Date}");
                    col.Item().Text($"Outcome: {battle.Outcome}");
                    col.Item().PaddingTop(10).Text(narrative);  // prose block

                    // Gang rosters
                    foreach (var gang in new[] { battle.Gang1, battle.Gang2 })
                    {
                        col.Item().PaddingTop(20).Text(gang.Name).Bold();
                        col.Item().Table(t =>
                        {
                            t.ColumnsDefinition(c => { c.RelativeColumn(3); c.RelativeColumn(5); c.RelativeColumn(4); });
                            t.Header(h => { /* Name, Weapons, Status */ });
                            foreach (var f in gang.Fighters)
                                t.Cell().Text(f.FighterName); // etc.
                        });
                    }
                });
            });
        });

        return document.GeneratePdf();
    }
}
```

### Docx with OpenXML

Use the `DocumentFormat.OpenXml` NuGet package. For a narrative-first document, structure it as: title → summary table → prose (one paragraph per round section) → roster tables. The OpenXML SDK is verbose but predictable — wrap it in a `DocxBuilder` helper class.

---

## Phase 1 Build Plan (MVP)

### Week 1: Scaffold + Gang Import

1. `dotnet new blazorserver -n MundaBattleRep` + add MudBlazor + EF Core + Npgsql packages
2. Set up PostgreSQL (Docker locally: `docker run -e POSTGRES_PASSWORD=dev -p 5432:5432 postgres`)
3. Define EF Core models + run first migration
4. Build `GangImportService` — deserialize MM JSON against `MundaFighter`
5. `Index.razor` — file upload or JSON paste field
6. `GangReview.razor` — fighter table with nickname edit, confirm button

**Milestone:** Upload JSON → see gang on screen ✓

### Week 2: Battle Logger

7. `BattleSetup.razor` — metadata form (optional scenario, location, tone selector)
8. `BattleLogger.razor` — core component:
   - Tab per round (MudTabs)
   - `ActivationRow.razor` per activation with: fighter select (MudSelect) → action (MudSelect) → target (text) → outcome (MudSelect) → notes (optional text)
   - Fighter status chips updated as outcomes are entered
9. Save battle to PostgreSQL via `BattleService`

**Milestone:** Log a complete battle ✓

### Week 3: Narrative + Export

10. `NarrativeService` — prompt assembly + Claude API streaming
11. `NarrativePreview.razor` — streaming render with Markdig
12. HTML export — render the preview as a self-contained HTML file (inject CSS inline)
13. PDF export via QuestPDF, download as `{gang1}-vs-{gang2}-{date}.pdf`

**Milestone:** Full end-to-end: import → log → narrative → export ✓

---

## Key Trade-offs

### Persistence: EF Core + PostgreSQL vs. In-Memory for Phase 1

**Option A: EF Core + PostgreSQL from day 1** (recommended)
- Required for Phase 3 campaign mode anyway
- Docker makes local Postgres trivial
- Railway free tier ($5 credit/month) covers a small hobby project

**Option B: In-memory / JSON file for Phase 1**
- Faster to prototype, no DB setup
- Dead end — you'll rewrite it for Phase 2

**Recommendation:** Start with Postgres. The EF Core setup takes one afternoon and pays off immediately.

### Hosting

| Option | Cost | Notes |
|---|---|---|
| Azure App Service B1 | ~$13/month | First-party .NET support, easy deploy from VS |
| Railway | Free → $5/month | Simple, supports .NET + Postgres together |
| Fly.io | Free tier available | Good for Docker-based deploy |
| Self-hosted VPS | ~$5/month | Full control; needs reverse proxy setup |

Blazor Server **cannot** run on Vercel, Netlify, or other serverless hosts — it needs a persistent process for SignalR. Any of the options above work fine.

### PDF: QuestPDF vs. Playwright (headless Chrome)

| Approach | Pros | Cons |
|---|---|---|
| QuestPDF | Pure .NET, no Chromium, fast, great docs | You lay out the PDF in C# (more work than HTML) |
| Playwright | Renders your actual Blazor HTML → perfect fidelity | Requires Chromium binary (~150MB), slower cold start |

**Recommendation:** QuestPDF for Phase 2. You have full C# control, it's fast, and the output is professional. Playwright is overkill for a hobby project.

---

## What to Do First (Next Steps, Ordered)

1. **Log into Munda Manager, go to `/api-access`.** 30 minutes before any code. Determines whether you get a clean API or fall back to JSON upload.

2. **Test a share link manually.** Open a MM share link and inspect the network tab — does it return JSON? If yes, your `HttpClient` call is straightforward.

3. **Scaffold the project.** `dotnet new blazorserver -n MundaBattleRep`, add MudBlazor, EF Core, Npgsql. Get it running.

4. **Build gang import with the JSON fallback first.** Don't wait on MM API. Take the `FighterProps` schema from the MM README, write `MundaFighter.cs`, and deserialize a real gang JSON export. This gets you to a working UI today.

5. **Write one sample prompt by hand and call Claude.** Before building the battle logger UI, take a battle you played, format it as the prompt structure above, and call the Claude API manually. Validate the output quality before investing in the input UX.

---

## Risks

| Risk | Likelihood | Mitigation |
|---|---|---|
| MM has no public API | Medium | JSON upload fallback is fine for a hobbyist tool |
| MM changes their schema | Low | `MundaFighter` deserialization is isolated; MM is open source so you'll see changes coming |
| Claude hallucinates game events | Low | Prompt explicitly forbids invention; structured input leaves little room |
| Blazor Server latency on cheap hosting | Low | Narrative streaming tolerates a few hundred ms; rest of the app is fast |
| GW IP concerns | Low | Fan tool disclaimer; don't monetize |

---

## What's Not Designed Here (Deferred to Phase 2/3)

- ASP.NET Core Identity (user accounts, login)
- Shareable report links
- Campaign mode and battle linking
- Injury persistence between battles
- Fighter veteran stat tracking
- Interactive battle map

---

*Design version: 1.1 — June 2026 (updated: Next.js → Blazor Server .NET 9)*
