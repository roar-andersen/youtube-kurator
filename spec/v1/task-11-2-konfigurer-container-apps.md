# Oppgave 11.2: Konfigurere Azure Container Apps og Key Vault

## Fase
Fase 11: Azure-deployment

## Avhengigheter
- Oppgave 11.1 (Azure SQL og Key Vault må eksistere)
- Oppgave 1.2 (Dockerfile må eksistere)

## Formål
Sette opp Azure Container Apps og konfigurere miljøvariabler fra Key Vault.

## Oppgavebeskrivelse

### 1. Opprett Azure Container Registry (ACR)
```bash
az acr create \
  --resource-group youtube-kurator-rg \
  --name youtubekuratoracr \
  --sku Basic \
  --admin-enabled true
```

### 2. Lag app-miljø for Container Apps
```bash
az containerapp env create \
  --name youtube-kurator-env \
  --resource-group youtube-kurator-rg \
  --location norwayeast
```

### 3. Opprett User-Assigned Managed Identity
For å få tilgang til Key Vault:

```bash
az identity create \
  --name youtube-kurator-identity \
  --resource-group youtube-kurator-rg

# Få identity ID
$identityId = (az identity show --name youtube-kurator-identity --resource-group youtube-kurator-rg --query id -o tsv)
```

### 4. Gi Managed Identity tilgang til Key Vault
```bash
az keyvault set-policy \
  --name youtube-kurator-kv \
  --secret-permissions get list \
  --object-id (az identity show --name youtube-kurator-identity --resource-group youtube-kurator-rg --query principalId -o tsv)
```

### 5. Opprett Container App
```bash
az containerapp create \
  --name youtube-kurator \
  --resource-group youtube-kurator-rg \
  --environment youtube-kurator-env \
  --image youtubekuratoracr.azurecr.io/youtube-kurator:latest \
  --target-port 80 \
  --ingress external \
  --query properties.configuration.ingress.fqdn
```

### 6. Konfigurer Environment Variables
Opprett environment-variabler som refererer til Key Vault secrets:

```bash
az containerapp update \
  --name youtube-kurator \
  --resource-group youtube-kurator-rg \
  --set-env-vars \
    YOUTUBE_API_KEY=keyvaultref:YOUTUBE_API_KEY \
    ConnectionStrings__DefaultConnection=keyvaultref:SqlConnectionString
```

### 7. Konfigurer Key Vault-referanser
Sett identiteten som skal brukes for Key Vault-akses:

```bash
$identityId = (az identity show --name youtube-kurator-identity --resource-group youtube-kurator-rg --query id -o tsv)

az containerapp update \
  --name youtube-kurator \
  --resource-group youtube-kurator-rg \
  --user-assigned $identityId
```

### 8. Verifiser Container App
```bash
# Få FQDN (offentlig URL)
az containerapp show \
  --name youtube-kurator \
  --resource-group youtube-kurator-rg \
  --query properties.configuration.ingress.fqdn
```

Besøk URL-en og verifiser at appen åpnes.

### 9. Konfigurer auto-scaling (valgfritt)
```bash
az containerapp update \
  --name youtube-kurator \
  --resource-group youtube-kurator-rg \
  --min-replicas 1 \
  --max-replicas 3
```

## Akseptansekriterier
- [ ] Azure Container Registry opprettet
- [ ] Container Apps-miljø opprettet
- [ ] Managed Identity opprettet
- [ ] Managed Identity har tilgang til Key Vault
- [ ] Container App opprettet
- [ ] Environment-variabler satt (YOUTUBE_API_KEY, ConnectionStrings__DefaultConnection)
- [ ] App er tilgjengelig på offentlig URL
- [ ] Helse-sjekk kjører (app starter uten feil)

## Referanser
- [Spesifikasjon: Hosting](youtube-kurator-v1-spec.md#2-teknologivalg)
- [Azure Container Apps Documentation](https://learn.microsoft.com/en-us/azure/container-apps/)
- [Azure Container Apps Secrets & Key Vault](https://learn.microsoft.com/en-us/azure/container-apps/manage-secrets)
- [Azure Managed Identities](https://learn.microsoft.com/en-us/azure/active-directory/managed-identities-azure-resources/)
