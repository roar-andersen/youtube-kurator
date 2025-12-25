# Oppgave 1.2: Opprett Dockerfile og container-konfigurasjon

## Fase
Fase 1: Prosjektoppsett

## Avhengigheter
- Oppgave 1.1 (ASP.NET Core-prosjektet må være opprettet)

## Formål
Konfigurere Docker-containeren slik at ASP.NET Core-appen med statiske frontend-filer kan bygges og kjøres.

## Oppgavebeskrivelse

### 1. Opprett Dockerfile
Opprett `Dockerfile` i rotmappen av prosjektet (`YouTubeKurator/` eller der `src/` mappen ligger).

Dockerfile skal:
- Bruke multisteg-build (build-stage og runtime-stage)
- Build-stage: `mcr.microsoft.com/dotnet/sdk:8.0` eller nyere
- Runtime-stage: `mcr.microsoft.com/dotnet/aspnet:8.0` eller nyere
- Kopiere `src/YouTubeKurator.Api/` inn i build-context
- Bygge prosjektet med `dotnet publish`
- Kopiere publishet output til runtime-image
- Eksponere port 80
- Sette ENTRYPOINT til å kjøre applikasjonen

Eksempel-struktur:
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/YouTubeKurator.Api/YouTubeKurator.Api.csproj", "YouTubeKurator.Api/"]
RUN dotnet restore "YouTubeKurator.Api/YouTubeKurator.Api.csproj"
COPY src/ .
WORKDIR "/src/YouTubeKurator.Api"
RUN dotnet build "YouTubeKurator.Api.csproj" -c Release -o /app/build

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/build .
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
ENTRYPOINT ["dotnet", "YouTubeKurator.Api.dll"]
```

**OBS**: Tilpass bildelagene (.NET versjon) etter hvilket SDK/Runtime du bruker. 8.0 er typisk for moderne ASP.NET Core.

### 2. Opprett .dockerignore
Opprett `.dockerignore` i rotkatalogen for å ekskludere unødvendige filer fra Docker-bygget:
```
.git
.gitignore
.dockerignore
bin/
obj/
packages/
*.db
.vs/
.vscode/
node_modules/
```

### 3. Verifiser build-funksjonalitet
Test at Dockerfile bygges korrekt:
```bash
docker build -t youtube-kurator:latest .
```

Verifiser at bildet oppstår uten feil.

### 4. Verifiser container-kjøring lokalt
Test at containeren kan kjøres:
```bash
docker run -p 8080:80 youtube-kurator:latest
```

Besøk `http://localhost:8080` i nettleseren og verifiser at applikasjonen starter (kan få 404 på root hvis `wwwroot/index.html` ikke finnes ennå, men applikasjonen skal være oppe).

## Akseptansekriterier
- [ ] `Dockerfile` eksisterer i rotkatalogen
- [ ] `.dockerignore` eksisterer og inneholder minimalt `bin/`, `obj/`, `.git`
- [ ] `docker build -t youtube-kurator:latest .` bygges uten feil
- [ ] `docker run -p 8080:80 youtube-kurator:latest` starter containeren
- [ ] Containeren eksponerer port 80
- [ ] Bildet bruker multisteg-build (build og runtime)

## Referanser
- [Spesifikasjon: Teknologivalg – Deployment](youtube-kurator-v1-spec.md#2-teknologivalg)
- [Spesifikasjon: Arkitektur](youtube-kurator-v1-spec.md#3-arkitektur)
