# Necromunda Battle Report Generator - Task List

## Project Setup & Investigation

### Priority: Critical (Blockers)

- [ ] **Investigate Munda Manager Integration**
  - Task ID: SETUP-001
  - Description: Determine how to access gang data from Munda Manager share links
  - Subtasks:
    - [ ] Check if Munda Manager has public API documentation
    - [ ] Reverse-engineer share link format (inspect network requests)
    - [ ] Test scraping approach on sample share link
    - [ ] Evaluate feasibility of each integration option
  - Estimated effort: 2-4 hours
  - Blocker for: All narrative generation work

- [ ] **Decide Tech Stack**
  - Task ID: SETUP-002
  - Description: Choose between client-side only, Node.js backend, Python backend
  - Subtasks:
    - [ ] Evaluate PDF/Docx export library options for chosen stack
    - [ ] Check cost implications (API calls, server hosting)
    - [ ] Prototype quick POC in 1-2 frameworks
  - Estimated effort: 3-6 hours
  - Blocker for: Backend development

- [ ] **Decide Narrative Approach**
  - Task ID: SETUP-003
  - Description: Choose between template-based, AI-generated, or hybrid prose generation
  - Subtasks:
    - [ ] Write 5-10 sample templates for common actions (shoot, move, charge)
    - [ ] Test 1-2 templates with sample battle data
    - [ ] Estimate effort for each approach
    - [ ] Get feedback from 1-2 Necromunda players
  - Estimated effort: 4-8 hours
  - Blocker for: Phase 1 narrative generation

- [ ] **Scope: Edition & Modes**
  - Task ID: SETUP-004
  - Description: Lock in support for Necromunda 2E only; decide on scenario support
  - Subtasks:
    - [ ] Confirm 2E is the priority
    - [ ] Document which scenarios to support in Phase 1 (if any)
    - [ ] Create list of common actions/outcomes for Phase 1
  - Estimated effort: 1-2 hours
  - Blocker for: Content creation

---

## Phase 1: MVP - Core Functionality

### Priority: Critical

- [ ] **Build Munda Manager Parser**
  - Task ID: PHASE1-001
  - Description: Fetch and parse gang data from Munda Manager share links
  - Subtasks:
    - [ ] Implement link validation
    - [ ] Build parser for chosen data format (JSON/HTML/API)
    - [ ] Handle errors gracefully (dead links, network timeouts)
    - [ ] Add session-based caching
    - [ ] Write unit tests for parser
  - Estimated effort: 6-10 hours
  - Dependencies: SETUP-001

- [ ] **Create Gang Review Screen**
  - Task ID: PHASE1-002
  - Description: Display imported gang data for user verification
  - Subtasks:
    - [ ] Design readable table/card layout for fighters
    - [ ] Show all relevant stats: name, equipment, skills, armor
    - [ ] Add option to edit fighter nicknames
    - [ ] Implement "Confirm" button to lock in gang data
    - [ ] Add error states if import failed
  - Estimated effort: 3-5 hours
  - Dependencies: PHASE1-001

- [ ] **Build Battle Metadata Input Form**
  - Task ID: PHASE1-003
  - Description: Collect optional battle context (scenario, location, terrain)
  - Subtasks:
    - [ ] Design form layout
    - [ ] Add fields: Scenario name, Location, Objective, Notes
    - [ ] Make all fields optional (MVPs are minimal)
    - [ ] Validate inputs
  - Estimated effort: 2-3 hours
  - Dependencies: None

- [ ] **Build Round-by-Round Activation Input UI**
  - Task ID: PHASE1-004
  - Description: Interface for logging activation order and actions per round
  - Subtasks:
    - [ ] Design round structure (Round 1, Round 2, etc.)
    - [ ] Build activation order selector (drag-drop or select)
    - [ ] Create action input form (action type, target, outcome, notes)
    - [ ] Implement per-activation save/edit
    - [ ] Add visual activation sequence indicators
    - [ ] Test UX with sample data
    - [ ] Get feedback from 1-2 Necromunda players
  - Estimated effort: 8-12 hours
  - Dependencies: PHASE1-002

