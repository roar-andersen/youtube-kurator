# Quick Start Orchestrator – Copy & Paste Ready

**Bruk denne templaten** for å starte task-orkestrering med en agent umiddelbart.

---

## INSTRUCTJON FOR ORCHESTRATOR-AGENT

Du er Task Orchestrator for YouTube-kurator. Din jobb:

1. **Velg neste oppgave** (se status-lista under)
2. **Ring en subagent** med oppgavefilen
3. **Track progresjon**
4. **Gjenta** til alle oppgaver er ferdig

---

## CURRENT STATUS (Real-time)

```
📊 PROGRESS: 0/22 tasks completed (0%)

✅ COMPLETED (0)
[Ingen oppgaver ferdig ennå]

🔄 IN PROGRESS
[Ingen oppgave kjører nå]

⏳ READY TO START (No dependencies)
- Task 1.1: Opprett ASP.NET Core-prosjekt
- Task 1.2: Opprett Dockerfile

🔗 PENDING (Waiting for dependencies)
- Task 2.1: Implementer entiteter (waiting: 1.1)
- Task 2.2: Opprett DbContext (waiting: 1.1)
- Task 2.3: Lag migrasjoner (waiting: 2.1, 2.2)
- Task 3.1: PlaylistsController (waiting: 2.3)
- Task 4.1: YouTubeService (waiting: 2.3)
- Task 4.2: Caching (waiting: 4.1)
- Task 5.1: Refresh-endepunkt (waiting: 3.1, 4.2)
- Task 6.1: index.html (waiting: 1.1)
- Task 6.2: styles.css (waiting: 6.1)
- Task 7.1: Alpine.js (waiting: 6.1, 6.2)
- Task 7.2: Navigasjon (waiting: 7.1)
- Task 8.1: PWA manifest (waiting: 6.1)
- Task 8.2: Service Worker (waiting: 6.1)
- Task 9.1: Feilmeldinger (waiting: 7.1)
- Task 9.2: Loading-indikatorer (waiting: 7.1)
- Task 10.1: Unit tests (waiting: 4.1, 4.2, 3.1)
- Task 10.2: E2E testing (waiting: hele appen)
- Task 11.1: Azure SQL (waiting: 2.3)
- Task 11.2: Container Apps (waiting: 11.1)
- Task 11.3: Deploy (waiting: 11.2)
- Task 12.1: Dokumentasjon (waiting: hele appen)

🚫 BLOCKED
[Ingen oppgaver blokkert]
```

---

## NESTE STEG

**Valg**: Task 1.1 eller Task 1.2 (kjør parallelt hvis mulig, eller sekvensielt)

**Anbefaling**: Start med **Task 1.1**, så Task 1.2

---

## INSTRUKSJON FOR SUBAGENT (Copy & Paste for Next Task)

```
# Execute Task: [X.Y] - [Task Name]

## Din Oppgave
Du skal utføre **Task [X.Y]** fra YouTube-kurator-prosjektet.

Oppgavefilen finnes her: `spec/task-[X]-[Y]-[navn].md`

### Hva skal gjøres (Short Summary)
[INSERT SHORT DESCRIPTION FROM TASK FILE]

### Acceptence Criteria (Must all be met)
[INSERT CHECKLIST FROM TASK FILE]

### Requirements/Dependencies
[INSERT DEPENDENCIES FROM TASK FILE]

## Instruksjoner

1. **Les hele oppgavefilen** (spec/task-[X]-[Y]-[navn].md) - den har all info du trenger
2. **Utfør oppgaven** basert på beskrivelsen
3. **Verifiser** at alle akseptansekriterier er oppfylt ✓
4. **Rapportér tilbake**:
   - Status: ✅ Completed / 🚫 Blocked / ❌ Failed
   - Summary: 1-2 setninger om hva du gjorde
   - Files created/modified: Liste over filer
   - Blockers (if any): Hva som hindret deg

## No need to report to orchestrator - just complete the task!
```

---

## Orchestrator Commands

### ✅ When Task Completed
Mark in status lista:
- Move from "⏳ READY TO START" or "🔗 PENDING" to "✅ COMPLETED"
- Check what tasks now become "READY TO START"
- Pick next task from "READY TO START"

### 🚫 When Task Blocked
- Move to "🚫 BLOCKED"
- Note the blocker reason
- Try to pick a different task that has no blockers
- Come back to this task later

### ⏳ When Ready for More Tasks
- Pick **one task** from "⏳ READY TO START"
- Call subagent with task instructions
- Update status and repeat

---

## PARALLELLIZATION OPPORTUNITIES

**Current Batch** (Both can run at same time):
- Task 1.1: Opprett ASP.NET Core-prosjekt
- Task 1.2: Opprett Dockerfile

**Next Batch** (After 1.1 + 1.2 done):
- Task 2.1: Implementer entiteter
- Task 2.2: Opprett DbContext
- (Then Task 2.3 sekvensielt)

**Later Batches** (See AGENT-ORCHESTRATOR-PROMPT.md for full parallelization map)

---

## QUICK REFERENCE: Task File Locations

All task files are in: `c:\src\youku\spec\`

```
Fase 1: task-1-1-*.md, task-1-2-*.md
Fase 2: task-2-1-*.md, task-2-2-*.md, task-2-3-*.md
Fase 3: task-3-1-*.md
Fase 4: task-4-1-*.md, task-4-2-*.md
Fase 5: task-5-1-*.md
Fase 6: task-6-1-*.md, task-6-2-*.md
Fase 7: task-7-1-*.md, task-7-2-*.md
Fase 8: task-8-1-*.md, task-8-2-*.md
Fase 9: task-9-1-*.md, task-9-2-*.md
Fase 10: task-10-1-*.md, task-10-2-*.md
Fase 11: task-11-1-*.md, task-11-2-*.md, task-11-3-*.md
Fase 12: task-12-1-*.md
```

---

## ONE-LINER FOR CALLING SUBAGENT

If you're using Claude Code's Task tool:

```
Task(
  description: "Execute Task [X.Y]: [Task Name]",
  prompt: """
  # Execute Task [X.Y]: [Task Name]

  Read and execute: spec/task-[X]-[Y]-[task-name].md

  Report back with:
  - Status (Completed/Blocked/Failed)
  - Summary of what was done
  - Files created/modified
  - Any blockers
  """,
  subagent_type: "general-purpose"
)
```

---

**Status**: Ready to start!
**Current Task**: Task 1.1 recommended
**Last Updated**: 2025-01-15
