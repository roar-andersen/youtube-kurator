# Minimal Task Prompt Template

**Denne filen kan copy-pastes direkte til en agent for å kjøre en enkelt oppgave.**

---

## For Task 1.1 (Example)

```
Execute this task from YouTube-kurator v1:

📋 Task: 1.1 – Opprett ASP.NET Core-prosjekt og grunnleggende struktur

📂 Read the full task file: spec/task-1-1-opprett-asp-net-core-prosjekt.md

🎯 What to do (short version):
- Opprett ASP.NET Core Web API prosjekt
- Lag mappestruktur (Controllers, Services, Data, wwwroot)
- Konfigurer Program.cs
- Sett opp appsettings.json
- Installer NuGet-pakker

✅ When you're done, report:
1. Status: Completed / Blocked / Failed
2. What you did (2-3 sentences)
3. Files created
4. Any blockers

Full details in the task file!
```

---

## For Task 1.2

```
Execute this task from YouTube-kurator v1:

📋 Task: 1.2 – Opprett Dockerfile og container-konfigurasjon

📂 Read the full task file: spec/task-1-2-opprett-dockerfile.md

🎯 What to do (short version):
- Opprett Dockerfile med multisteg-build
- Opprett .dockerignore
- Verifiser at Docker-image bygges
- Test at container kjører lokalt

✅ When you're done, report:
1. Status: Completed / Blocked / Failed
2. What you did (2-3 sentences)
3. Files created
4. Any blockers

Full details in the task file!
```

---

## TEMPLATE FOR ANY TASK

```
Execute this task from YouTube-kurator v1:

📋 Task: [X.Y] – [Task Name]

📂 Read the full task file: spec/task-[X]-[Y]-[task-name].md

🎯 What to do (short version):
[2-3 bullet points]

✅ When you're done, report:
1. Status: Completed / Blocked / Failed
2. What you did (2-3 sentences)
3. Files created
4. Any blockers

Full details in the task file!
```

---

## HOW TO USE

1. **Pick a task** from the QUICK-START-ORCHESTRATOR.md status list
2. **Copy the template above**
3. **Fill in** [X.Y], [Task Name], and bullet points
4. **Give to agent** (via Task tool or in conversation)
5. **Agent reads task file** for full details
6. **Agent executes** and reports back

---

## EVEN SIMPLER: One-Line Version

If you want ultra-minimal:

```
Execute task spec/task-[X]-[Y]-[name].md and report back when done.
```

That's it! The task file has everything.

---

**This is the simplest way to hand off tasks to agents with minimal context.**