- [ ] **Build Fighter Status Tracker**
  - Task ID: PHASE1-005
  - Description: Track fighter status (active, OOA, injured, engaged) throughout battle
  - Subtasks:
    - [ ] Add status dropdown per fighter per round
    - [ ] Implement injury type input (if applicable)
    - [ ] Auto-generate casualty count at end
    - [ ] Visual indicators for OOA/injured fighters
  - Estimated effort: 3-5 hours
  - Dependencies: PHASE1-004

- [ ] **Build Narrative Template Engine**
  - Task ID: PHASE1-006
  - Description: Convert actions into prose using templates
  - Subtasks:
    - [ ] Create template library for action types (shoot, move, charge, skill, overwatch)
    - [ ] Implement variable substitution (fighter name, weapon, outcome, target)
    - [ ] Add house-specific flavor templates
    - [ ] Handle edge cases (missed shots, critical hits, injuries)
    - [ ] Write and test 20-30 templates
    - [ ] Validate accuracy against Necromunda 2E rules
  - Estimated effort: 10-15 hours
  - Dependencies: SETUP-003

- [ ] **Implement Round & Opening/Closing Prose**
  - Task ID: PHASE1-007
  - Description: Generate narrative structure (opening, per-round, summary, closing)
  - Subtasks:
    - [ ] Write opening paragraph template (sets scenario/location/atmosphere)
    - [ ] Build round header template ("Gang X activates their fighters")
    - [ ] Create battle summary template (casualties, outcome, dramatic moment)
    - [ ] Add closing paragraph template
    - [ ] Integrate with action templates from PHASE1-006
  - Estimated effort: 4-6 hours
  - Dependencies: PHASE1-006

- [ ] **Build Narrative Generation Pipeline**
  - Task ID: PHASE1-008
  - Description: Orchestrate full narrative generation from battle data
  - Subtasks:
    - [ ] Accept battle object (gangs, rounds, actions, metadata)
    - [ ] Build narrative in order: opening → rounds → summary → closing
    - [ ] Substitute gang names, fighter names, weapons
    - [ ] Insert dramatic moments (crits, OOA, comebacks)
    - [ ] Validate output grammar/coherence
    - [ ] Test with 5+ sample battles
  - Estimated effort: 6-10 hours
  - Dependencies: PHASE1-006, PHASE1-007

- [ ] **Implement HTML Export**
  - Task ID: PHASE1-009
  - Description: Export narrative as styled HTML
  - Subtasks:
    - [ ] Create HTML template (structure for report)
    - [ ] Add basic CSS styling (readable, mobile-friendly)
    - [ ] Insert narrative content into template
    - [ ] Generate download link or copy-to-clipboard
    - [ ] Test on mobile and desktop
    - [ ] Test in multiple browsers
  - Estimated effort: 4-6 hours
  - Dependencies: PHASE1-008

- [ ] **Build Gang Roster Section**
  - Task ID: PHASE1-010
  - Description: Include fighter rosters with final status in HTML export
  - Subtasks:
    - [ ] Create roster table template
    - [ ] Columns: Fighter Name, Equipment, Skills, Status (end of battle)
    - [ ] Pull data from imported Munda Manager gang
    - [ ] Show injuries/OOA status from battle log
    - [ ] Include for both gangs in report
  - Estimated effort: 2-3 hours
  - Dependencies: PHASE1-009, PHASE1-005

- [ ] **Build Battle Summary Section**
  - Task ID: PHASE1-011
  - Description: Create battle metadata summary (gangs, scenario, outcome, casualties)
  - Subtasks:
    - [ ] Design summary layout (paragraph or table)
    - [ ] Include: Gang names, houses, scenario, location, date, outcome
    - [ ] Add casualty summary (gang A: 0 OOA, gang B: 2 OOA)
    - [ ] Make visually distinct from narrative prose
  - Estimated effort: 2-3 hours
  - Dependencies: PHASE1-009, PHASE1-005

