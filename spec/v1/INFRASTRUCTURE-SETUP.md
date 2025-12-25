# YouTube-kurator v1 – Infrastruktur Setup Sjekkliste

Denne sjekklisten dekker alt du må sette opp før implementering kan starte.

## 1. Lokalt Setup

### 1.1 Systemkrav
- [ ] Windows 10/11, macOS, eller Linux
- [ ] 8GB RAM minimum, 50GB ledig disk
- [ ] Internettforbindelse

### 1.2 Utvikler-verktøy
- [ ] **Git** installert
  ```bash
  git --version
  ```
  - Windows: Installer fra [git-scm.com](https://git-scm.com/)
  - macOS: `brew install git`
  - Linux: `apt-get install git` eller ekvivalent

- [ ] **.NET 8.0 SDK eller nyere**
  ```bash
  dotnet --version
  ```
  - Last ned fra [dotnet.microsoft.com](https://dotnet.microsoft.com/download)

- [ ] **Visual Studio Code** (eller Visual Studio Community)
  - VS Code: [code.visualstudio.com](https://code.visualstudio.com/)
  - Tillegg: C#, REST Client, SQLite (valgfritt)

- [ ] **Docker Desktop**
  ```bash
  docker --version
  ```
  - Windows/macOS: [docker.com/products/docker-desktop](https://www.docker.com/products/docker-desktop)
  - Linux: `apt-get install docker.io`

### 1.3 Lokal database-verktøy (valgfritt)
- [ ] **SQLite Browser** (for å inspisere lokal database)
  - [sqlitebrowser.org](https://sqlitebrowser.org/)
  - Eller VS Code-tillegg: "SQLite" av alexcvzz

### 1.4 API-testing-verktøy (valgfritt)
- [ ] **Postman** eller **REST Client VS Code-tillegg**
  - Postman: [postman.com](https://www.postman.com/)
  - REST Client: VS Code-tillegg for inline HTTP-testing

---

## 2. YouTube API Setup

### 2.1 Google Cloud Console
- [ ] Gå til [Google Cloud Console](https://console.cloud.google.com/)
- [ ] Logg inn med Google-konto (eller opprett ny)
- [ ] Opprett nytt prosjekt
  - Navn: "YouTube Kurator" eller lignende
- [ ] Aktivér YouTube Data API v3
  - Søk etter "YouTube Data API v3"
  - Klikk "Enable"
- [ ] Opprett API-nøkkel
  - Gå til "Credentials"
  - Klikk "Create Credentials" → "API Key"
  - Kopier API-nøkkelen (sparer den senere)
- [ ] Konfigurer API-nøkkel-restriksjoner (anbefalt)
  - Restrict to HTTP Referrers
  - Legg til:
    - `http://localhost:5000`
    - `http://localhost:8080`
    - `https://*.azurecontainer.io` (etter du setter opp Azure)

### 2.2 Test YouTube API-nøkkelen
```bash
curl "https://www.googleapis.com/youtube/v3/search?part=snippet&q=test&type=video&maxResults=1&key=DIN-API-NØKKEL"
```
- [ ] Respons inneholder videoer (ikke error)

---

## 3. Azure Setup

### 3.1 Azure Abonnement
- [ ] Har Azure abonnement
  - Opprett fra [azure.microsoft.com](https://azure.microsoft.com/en-us/free/)
  - Gratis trial: $200 USD eller 30 dager
- [ ] Logg inn på [Azure Portal](https://portal.azure.com/)

### 3.2 Azure CLI
- [ ] **Azure CLI** installert
  ```bash
  az --version
  ```
  - Last ned fra [learn.microsoft.com/en-us/cli/azure/install-azure-cli](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli)

- [ ] Logg inn på Azure
  ```bash
  az login
  ```
  - Åpner nettleser for Microsoft-konto-login
  - Velg subscription hvis du har flere

- [ ] Verifiser subscription
  ```bash
  az account show
  ```

### 3.3 Azure-ressurser opprettet senere (Task 11.x)
Disse opprettes som del av Phase 11, men planlegg nå:

- [ ] **Azure Resource Group** – container for alle ressurser
- [ ] **Azure SQL Server** – database-server
- [ ] **Azure SQL Database** – youtube-kurator-db
- [ ] **Azure Container Registry (ACR)** – Docker-image storage
- [ ] **Azure Container Apps** – hosting av appen
- [ ] **Azure Key Vault** – sikker lagring av secrets
- [ ] **Managed Identity** – for Key Vault-access

**Estimert kostnad** (per måned):
- SQL Database (Basic, serverless): ~$7
- Container Apps (1 replica): ~$12
- Container Registry: ~$5
- Key Vault: ~$0.50
- **Total: ~$25/måned**

---

## 4. Git Repository Setup

### 4.1 GitHub/GitLab/Bitbucket
- [ ] Konto opprettet på GitHub/GitLab/Bitbucket
  - [github.com](https://github.com/) (anbefalt)
  - [gitlab.com](https://gitlab.com/)
  - [bitbucket.org](https://bitbucket.org/)

- [ ] Nytt repository opprettet
  - Navn: `youtube-kurator`
  - Type: Public eller Private (valg ditt)
  - Initialize with: README (valgfritt)

- [ ] Lokal repository konfigurering
  ```bash
  git config --global user.name "Dit Navn"
  git config --global user.email "din@email.com"
  ```

- [ ] SSH-nøkler opprettet (anbefalt for github/gitlab)
  ```bash
  ssh-keygen -t ed25519 -C "din@email.com"
  # Legg til offentlig nøkkel i GitHub/GitLab settings
  ```

### 4.2 .gitignore opprettet
- [ ] `.gitignore` fil opprettet i rotkatalogen
  ```
  # Secrets & config
  appsettings.Development.json
  .env

  # Build output
  bin/
  obj/
  dist/

  # Database
  *.db
  *.db-shm
  *.db-wal

  # IDE
  .vscode/
  .idea/
  .vs/
  *.swp
  *.swo

  # OS
  .DS_Store
  Thumbs.db

  # Dependencies
  node_modules/
  packages/
  ```

---

## 5. Dokumentasjon & Planlegging

### 5.1 Spesifikasjon
- [ ] `spec/youtube-kurator-v1-spec.md` lest og forstått
  - Krav, data-modell, API-endepunkter
  - Arkitektur, teknologivalg
  - Akseptansekriterier

### 5.2 Task-filer
- [ ] Alle 22 task-filer lest (eller minst oppgavene du skal jobbe med)
  - Se `spec/TASK-LIST.md`

### 5.3 Prosjekt-struktur planlagt
- [ ] Mappestruktur planlagt
  ```
  youtube-kurator/
  ├── src/YouTubeKurator.Api/
  ├── tests/YouTubeKurator.Tests/
  ├── spec/
  ├── .github/workflows/  (for CI/CD senere)
  ├── README.md
  ├── Dockerfile
  └── .gitignore
  ```

---

## 6. Team & Kommunikasjon

### 6.1 Team-medlemmer
- [ ] Team-medlemmer identifisert
  - Antall agenter/utviklere som arbeider på prosjektet
  - Arbeidsfordeling basert på oppgaver

### 6.2 Kommunikasjons-kanal
- [ ] Kommunikasjons-verktøy opprettet/valgt
  - Slack, Teams, Discord, eller annet
  - Kanal for prosjekt-oppdateringer og debugging

### 6.3 Code Review
- [ ] Code Review-prosess definert
  - Pull Request-sjekkliste
  - Hvem som approver PR-er
  - Minimum 1 review før merge

---

## 7. Testing & Quality

### 7.1 Testing-verktøy
- [ ] xUnit installert (for backend-unit-tests)
- [ ] Moq installert (for mocking)
- [ ] Jest eller Vitest vurdert (for frontend-tests, valgfritt for v1)

### 7.2 CI/CD Pipeline (GitHub Actions)
- [ ] `.github/workflows/` mappe opprettet (valgfritt for v1)
  - Vil legge inn CI-pipeline for:
    - `dotnet build`
    - `dotnet test`
    - `docker build`
    - Deploy (fase 11)

---

## 8. Secrets Management

### 8.1 Lokal secrets
- [ ] `appsettings.Development.json` opprettet (IKKE committed til Git)
  ```json
  {
    "ConnectionStrings": {
      "DefaultConnection": "Data Source=youtube-kurator.db"
    },
    "YouTubeApi": {
      "ApiKey": "din-youtube-api-nøkkel-her"
    }
  }
  ```

- [ ] YouTube API-nøkkel lagret sikkert
  - IKKE i version control
  - Notert i "sikker" lokasjon (password manager?)

### 8.2 Azure Secrets
- [ ] Azure Key Vault-plan (implementeres i fase 11)
  - Vil lagre: YouTube API-nøkkel, Database Connection String

---

## 9. Monitoring & Logging

### 9.1 Logging-setup
- [ ] Serilog eller annet logging-framework vurdert
  - For lokalt: Console logging (standard ASP.NET)
  - For produksjon: Azure Application Insights (fase 11)

### 9.2 Error Tracking
- [ ] Application Insights eller Sentry vurdert
  - For å track errors i produksjon

---

## 10. Performance & Security

### 10.1 Performance
- [ ] Lighthouse-audit plan
  - Skal testes før launch
  - Target: 90+ score på Lighthouse

### 10.2 Security
- [ ] HTTPS sikret
  - Lokal: `https://localhost:5000`
  - Produksjon: Azure Certificate Management
- [ ] CORS konfigurert korrekt
- [ ] XSS-proteksjon vurdert
- [ ] CSRF-proteksjon (hvis relevant)

---

## Sjekkliste-oppsummering

**Total oppgaver**: ~60
**Kritisk (må gjøres før start)**: ~20
**Valgfritt**: ~15

### Minimum før oppgaver 1.1 kan starte:
- [ ] Git installert
- [ ] .NET 8.0 SDK installert
- [ ] Repository opprettet
- [ ] YouTube API-nøkkel opprettet
- [ ] Azure abonnement aktivt
- [ ] Azure CLI installert og logget inn

---

## Nyttige lenker

| Ressurs | URL |
|---------|-----|
| .NET SDK | https://dotnet.microsoft.com/download |
| Git | https://git-scm.com |
| Docker | https://docker.com |
| Google Cloud Console | https://console.cloud.google.com |
| Azure Portal | https://portal.azure.com |
| YouTube API Docs | https://developers.google.com/youtube/v3 |
| ASP.NET Core Docs | https://learn.microsoft.com/en-us/aspnet/core/ |
| Alpine.js | https://alpinejs.dev |
| PWA Checklist | https://web.dev/pwa-checklist/ |

---

## Support & Troubleshooting

Hvis noe feiler under setup:

### Git-problemer
- [ ] Verifiser at Git er riktig installert: `git --version`
- [ ] Sjekk at SSH-nøkler er opprettet: `ls ~/.ssh`

### .NET-problemer
- [ ] Verifiser .NET versjon: `dotnet --version`
- [ ] Installer eller oppgrader fra [dotnet.microsoft.com](https://dotnet.microsoft.com)

### Azure-problemer
- [ ] Verifiser login: `az account show`
- [ ] Sjekk subscription: `az account list`
- [ ] Update Azure CLI: `az upgrade`

### YouTube API-problemer
- [ ] Verifiser API-nøkkel i Google Cloud Console
- [ ] Sjekk at YouTube Data API v3 er aktivert
- [ ] Test API-nøkkelen med curl-kommando (se over)

---

**Status**: ✅ Infrastruktur Setup Sjekkliste – Rev. 1.0
**Sist oppdatert**: 2025-01-15
**Neste**: Start med Task 1.1 når alt er på plass!
