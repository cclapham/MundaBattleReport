# Necromunda Narrative Battle Report Generator
## Project Plan & Specification

---

## Table of Contents

1. [Vision](#vision)
2. [Core Concept](#core-concept)
3. [Data Flow](#data-flow)
4. [Epic Structure](#epic-structure)
5. [Key Features by Phase](#key-features-by-phase)
6. [Technical Architecture](#technical-architecture)
7. [Data Model](#data-model)
8. [Critical Questions & Blockers](#critical-questions--blockers)
9. [Success Criteria](#success-criteria)

---

## Vision

Transform Necromunda game data into immersive, atmospheric battle narratives. Players feed in their gang from Munda Manager and log activation order + actions per round. The system generates cinematic battle reports that capture the drama, humor, and consequences of underhive warfare.

**Target outcome:** A player finishes a Necromunda game, pastes their Munda Manager link, logs the battle, and gets back a polished narrative they can share with their group.

---

## Core Concept

### What Makes This Different

- **Data source:** Munda Manager share links provide authoritative gang rosters (eliminating manual entry)
- **Input model:** Users only log what Munda Manager can't capture—activation order and actions per round
- **Output:** Prose-based battle reports that respect the mechanical reality while dramatizing the experience
- **Scope:** Live gang data + battle events → narrative

### Key Insight

Munda Manager becomes your source of truth. You don't need to replicate gang building; you just need to:
1. Fetch gang data from the share link
2. Combine it with battle events (user input)
3. Generate prose that weaves both together

---

## Data Flow

```
User provides Munda Manager share link
    ↓
System fetches + parses gang data
    ↓
User reviews imported gang (verify names, gear, skills)
    ↓
User enters battle metadata:
    - Scenario name / objective
    - Terrain / location flavor
    - Activation order per round
    - Actions taken per activation
    - Outcomes (hits/misses, injuries, OOA fighters)
    ↓
Narrative engine synthesizes:
    - Gang roster data (from Munda Manager)
    - Battle events (from user input)
    - Tone/customization preferences
    - Necromunda lore (house flavors, weapon descriptions, injury drama)
    ↓
Generate prose + export (HTML, docx, PDF)
    ↓
User shares report
```

---

## Epic Structure

### Epic 1: Munda Manager Integration

**Goal:** Safely fetch and parse gang data from Munda Manager share links.

**Stories:**

- **Story 1.1:** As a user, I want to paste a Munda Manager share link and have my gang auto-populated so I don't manually enter fighter stats
  - Acceptance criteria:
    - Share link input field accepts valid Munda Manager URLs
    - System fetches and displays gang data within 5 seconds
    - User sees a confirmation screen showing imported fighters, equipment, and skills
    - Clear error message if link is invalid or expired

- **Story 1.2:** As a user, I want the system to validate the link and gracefully handle errors so I get clear feedback
  - Acceptance criteria:
    - Dead/expired links show user-friendly error
    - Network timeouts show retry option
    - Malformed URLs show format guidance
    - System logs errors for debugging

- **Story 1.3:** As a user, I want gang roster data (names, equipment, skills, stats) extracted and cached so it's available for narrative generation
  - Acceptance criteria:
    - Fighter names, WS/BS/S/T/W/I/A/Ld are captured
    - Equipment list per fighter is complete
    - Skills/traits are preserved
    - Data is cached temporarily (session-based)

- **Story 1.4:** As a dev, I want a robust parser that handles Munda Manager's output format so it's resilient to minor format changes
  - Acceptance criteria:
    - Parser handles JSON, HTML, or CSV (whichever Munda Manager uses)
    - Graceful degradation if a field is missing
    - Unit tests cover major edge cases
    - Clear documentation on expected input format

- **Story 1.5:** As a user, I want to verify the imported gang looks correct before committing to a battle so I catch mistakes early
  - Acceptance criteria:
    - Imported gang displayed in a readable table/card format
    - Option to edit fighter names (add nicknames) before proceeding
    - Confirm button locks in the gang data for this battle

---

### Epic 2: Battle Input (Simplified)

**Goal:** Collect only the information Munda Manager doesn't provide—battle events and dynamics.

**Stories:**

- **Story 2.1:** As a user, I want to input activation order per round so the narrative reflects the actual turn sequence
  - Acceptance criteria:
    - Round-by-round UI (Round 1, Round 2, etc.)
    - For each round: drag-and-drop or select activation order
    - Fighter names auto-populated from imported gang
    - Save/edit per round
    - Visual indicator of activation sequence (1st, 2nd, 3rd, etc.)

- **Story 2.2:** As a user, I want to input actions taken per activation (move, shoot, charge, use skill) with optional outcomes so the system has the battle flow
  - Acceptance criteria:
    - Per-activation form with:
      - Action type (move, shoot, charge, use skill, overwatch, etc.)
      - Target/direction (optional for some actions)
      - Outcome (success, hit, miss, critical, injury table roll, etc.)
    - Short description box (optional) for narrative details
    - Repeat for each fighter's activation

- **Story 2.3:** As a user, I want to mark which fighters were engaged/injured/taken out of action so end states are clear
  - Acceptance criteria:
    - Fighter status tracker (active, engaged, pinned, injured, OOA, captured)
    - Option to note injury type (minor wound, serious injury, critical, etc.)
    - Battle summary showing final gang status
    - Casualty report auto-generated from marked fighters

- **Story 2.4:** As a user, I want optional battle context (scenario name, objective, terrain features, deployment zones) to flavor the narrative without being required
  - Acceptance criteria:
    - Optional metadata form:
      - Scenario name (e.g., "Turf War", "Hab Raid")
      - Objective description
      - Location/terrain (e.g., "Industrial Zone", "Spire Precinct")
      - Weather/conditions
    - These fields are optional; battle input works without them
    - Context is woven into opening paragraph of narrative

- **Story 2.5:** As a user, I want a UI that mirrors a typical turn order tracker so input feels natural to how we play
  - Acceptance criteria:
    - Layout matches mental model of Necromunda play
    - Compact but not cramped
    - Keyboard shortcuts for common actions (advance, mark OOA, etc.)
    - Option for "active vs. passive" view (just log the actions vs. replay step-by-step)

---

### Epic 3: Narrative Generation

**Goal:** Turn activation order + actions into atmospheric prose respecting Necromunda mechanics and lore.

**Stories:**

- **Story 3.1:** As a user, I want the narrative to respect actual activation order so the report reads like how the battle really unfolded
  - Acceptance criteria:
    - Report follows round-by-round structure matching user input
    - Activations presented in order
    - Turn transitions are clear (e.g., "Gang name activates their fighters")
    - No narrative reorganization for dramatic effect (accuracy first)

- **Story 3.2:** As a user, I want each fighter's full loadout and stats referenced in their actions so descriptions are accurate
  - Acceptance criteria:
    - Fighter names include nicknames if provided
    - Weapon names match Munda Manager data (e.g., "plasma gun" not generic "ranged weapon")
    - Stats inform flavor (Goliath vs. Escher vs. Cawdor get different descriptions)
    - Armor/equipment affects narrative impact (e.g., carapace armor mentioned in close calls)

- **Story 3.3:** As a user, I want momentum/swing moments highlighted so dramatic arcs emerge
  - Acceptance criteria:
    - Critical hits/misses get dramatic prose
    - Last fighter standing moment emphasized
    - Comebacks highlighted (gang down 3 fighters, comes back to win)
    - Victory/defeat clearly marked with impact
    - Injury moments (serious wound, taking OOA) given weight

- **Story 3.4:** As a user, I want different narrative tones available so I can match table culture
  - Acceptance criteria:
    - At least 3 tone options:
      - Gritty & Dark (noir underhive, consequences matter)
      - Over-the-Top Action (bombastic, cinematic)
      - Dark Comedy (absurdism, slapstick violence, ridiculous moments)
    - Tone affects word choice, sentence structure, dramatic emphasis
    - User selects tone before generation

- **Story 3.5:** As a user, I want Necromunda house-specific lore integrated so narratives feel authentic
  - Acceptance criteria:
    - Gang house identified from Munda Manager data
    - House flavor applied:
      - House Escher: ruthlessness, cunning, honor among sisters
      - House Goliath: brute strength, pride, tribal hierarchy
      - House Cawdor: faith, redemption, zealotry
      - House Delaque: intrigue, deception, shadows
      - House Van Saar: tech, innovation, pride in craftsmanship
      - Orlocks, Enforcers, etc.: appropriate flavor
    - Descriptions reflect house aesthetics and values
    - Weapons/equipment tied to house identity

---

### Epic 4: Output & Sharing

**Goal:** Export polished, shareable reports in multiple formats.

**Stories:**

- **Story 4.1:** As a user, I want formatted HTML exports so I can post them to a blog or Discord
  - Acceptance criteria:
    - HTML output is styled and readable
    - Includes CSS for printing
    - Works on mobile and desktop
    - Responsive layout
    - Option to share via link (if backend supports)

- **Story 4.2:** As a user, I want to export reports as formatted documents (docx, PDF) so I can share with my group
  - Acceptance criteria:
    - Docx export with proper formatting (headings, paragraphs, tables)
    - PDF export with professional layout
    - Both include battle metadata and gang rosters
    - File names include battle date and gang names

- **Story 4.3:** As a user, I want the gang rosters embedded in the report so readers see exact loadouts
  - Acceptance criteria:
    - Report includes a gang roster section per side
    - Table with: Fighter Name, Equipment, Skills, Status (at end of battle)
    - Optional: show casualties and injuries incurred
    - Matches Munda Manager data for accuracy

- **Story 4.4:** As a user, I want a battle summary at the top so context is clear
  - Acceptance criteria:
    - Summary includes:
      - Gang names and houses
      - Scenario name (if provided)
      - Location/terrain (if provided)
      - Date
      - Final outcome (victory, defeat, draw)
      - Casualty summary (gang A: 0 OOA, gang B: 2 OOA)
    - Summary is 1 paragraph or short table

---

### Epic 5: Campaign Tracking (Future)

**Goal:** Chain battles into narrative arcs; track gang evolution over time.

**Stories:**

- **Story 5.1:** As a user, I want to link successive battle reports so I can read a campaign timeline
  - Acceptance criteria:
    - Campaign view shows list of battles in chronological order
    - Each battle card shows gangs, date, outcome, key moments
    - Full battle reports linked from cards
    - Option to export full campaign as single document

- **Story 5.2:** As a user, I want injury outcomes to flow back into gang narrative so the living world evolves
  - Acceptance criteria:
    - At end of battle, user inputs injury table rolls for wounded fighters
    - System tracks injuries between battles (serious wound → recovery period → back to fighting)
    - Subsequent battle reports reference ongoing injuries (e.g., "fighter limping from last month's wound")
    - Campaign roster shows current injury status

- **Story 5.3:** As a user, I want cumulative stats to color how fighters are described
  - Acceptance criteria:
    - System tracks per-fighter lifetime stats:
      - Battles fought
      - Kills
      - Injuries sustained / suffered
      - Skills earned
      - Reputation
    - Descriptions in reports reflect veteran status (seasoned fighter vs. wet-behind-ears juve)
    - Running tally in campaign view

---

## Key Features by Phase

### Phase 1: MVP (True Minimum Viable Product)

**Focus:** Munda Manager integration + basic battle logging + prose generation

**Features:**
- ✅ Paste Munda Manager share link → import gang
- ✅ Verify imported gang
- ✅ Round-by-round activation order input
- ✅ Per-activation action logging (action type + outcome)
- ✅ Fighter status tracking (active/OOA)
- ✅ Basic prose generation (action → sentence mapping)
- ✅ HTML export
- ✅ Single narrative tone (gritty default)

**Not in MVP:**
- PDF/Docx export
- Campaign mode
- Multiple tones
- House-specific lore (basic only)
- Custom moment notes

**Deliverable:** A user can paste a gang link, log a complete battle, and export a readable HTML report.

---

### Phase 2: Polish & Customization

**Focus:** Multiple export formats, tone selection, personalization

**Features:**
- ✅ PDF and Docx export
- ✅ 3+ narrative tones (gritty, action, comedy)
- ✅ Custom notes per moment ("this was a revenge duel")
- ✅ Fighter nicknames / custom names
- ✅ Battle metadata (scenario, location, flavor)
- ✅ Gang roster section in report
- ✅ Battle summary header
- ✅ House-specific lore depth
- ✅ Improved prose variety (avoid repetition)

**Not in Phase 2:**
- Campaign mode
- Injury tracking across battles
- Advanced customization

**Deliverable:** Reports are professional, shareable, and personalized to the user's table.

---

### Phase 3: Campaign & Advanced Features

**Focus:** Multi-battle campaigns, gang evolution, deep customization

**Features:**
- ✅ Campaign mode (link multiple battles)
- ✅ Injury tracking between battles
- ✅ Gang evolution (reputation, veteran status)
- ✅ Advanced customization:
  - Narrative length (short/medium/epic)
  - Emphasis toggle (mechanics vs. drama)
  - Custom lore fragments
- ✅ Campaign export (full chronology as document)
- ✅ Leaderboards / gang stats
- ✅ Interactive battle map (if feasible)

**Not in Phase 3:**
- Multi-player simultaneous input
- Real-time battle logging (during game)

**Deliverable:** Users can track full campaigns with narrative continuity.

---

## Technical Architecture

### Technology Stack (Recommendations)

**Frontend:**
- React or Vue (dynamic form handling, real-time preview)
- TailwindCSS or similar (rapid styling)
- Axios or Fetch API (Munda Manager link fetching)

**Backend (if needed):**
- Node.js + Express or Python + FastAPI (Munda Manager parsing, caching)
- Optional: database (PostgreSQL/MongoDB) for campaign persistence
- Optional: Redis for session/cache management

**Export:**
- HTML: native (template strings or template engine)
- PDF: PDFKit or similar
- Docx: docx-js or similar

**Narrative Generation:**
- Option A: Template-based (fastest, most predictable)
  - Hand-written templates for action types
  - Variable substitution (fighter name, weapon, outcome)
  - Example: `"{Fighter} unleashed a burst from their {weapon} at {target}, the shot {outcome}."`
  
- Option B: AI-generated (more varied, needs guardrails)
  - Prompt an LLM (Claude, GPT-4, etc.) with:
    - Battle event (structured JSON)
    - Tone specification
    - House lore
    - Constraints (don't mention rules, stay accurate)
  - Cost: per-report API calls
  
- Option C: Hybrid (best of both)
  - Templates provide structure (action → sentence)
  - LLM adds flavor/variety (rewrite 1-2 sentences per action for tone)
  - Balances speed, accuracy, variety

**Recommendation:** Start with template-based (Phase 1), add hybrid AI (Phase 2) if needed for variety.

---

### Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                     Frontend (React/Vue)                     │
├─────────────────────────────────────────────────────────────┤
│  [Munda Manager Link Input] → [Gang Review] → [Battle Log]   │
│                           ↓                                   │
│                  [Tone & Settings] → [Preview/Export]        │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ↓
┌─────────────────────────────────────────────────────────────┐
│                    Backend API Layer                         │
├─────────────────────────────────────────────────────────────┤
│  [Munda Manager Parser] → [Cache/Store Gang Data]            │
│  [Battle Event Processor] → [Validate Input]                 │
│  [Narrative Generator] → [Template Engine + Optional LLM]    │
│  [Export Engine] → [HTML/PDF/Docx Serializer]               │
└────────────────────────┬────────────────────────────────────┘
                         │
                         ↓
┌─────────────────────────────────────────────────────────────┐
│                  Data & External Services                    │
├─────────────────────────────────────────────────────────────┤
│  [Munda Manager API/Web] [Session Cache] [Optional: DB]      │
│  [Optional: LLM API] [File Storage for Exports]              │
└─────────────────────────────────────────────────────────────┘
```

---

### Munda Manager Integration Options

**Option 1: Web Scraping (Fragile)**
- Scrape HTML from share link
- Parse gang data from DOM
- **Pros:** No Munda Manager cooperation needed
- **Cons:** Breaks if Munda Manager changes HTML structure

**Option 2: Reverse-Engineer API (Better)**
- Inspect network requests made by Munda Manager
- Call same API endpoints
- **Pros:** More reliable, less dependent on HTML format
- **Cons:** Terms of service / terms of use concerns

**Option 3: Public API (Best)**
- Check if Munda Manager exposes a public API
- Call documented endpoints
- **Pros:** Supported, reliable, explicit permission
- **Cons:** May not exist; may require API key

**Option 4: User Export (Most Reliable)**
- Ask user to export gang as JSON/CSV from Munda Manager
- Paste JSON into tool
- **Pros:** 100% reliable, no scraping needed
- **Cons:** Extra step for user, less seamless

**Recommendation:** Start by investigating Option 2 (reverse-engineer). If that's complex, fall back to Option 4 (user export). Aim for Option 3 if you can contact Munda Manager team.

---

## Data Model

### Gang Object

```json
{
  "gangId": "unique-id",
  "name": "Gang Name",
  "house": "Goliath|Escher|Cawdor|Delaque|VanSaar|Orlock|Enforcer|Other",
  "source": "Munda Manager share link",
  "importedAt": "2026-06-07T12:00:00Z",
  "fighters": [
    {
      "fighterId": "fighter-1",
      "name": "Fighter Name",
      "nickname": "Optional Nickname",
      "role": "Leader|Champion|Ganger|Juve|Specialist",
      "stats": {
        "WS": 4,
        "BS": 4,
        "S": 3,
        "T": 3,
        "W": 1,
        "I": 4,
        "A": 1,
        "Ld": 7
      },
      "equipment": [
        {
          "name": "Weapon Name",
          "type": "melee|ranged",
          "description": "Optional flavor"
        }
      ],
      "skills": ["Skill1", "Skill2"],
      "armor": "light|carapace|none",
      "injuries": "none|minor|serious|critical"
    }
  ]
}
```

### Battle Object

```json
{
  "battleId": "unique-id",
  "date": "2026-06-07",
  "scenario": "Turf War",
  "location": "Industrial Zone",
  "gangs": ["gang-1", "gang-2"],
  "rounds": [
    {
      "roundNumber": 1,
      "activations": [
        {
          "fighterName": "Fighter A",
          "gangId": "gang-1",
          "action": "shoot",
          "target": "Fighter B",
          "outcome": "hit",
          "notes": "Optional player notes"
        },
        {
          "fighterName": "Fighter B",
          "gangId": "gang-2",
          "action": "charge",
          "target": "Fighter A",
          "outcome": "success",
          "notes": ""
        }
      ]
    }
  ],
  "finalStatus": {
    "gang-1": {
      "outcome": "victory",
      "casualties": 0,
      "fighters": {
        "Fighter A": "active",
        "Fighter C": "OOA"
      }
    },
    "gang-2": {
      "outcome": "defeat",
      "casualties": 2,
      "fighters": {
        "Fighter B": "serious-injury",
        "Fighter D": "OOA"
      }
    }
  },
  "narrativeTone": "gritty|action|comedy",
  "customNotes": []
}
```

### Narrative Template (Pseudo-Code)

```
BATTLE REPORT: [Gang1] vs [Gang2]
Scenario: [Scenario Name]
Location: [Location Description]
---

[OPENING PARAGRAPH - Set atmosphere based on scenario/location]

## Round 1

Gang [GangName] activates their fighters.

[For each activation in order:]
  [Fighter Name] [ACTION_TEMPLATE] 
  - If outcome is notable: [DRAMATIC_CONSEQUENCE]

Gang [GangName] activates their fighters.

[Same format]

## Round 2
[Repeat structure]

---

## Battle Summary

[Final casualty count]
[Victory/Defeat statement]
[Most dramatic moment]

## Rosters

[Gang 1 Roster - table with final status]
[Gang 2 Roster - table with final status]
```

---

## Critical Questions & Blockers

### 1. Munda Manager Integration

**Blocker:** How do you access Munda Manager gang data?

- [ ] Is there a public API?
- [ ] Can you scrape share links successfully?
- [ ] Would the Munda Manager team cooperate on API access?
- [ ] Should you ask users to export as JSON instead?

**Action Required:** Investigate Munda Manager's technical architecture before building.

---

### 2. Narrative Quality

**Question:** Template-based or AI-generated?

- **Template approach:** Fast, predictable, accurate, but can feel repetitive
- **AI approach:** More varied and creative, but costs money per report and needs careful guardrails to avoid hallucinating rules
- **Hybrid approach:** Templates + light AI rewriting (probably the sweet spot)

**Action Required:** Build a small prototype of each and see which feels best.

---

### 3. User Input Complexity

**Question:** How much friction is acceptable?

- **Low friction:** Dropdown menus, auto-complete, minimal typing
- **High accuracy:** Users manually type everything so nothing is missed

**Action Required:** User testing with actual Necromunda players to find the right balance.

---

### 4. Scope: Multiple Editions?

**Question:** Support only Necromunda 2E, or also older editions?

- 2E is current; it's the obvious choice
- But older editions have passionate players
- Rules differ significantly (doesn't affect this tool much, but affects flavor text)

**Action Required:** Decide upfront. Starting with 2E only is sensible.

---

### 5. Scope: Multiple Game Modes?

**Question:** Just gang-vs-gang battles, or scenarios (objective missions, PvE, etc.)?

- Gang vs. Gang is the core mode
- Scenarios add flavor but complicate input
- Start with gang vs. gang; add scenarios in Phase 2 if demand is high

**Action Required:** Validate with community; prioritize the most common play pattern.

---

### 6. Backend vs. Frontend-Only

**Question:** Can this be a purely client-side tool, or does it need a backend?

- **Client-side only:** Simpler, no server costs, but limited
  - Can do HTML export easily
  - PDF/Docx export requires browser libraries (possible but bulky)
  - No campaign persistence
  - No caching of Munda Manager data
  
- **Backend:** More complex, but more features
  - Handles Munda Manager scraping/caching
  - Generates PDF/Docx server-side (cleaner)
  - Can persist campaigns to DB
  - Can optionally add LLM integration

**Action Required:** Decide based on desired Phase 1 scope. MVP could be client-side only; Phase 2+ needs backend.

---

### 7. Licensing & Legal

**Question:** Munda Manager & Games Workshop IP

- Munda Manager is a fan project (not affiliated with GW)
- Your tool would be a fan project too
- GW is generally tolerant of fan tools but keeps ultimate control
- **Action Required:** Consider adding a disclaimer that this is a fan tool and include appropriate GW copyright notices.

---

## Success Criteria

### MVP (Phase 1) Success

- [ ] User can paste Munda Manager link and import a gang in < 30 seconds
- [ ] Imported gang matches Munda Manager exactly (spot check equipment, skills)
- [ ] User can log a complete 3-round battle in < 15 minutes
- [ ] Generated report reads coherently (no grammar errors, logical flow)
- [ ] Report matches actual battle events (activation order, outcomes, casualties)
- [ ] HTML export works and looks readable on mobile + desktop
- [ ] At least 1 playtester successfully uses tool end-to-end

---

### Phase 2 Success

- [ ] PDF and Docx exports work and look professional
- [ ] User can select from 3 narrative tones and sees visible difference in prose
- [ ] House-specific lore is evident in descriptions
- [ ] Casualty tracking is accurate
- [ ] At least 5 different playtests with different gangs (Goliath, Escher, Cawdor, etc.)

---

### Phase 3 Success

- [ ] Users can link multiple battles into a campaign view
- [ ] Injuries persist between battles
- [ ] Fighter nickname/reputation is woven into narrative across battles
- [ ] Campaign export as single document works
- [ ] Users report sharing reports with their gaming groups regularly

---

## Next Steps

1. **Investigate Munda Manager:**
   - Can you access gang data from share links?
   - What format is it in (JSON, HTML, API)?
   - Estimate effort to build parser

2. **Prototype Input UX:**
   - Sketch the battle logging form
   - Get feedback from 1-2 Necromunda players
   - Refine interaction model

3. **Prototype Narrative Generation:**
   - Build 5-10 template sentences for common actions (move, shoot, charge)
   - Test with sample battle data
   - Evaluate quality and variety

4. **Define Phase 1 Scope Precisely:**
   - Lock in feature list
   - Estimate engineering effort
   - Decide on tech stack

5. **Set Up Project:**
   - Choose version control (GitHub)
   - Set up build/deploy pipeline (if backend needed)
   - Create task board (Trello, Jira, GitHub Issues)

---

## Appendix: Necromunda Lore Flavor by House

### House Escher
- **Aesthetics:** Elegant, ruthless, matriarchal
- **Language:** "The Queens of the Underhive", cunning, tactical
- **Weapon flavor:** Precise, elegant
- **Example narrative:** "With the precision of a predator, the Escher warrior's plasma pistol sang out..."

### House Goliath
- **Aesthetics:** Brutal, proud, tribal
- **Language:** "Brute strength", "genetic gift", honor among brothers
- **Weapon flavor:** Heavy, devastating, crude
- **Example narrative:** "The massive Goliath warrior's auto-cannon roared, leaving nothing but scorched armor in its wake..."

### House Cawdor
- **Aesthetics:** Zealous, desperate faith, self-flagellation
- **Language:** "Righteous", "blessed", "redemption", "heresy"
- **Weapon flavor:** Improvised, jury-rigged, religious zeal
- **Example narrative:** "The Cawdor devotee chanted prayers as flames from their flamer engulfed the enemies of faith..."

### House Delaque
- **Aesthetics:** Mysterious, intrigue, deception
- **Language:** "Shadows", "whispered", "conspiracy", "secrets"
- **Weapon flavor:** Subtle, poisons, deception
- **Example narrative:** "In the darkness, the Delaque assassin moved unseen, their poisoned blade glinting briefly before finding its mark..."

### House Van Saar
- **Aesthetics:** Technical, superior, innovative
- **Language:** "Cutting-edge", "precision engineering", "superior doctrine"
- **Weapon flavor:** Advanced, reliable, tech-forward
- **Example narrative:** "The Van Saar champion's plasma rifle, a marvel of 3D-printed engineering, flared with barely contained energy..."

### Orlocks
- **Aesthetics:** Rough, industrial, mercenary
- **Language:** "Earned", "grit", "hard work", mercenary ethos
- **Weapon flavor:** Practical, heavy, industrial
- **Example narrative:** "The seasoned Orlock mercenary braced their heavy stubber and let loose a withering hail of fire..."

---

## Document History

- **v1.0** (2026-06-07): Initial project plan created with Munda Manager integration focus