### Priority: High (Complete MVP)

- [ ] **Create End-to-End Happy Path Test**
  - Task ID: PHASE1-012
  - Description: Full test from Munda Manager link → battle log → HTML export
  - Subtasks:
    - [ ] Set up test with sample Munda Manager share link
    - [ ] Log complete 3-round battle
    - [ ] Generate HTML export
    - [ ] Verify: gang data correct, activation order respected, actions match outcomes
    - [ ] Check: narrative reads coherently, no typos, formatting looks good
  - Estimated effort: 2-3 hours
  - Dependencies: All PHASE1 tasks

- [ ] **Playtest with Real User (1-2 testers)**
  - Task ID: PHASE1-013
  - Description: Get feedback from actual Necromunda players
  - Subtasks:
    - [ ] Recruit 1-2 testers familiar with Necromunda 2E
    - [ ] Have them complete end-to-end flow
    - [ ] Collect feedback on:
      - Munda Manager import (easy/hard?)
      - Battle input UI (intuitive?)
      - Generated narrative (accurate? entertaining?)
      - Export quality (useful as-is?)
    - [ ] Document pain points and suggestions
    - [ ] Prioritize fixes for Phase 1 vs. Phase 2
  - Estimated effort: 4-6 hours (including test prep/analysis)
  - Dependencies: PHASE1-012

### Priority: Medium (Nice-to-have for MVP)

- [ ] **Add Basic Error Handling & Validation**
  - Task ID: PHASE1-014
  - Description: Graceful error messages for common issues
  - Subtasks:
    - [ ] Invalid Munda Manager link
    - [ ] Missing required fields in battle input
    - [ ] Malformed action outcomes
    - [ ] Export failures
    - [ ] User-friendly error messages for each
  - Estimated effort: 3-4 hours
  - Dependencies: All input-related tasks

- [ ] **Add Help/Documentation**
  - Task ID: PHASE1-015
  - Description: Inline help text and quick-start guide
  - Subtasks:
    - [ ] Add tooltips to form fields
    - [ ] Write 1-page quick start guide
    - [ ] Explain action types and how to log them
    - [ ] Link to Necromunda 2E rules if needed
  - Estimated effort: 2-3 hours
  - Dependencies: All UI tasks

---

## Phase 2: Polish & Customization

### Priority: Critical

- [ ] **Implement PDF Export**
  - Task ID: PHASE2-001
  - Description: Export narrative as professional PDF
  - Subtasks:
    - [ ] Choose PDF library (PDFKit, Puppeteer, etc.)
    - [ ] Create PDF template (similar to HTML but print-optimized)
    - [ ] Test layout on A4/Letter paper
    - [ ] Verify all content is readable in PDF reader
    - [ ] Test with different report lengths (short/medium/long)
  - Estimated effort: 4-6 hours
  - Dependencies: PHASE1-009

- [ ] **Implement Docx Export**
  - Task ID: PHASE2-002
  - Description: Export narrative as formatted Word document
  - Subtasks:
    - [ ] Choose Docx library (docx-js, docx, etc.)
    - [ ] Create Docx template with proper formatting
    - [ ] Add headings, paragraphs, tables (rosters)
    - [ ] Include page breaks for readability
    - [ ] Test in Microsoft Word and Google Docs
    - [ ] Verify file can be edited by users
  - Estimated effort: 4-6 hours
  - Dependencies: PHASE1-009

- [ ] **Implement Multiple Narrative Tones**
  - Task ID: PHASE2-003
  - Description: Add 3+ selectable narrative voices
  - Subtasks:
    - [ ] Design 3 tones:
      - Gritty & Dark (noir, consequences, serious)
      - Over-the-Top Action (bombastic, cinematic, absurd)
      - Dark Comedy (slapstick, ridiculous, irreverent)
    - [ ] Rewrite template library for each tone (word choice, emphasis, humor)
    - [ ] Add tone selector to UI
    - [ ] Generate sample narratives for each tone with same battle
    - [ ] Get feedback on tone distinctiveness
  - Estimated effort: 10-15 hours
  - Dependencies: PHASE1-006

