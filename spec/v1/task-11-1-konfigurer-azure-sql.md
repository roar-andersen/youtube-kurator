# Oppgave 11.1: Konfigurere Azure SQL Database

## Fase
Fase 11: Azure-deployment

## Avhengigheter
- Oppgave 2.3 (Database-migrasjoner må være klare)
- Azure-abonnement må være aktivt

## Formål
Sette opp Azure SQL Database og Key Vault for produksjonsmiljøet.

## Oppgavebeskrivelse

### 1. Opprett Azure Resource Group
```bash
az group create --name youtube-kurator-rg --location norwayeast
```

### 2. Opprett Azure SQL Server
```bash
az sql server create \
  --name youtube-kurator-server \
  --resource-group youtube-kurator-rg \
  --admin-user sqladmin \
  --admin-password 'SecurePassword123!' \
  --location norwayeast
```

**OBS**: Bruk en sterk passord og lagre det sikkert (f.eks. i Key Vault).

### 3. Opprett Azure SQL Database
```bash
az sql db create \
  --server youtube-kurator-server \
  --name youtube-kurator-db \
  --resource-group youtube-kurator-rg \
  --edition Basic \
  --compute-model Serverless
```

### 4. Åpne Firewall for lokalt setup
For å kunne kjøre migrasjoner fra lokal maskin:

```bash
# Få din offentlige IP
$ip = (Invoke-WebRequest -Uri "https://api.ipify.org" -UseBasicParsing).Content

# Tillat denne IP-adressen
az sql server firewall-rule create \
  --server youtube-kurator-server \
  --resource-group youtube-kurator-rg \
  --name "LocalDevelopment" \
  --start-ip-address $ip \
  --end-ip-address $ip
```

### 5. Opprett Azure Key Vault
```bash
az keyvault create \
  --name youtube-kurator-kv \
  --resource-group youtube-kurator-rg \
  --location norwayeast
```

### 6. Lagre secrets i Key Vault
```bash
# Lagre YouTube API-nøkkel
az keyvault secret set \
  --vault-name youtube-kurator-kv \
  --name YouTubeApiKey \
  --value "din-youtube-api-nøkkel"

# Lagre Database Connection String
az keyvault secret set \
  --vault-name youtube-kurator-kv \
  --name SqlConnectionString \
  --value "Server=tcp:youtube-kurator-server.database.windows.net,1433;Initial Catalog=youtube-kurator-db;Persist Security Info=False;User ID=sqladmin;Password=SecurePassword123!;Encrypt=True;Connection Timeout=30;"
```

### 7. Kjør migrasjoner mot Azure SQL
Lokal kjøring av migrations mot Azure-databasen:

```bash
# I prosjektmappen
$connectionString = "Server=tcp:youtube-kurator-server.database.windows.net,1433;Initial Catalog=youtube-kurator-db;Persist Security Info=False;User ID=sqladmin;Password=SecurePassword123!;Encrypt=True;Connection Timeout=30;"

$env:ConnectionStrings__DefaultConnection = $connectionString
dotnet ef database update
```

### 8. Verifiser database-struktur
```bash
# Koble til Azure SQL og verifiser tabeller
sqlcmd -S youtube-kurator-server.database.windows.net -d youtube-kurator-db -U sqladmin -P "SecurePassword123!"

# Kjør SQL-spørring
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES;
GO
```

Tabeller som skal eksistere:
- `Playlists`
- `CachedSearches`
- `__EFMigrationsHistory` (EF Core-metadata)

## Akseptansekriterier
- [ ] Azure Resource Group opprettet
- [ ] Azure SQL Server opprettet
- [ ] Azure SQL Database opprettet
- [ ] Lokal IP er tillatt i Firewall
- [ ] Key Vault opprettet
- [ ] YouTube API-nøkkel lagret i Key Vault
- [ ] Database Connection String lagret i Key Vault
- [ ] Migrasjoner kjørt mot Azure SQL
- [ ] Tabeller finnes i Azure SQL Database

## Referanser
- [Spesifikasjon: Teknologivalg – Database](youtube-kurator-v1-spec.md#2-teknologivalg)
- [Azure SQL Database Documentation](https://learn.microsoft.com/en-us/azure/azure-sql/database/)
- [Azure Key Vault Documentation](https://learn.microsoft.com/en-us/azure/key-vault/)
- [Azure CLI SQL Commands](https://learn.microsoft.com/en-us/cli/azure/sql)
