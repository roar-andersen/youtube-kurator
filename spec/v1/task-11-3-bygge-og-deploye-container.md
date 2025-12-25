# Oppgave 11.3: Bygge og deploye container

## Fase
Fase 11: Azure-deployment

## Avhengigheter
- Oppgave 11.2 (Container Apps og Registry må eksistere)
- Oppgave 1.2 (Dockerfile må eksistere)
- Alle tidligere oppgaver (appen må være ferdig)

## Formål
Bygge Docker-image og deploye til Azure Container Apps.

## Oppgavebeskrivelse

### 1. Logg inn på Azure
```bash
az login
az account set --subscription "Your Subscription ID"
```

### 2. Logg inn på Container Registry
```bash
az acr login --name youtubekuratoracr
```

### 3. Bygg Docker-image lokalt (test)
```bash
docker build -t youtube-kurator:latest .
docker run -p 8080:80 youtube-kurator:latest
```

Besøk `http://localhost:8080` og verifiser at appen fungerer.

### 4. Tag image for ACR
```bash
docker tag youtube-kurator:latest youtubekuratoracr.azurecr.io/youtube-kurator:latest
```

### 5. Push image til ACR
```bash
docker push youtubekuratoracr.azurecr.io/youtube-kurator:latest
```

### 6. Oppdater Container App med nytt image
```bash
az containerapp update \
  --name youtube-kurator \
  --resource-group youtube-kurator-rg \
  --image youtubekuratoracr.azurecr.io/youtube-kurator:latest
```

### 7. Verifiser deployment
```bash
# Sjekk app-status
az containerapp show \
  --name youtube-kurator \
  --resource-group youtube-kurator-rg \
  --query properties.provisioningState

# Sjekk replicas
az containerapp replica list \
  --name youtube-kurator \
  --resource-group youtube-kurator-rg
```

### 8. Test produksjon-appen
```bash
# Få FQDN
$url = az containerapp show \
  --name youtube-kurator \
  --resource-group youtube-kurator-rg \
  --query properties.configuration.ingress.fqdn \
  -o tsv

# Åpne i nettleser
start https://$url
```

Verifiser at:
- Appen lastes
- Du kan opprett spillelister
- Du kan refresh videoer
- Service Worker er registrert

### 9. Sjekk logs
```bash
# Se de siste logs fra container
az containerapp logs show \
  --name youtube-kurator \
  --resource-group youtube-kurator-rg \
  --follow
```

Sjekk for errors eller warnings.

### 10. Konfigurer CD-pipeline (valgfritt)
For automatisk deployment når du pusher til Git:

Lag `.github/workflows/deploy.yml` (hvis du bruker GitHub):

```yaml
name: Deploy to Azure Container Apps

on:
  push:
    branches: [ main ]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2

      - name: Build and push image
        run: |
          docker login -u ${{ secrets.REGISTRY_USERNAME }} \
            -p ${{ secrets.REGISTRY_PASSWORD }} \
            youtubekuratoracr.azurecr.io
          docker build -t youtubekuratoracr.azurecr.io/youtube-kurator:latest .
          docker push youtubekuratoracr.azurecr.io/youtube-kurator:latest

      - name: Deploy to Container Apps
        run: |
          az login --service-principal \
            -u ${{ secrets.AZURE_CLIENT_ID }} \
            -p ${{ secrets.AZURE_CLIENT_SECRET }} \
            --tenant ${{ secrets.AZURE_TENANT_ID }}
          az containerapp update \
            --name youtube-kurator \
            --resource-group youtube-kurator-rg \
            --image youtubekuratoracr.azurecr.io/youtube-kurator:latest
```

## Akseptansekriterier
- [ ] Docker-image bygges uten feil lokalt
- [ ] Lokalt container-test (`docker run`) fungerer
- [ ] Image pushet til ACR
- [ ] Container App oppdateres med nytt image
- [ ] Deployment vellykket (provisioning state = Succeeded)
- [ ] App er tilgjengelig på offentlig URL
- [ ] Alle features fungerer i produksjon (CRUD, refresh, PWA)
- [ ] Logs viser ingen errors
- [ ] CD-pipeline satt opp (hvis ønsket)

## Referanser
- [Spesifikasjon: Deployment](youtube-kurator-v1-spec.md#2-teknologivalg)
- [Docker Documentation](https://docs.docker.com/)
- [Azure Container Registry](https://learn.microsoft.com/en-us/azure/container-registry/)
- [Azure Container Apps Deployment](https://learn.microsoft.com/en-us/azure/container-apps/deploy-containerapp)