- [ ] **Deepen House-Specific Lore**
  - Task ID: PHASE2-004
  - Description: Add authentic flavor text for each Necromunda house
  - Subtasks:
    - [ ] Research each house (Goliath, Escher, Cawdor, Delaque, Van Saar, Orlock, Enforcer)
    - [ ] Create house-specific vocabulary & aesthetics
    - [ ] Write 10-20 house-specific action variants per house
    - [ ] Auto-detect house from imported gang
    - [ ] Weave house flavor into narrative prose
    - [ ] Test with mixed-house battles
  - Estimated effort: 12-16 hours
  - Dependencies: PHASE1-006

- [ ] **Add Custom Notes Per Moment**
  - Task ID: PHASE2-005
  - Description: Allow users to inject custom narrative snippets
  - Subtasks:
    - [ ] Add optional "narrative note" field per activation
    - [ ] Create merge strategy (user note + template prose)
    - [ ] Test readability of merged text
    - [ ] Add example use cases (personal vendetta, dramatic moment, house politics)
  - Estimated effort: 3-4 hours
  - Dependencies: PHASE1-004, PHASE1-008

- [ ] **Add Fighter Nicknames & Custom Names**
  - Task ID: PHASE2-006
  - Description: Allow users to add/edit fighter nicknames for narrative
  - Subtasks:
    - [ ] Edit nickname during gang review (PHASE1-002)
    - [ ] Store nickname alongside Munda Manager name
    - [ ] Prioritize nicknames in narrative (use nickname if available)
    - [ ] Add example ("Fighter Bob" → "Scar-Eye Bob")
  - Estimated effort: 2-3 hours
  - Dependencies: PHASE1-002, PHASE1-008

### Priority: High

- [ ] **Improve Narrative Variety & Anti-Repetition**
  - Task ID: PHASE2-007
  - Description: Reduce template fatigue across multiple actions of same type
  - Subtasks:
    - [ ] Audit template library for variety
    - [ ] Create synonym lists for common phrases
    - [ ] Implement random selection from variant templates
    - [ ] Test with 5+ shot actions in sequence (should not repeat)
    - [ ] Track which templates were used (avoid immediate repeats)
  - Estimated effort: 6-8 hours
  - Dependencies: PHASE1-006

- [ ] **Add Battle Metadata to Reports**
  - Task ID: PHASE2-008
  - Description: Embed scenario name, location, date in exported reports
  - Subtasks:
    - [ ] Add metadata section to HTML/PDF/Docx exports
    - [ ] Include: Gang names, houses, scenario, location, date, outcome
    - [ ] Format as header or info box
    - [ ] Make visually distinct from narrative
  - Estimated effort: 2-3 hours
  - Dependencies: PHASE1-009, PHASE1-010

- [ ] **Playtest with Multiple Groups**
  - Task ID: PHASE2-009
  - Description: Get feedback from 5+ different Necromunda players/groups
  - Subtasks:
    - [ ] Recruit diverse testers (different houses, playstyles)
    - [ ] Have each generate 2-3 battle reports
    - [ ] Collect feedback on:
      - Tone variety (is it noticeable?)
      - House flavor (does Goliath feel different from Escher?)
      - Output quality (docx/PDF formatting)
      - Usefulness for sharing with their group
    - [ ] Document feature requests
  - Estimated effort: 6-8 hours (including analysis)
  - Dependencies: PHASE2-003, PHASE2-004

### Priority: Medium

- [ ] **Add Injury Type Input**
  - Task ID: PHASE2-010
  - Description: Let users specify injury severity (minor, serious, critical)
  - Subtasks:
    - [ ] Add injury type dropdown when marking fighter injured
    - [ ] Store injury data with fighter status
    - [ ] Reference injury type in narrative (e.g., "grievous wound")
    - [ ] Include in roster section
  - Estimated effort: 2-3 hours
  - Dependencies: PHASE1-005

---

## Phase 3: Campaign & Advanced Features

### Priority: Critical

- [ ] **Build Campaign Data Model**
  - Task ID: PHASE3-001
  - Description: Design data structure for linked battles over time
  - Subtasks:
    - [ ] Define campaign object (name, gangs, battle list, metadata)
    - [ ] Create persistence layer (database schema or file storage)
    - [ ] Plan for injury/recovery tracking across battles
    - [ ] Design gang reputation/stats tracking
    - [ ] Document migration path (Phase 2 battle data → campaign)
  - Estimated effort: 4-6 hours
  - Blocker for: Campaign implementation

- [ ] **Build Campaign View**
  - Task ID: PHASE3-002
  - Description: Display list of battles in chronological order
  - Subtasks:
    - [ ] Design campaign dashboard layout
    - [ ] Show battle list with: date, gangs, outcome, casualties
    - [ ] Add battle detail cards (quick stats)
    - [ ] Link to full battle reports
    - [ ] Implement campaign creation UI
  - Estimated effort: 5-8 hours
  - Dependencies: PHASE3-001

- [ ] **Implement Injury Tracking Between Battles**
  - Task ID: PHASE3-003
  - Description: Persist injuries and recovery across multiple battles
  - Subtasks:
    - [ ] Add injury roll input after battle (user specifies recovery time)
    - [ ] Track recovery period in campaign
    - [ ] Auto-disable injured fighters in subsequent battles (until recovered)
    - [ ] Reference recovery status in narrative ("still nursing that plasma wound")
    - [ ] Add injury history to fighter profile
  - Estimated effort: 6-8 hours
  - Dependencies: PHASE3-001, PHASE2-010

- [ ] **Add Cumulative Fighter Stats**
  - Task ID: PHASE3-004
  - Description: Track lifetime stats (battles, kills, injuries, skills earned)
  - Subtasks:
    - [ ] Design stats schema (battles fought, kills, taken OOA count, skills acquired)
    - [ ] Auto-calculate from campaign battle history
    - [ ] Display on fighter profile (in campaign view)
    - [ ] Reference in narrative (veteran vs. juve flavor)
    - [ ] Show leaderboard per campaign (top killers, etc.)
  - Estimated effort: 6-10 hours
  - Dependencies: PHASE3-001, PHASE3-002

- [ ] **Implement Campaign Export**
  - Task ID: PHASE3-005
  - Description: Export full campaign as single document (HTML/PDF/Docx)
  - Subtasks:
    - [ ] Create campaign report template (campaign intro, battle list with full narratives)
    - [ ] Generate for HTML/PDF/Docx formats
    - [ ] Test with 3-5 battle campaigns
    - [ ] Ensure formatting remains readable in long document
    - [ ] Add table of contents
  - Estimated effort: 6-8 hours
  - Dependencies: PHASE3-002, PHASE2-001, PHASE2-002

### Priority: High

- [ ] **Add Campaign-Level Narrative Arc**
  - Task ID: PHASE3-006
  - Description: Weave battle summaries into overarching campaign story
  - Subtasks:
    - [ ] Add campaign metadata (setting, intro narrative, stakes)
    - [ ] Generate campaign intro paragraph (establish stakes/atmosphere)
    - [ ] Add inter-battle summaries (gang reputation changes, injury updates)
    - [ ] Create campaign conclusion (final standings, campaign winner)
    - [ ] Test readability across 5+ battles
  - Estimated effort: 8-10 hours
  - Dependencies: PHASE3-002

- [ ] **Build Gang Profile Page**
  - Task ID: PHASE3-007
  - Description: Show gang stats and evolution over campaign
  - Subtasks:
    - [ ] Design gang profile layout
    - [ ] Display current roster with injury status
    - [ ] Show campaign stats (battles fought, wins/losses, casualties)
    - [ ] Display reputation/status changes
    - [ ] List major moments/achievements
    - [ ] Link to relevant battles
  - Estimated effort: 4-6 hours
  - Dependencies: PHASE3-002, PHASE3-004

### Priority: Medium

- [ ] **Add Campaign Leaderboards/Rankings**
  - Task ID: PHASE3-008
  - Description: Track and display campaign standings
  - Subtasks:
    - [ ] Design leaderboard layout
    - [ ] Calculate rankings: wins, kill count, fighter survival rate
    - [ ] Display gang standings per campaign
    - [ ] Add fighter rankings (top killers, most survived, etc.)
    - [ ] Update in real-time as battles are added
  - Estimated effort: 4-6 hours
  - Dependencies: PHASE3-004

---

## Infrastructure & DevOps

### Priority: High

- [ ] **Set Up Version Control**
  - Task ID: INFRA-001
  - Description: Initialize GitHub repo with structure
  - Subtasks:
    - [ ] Create GitHub repository
    - [ ] Set up .gitignore for node_modules, env files, etc.
    - [ ] Create initial folder structure (src, tests, docs, etc.)
    - [ ] Add README with project overview
    - [ ] Add contributing guidelines
  - Estimated effort: 1-2 hours

- [ ] **Set Up Build & Development Environment**
  - Task ID: INFRA-002
  - Description: Configure build tools and local dev setup
  - Subtasks:
    - [ ] Choose build tool (Webpack, Vite, etc.)
    - [ ] Configure for dev/prod environments
    - [ ] Set up npm scripts (dev, build, test)
    - [ ] Create .env template for config
    - [ ] Write setup instructions in README
  - Estimated effort: 2-3 hours
  - Dependencies: INFRA-001, SETUP-002

- [ ] **Set Up Testing Framework**
  - Task ID: INFRA-003
  - Description: Configure unit and integration tests
  - Subtasks:
    - [ ] Choose testing framework (Jest, Mocha, etc.)
    - [ ] Set up test runner and coverage reports
    - [ ] Create sample test file
    - [ ] Add test npm script
    - [ ] Document testing conventions
  - Estimated effort: 2-3 hours
  - Dependencies: INFRA-002

### Priority: Medium

- [ ] **Set Up Continuous Integration (CI)**
  - Task ID: INFRA-004
  - Description: Automate testing on commits
  - Subtasks:
    - [ ] Choose CI platform (GitHub Actions, CircleCI, etc.)
    - [ ] Create CI workflow (run tests on PR)
    - [ ] Configure notifications
    - [ ] Document CI process
  - Estimated effort: 2-4 hours
  - Dependencies: INFRA-003

- [ ] **Set Up Deployment Pipeline (if needed)**
  - Task ID: INFRA-005
  - Description: Automate deployment to staging/production
  - Subtasks:
    - [ ] Choose hosting (Vercel, Netlify, Heroku, etc.)
    - [ ] Configure automatic deployment from main branch
    - [ ] Set up staging environment
    - [ ] Document deployment process
  - Estimated effort: 3-4 hours
  - Dependencies: INFRA-004

---

## Content & Documentation

### Priority: High

- [ ] **Create Necromunda Rule Reference**
  - Task ID: CONTENT-001
  - Description: Document 2E rules relevant to this tool
  - Subtasks:
    - [ ] List common actions (shoot, move, charge, etc.)
    - [ ] Document outcomes (hit/miss, injury severity, OOA)
    - [ ] Create quick reference for weapon types
    - [ ] Document house-specific rules (if relevant)
    - [ ] Add example battle turn
  - Estimated effort: 3-4 hours

- [ ] **Create User Documentation**
  - Task ID: CONTENT-002
  - Description: Write guide for using the tool
  - Subtasks:
    - [ ] Quick start guide (5 minutes to first report)
    - [ ] Detailed walkthrough (each step with screenshots)
    - [ ] FAQ (common issues, tips)
    - [ ] Glossary (Necromunda terms, tool-specific terms)
  - Estimated effort: 4-6 hours
  - Dependencies: PHASE1-015

- [ ] **Create Developer Documentation**
  - Task ID: CONTENT-003
  - Description: Document codebase for future contributors
  - Subtasks:
    - [ ] Architecture overview diagram
    - [ ] Data flow documentation
    - [ ] Component/module descriptions
    - [ ] API documentation (if applicable)
    - [ ] Contributing guidelines
    - [ ] Known issues and future improvements
  - Estimated effort: 4-5 hours
  - Dependencies: All dev tasks

### Priority: Medium

- [ ] **Create Sample Battles**
  - Task ID: CONTENT-004
  - Description: Pre-generate example reports for different scenarios
  - Subtasks:
    - [ ] Create 3-4 sample battles (different houses, scenarios)
    - [ ] Generate reports in all export formats
    - [ ] Use as test cases and examples
    - [ ] Showcase on website or in docs
  - Estimated effort: 3-4 hours
  - Dependencies: PHASE1-012

---

## Community & Feedback

### Priority: High

- [ ] **Recruit Beta Testers**
  - Task ID: COMMUNITY-001
  - Description: Find 5-10 Necromunda players for feedback
  - Subtasks:
    - [ ] Post in Necromunda Discord/Reddit communities
    - [ ] Reach out to known content creators
    - [ ] Gather contact info for testers
    - [ ] Create beta tester agreement/feedback form
    - [ ] Schedule testing windows
  - Estimated effort: 2-3 hours

- [ ] **Set Up Feedback Channels**
  - Task ID: COMMUNITY-002
  - Description: Create way for users to report issues and request features
  - Subtasks:
    - [ ] Create GitHub Issues template (bug report, feature request)
    - [ ] Set up Discord channel or Slack for community chat
    - [ ] Create user feedback form
    - [ ] Document how feedback gets prioritized
  - Estimated effort: 2-3 hours

### Priority: Medium

- [ ] **Write Blog Posts**
  - Task ID: COMMUNITY-003
  - Description: Share the project and generate interest
  - Subtasks:
    - [ ] Write "What is this tool?" post
    - [ ] Create tutorial post (how to use)
    - [ ] Share sample battle reports
    - [ ] Post to Reddit r/necromunda, Discord communities
  - Estimated effort: 4-6 hours
  - Dependencies: PHASE1-012

---

## Summary by Timeline

### Week 1-2: Investigation & Setup
- SETUP-001, SETUP-002, SETUP-003, SETUP-004 (all critical investigation)
- INFRA-001, INFRA-002, INFRA-003 (dev environment)

### Week 3-6: Phase 1 MVP
- PHASE1-001 through PHASE1-015
- PHASE1-012, PHASE1-013 (testing & validation)

### Week 7-10: Phase 2 Polish
- PHASE2-001 through PHASE2-010
- PHASE2-009 (playtesting)

### Week 11-14: Phase 3 Campaign (if pursuing)
- PHASE3-001 through PHASE3-008

### Ongoing:
- INFRA-004, INFRA-005 (CI/CD)
- CONTENT-001, CONTENT-002, CONTENT-003 (documentation)
- COMMUNITY-001, COMMUNITY-002, COMMUNITY-003 (feedback & community)

---

## Task Statistics

- **Total Tasks:** 80+
- **Phase 1 (MVP):** 15 tasks (~60-80 hours)
- **Phase 2 (Polish):** 10 tasks (~40-60 hours)
- **Phase 3 (Campaign):** 8 tasks (~40-60 hours)
- **Infrastructure:** 5 tasks (~10-18 hours)
- **Content & Community:** 8 tasks (~20-30 hours)

**Estimated Total:** 160-250 hours for all phases

---

## Legend

- [ ] = Incomplete
- [x] = Complete
- Task ID = Reference code for tracking
- Dependencies = Which tasks must finish before this one starts
- Estimated effort = Hours to complete (if working alone)

