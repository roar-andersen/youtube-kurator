# Deployment Guide - YouTube Kurator

This guide covers the complete deployment process for YouTube Kurator to Azure Container Apps, including Azure SQL Database setup, container registry configuration, and CI/CD pipeline setup.

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Azure SQL Database Setup](#azure-sql-database-setup)
3. [Azure Container Apps Setup](#azure-container-apps-setup)
4. [Docker Build & Deployment](#docker-build--deployment)
5. [Environment Configuration](#environment-configuration)
6. [Troubleshooting](#troubleshooting)
7. [GitHub Actions CI/CD](#github-actions-cicd)

---

## Prerequisites

Before starting the deployment process, ensure you have:

- **Azure Account**: Active Azure subscription ([Get a free account](https://azure.microsoft.com/free/))
- **Azure CLI**: Installed and configured ([Install guide](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli))
- **Docker**: Installed locally ([Install guide](https://docs.docker.com/get-docker/))
- **YouTube API Key**: From [Google Cloud Console](https://console.cloud.google.com/)
- **.NET 10.0 SDK**: For running migrations ([Download](https://dotnet.microsoft.com/download))

Verify installations:
```bash
az --version
docker --version
dotnet --version
```

---

## Azure SQL Database Setup

### Step 1: Create Resource Group

Create a resource group to organize all Azure resources:

```bash
az group create \
  --name youtube-kurator-rg \
  --location norwayeast
```

**Note**: You can choose a different location (e.g., `westeurope`, `eastus`). Use `az account list-locations -o table` to see available regions.

### Step 2: Create Azure SQL Server

```bash
az sql server create \
  --name youtube-kurator-server \
  --resource-group youtube-kurator-rg \
  --admin-user sqladmin \
  --admin-password 'YOUR_SECURE_PASSWORD_HERE' \
  --location norwayeast
```

**Important Security Notes**:
- Replace `YOUR_SECURE_PASSWORD_HERE` with a strong password (min 8 characters, uppercase, lowercase, numbers, special characters)
- Save this password securely - you'll need it for migrations and Key Vault
- The server name must be globally unique - if `youtube-kurator-server` is taken, use `youtube-kurator-server-yourname`

### Step 3: Create Azure SQL Database

```bash
az sql db create \
  --server youtube-kurator-server \
  --name youtube-kurator-db \
  --resource-group youtube-kurator-rg \
  --edition Basic \
  --compute-model Serverless \
  --auto-pause-delay 60
```

**Database Tiers**:
- **Basic**: Best for development/testing (low cost)
- **Standard S0-S12**: Production workloads with predictable performance
- **Serverless**: Auto-scales and pauses when idle (cost-effective for variable workloads)

### Step 4: Configure Firewall Rules

#### Allow Azure Services

```bash
az sql server firewall-rule create \
  --server youtube-kurator-server \
  --resource-group youtube-kurator-rg \
  --name "AllowAzureServices" \
  --start-ip-address 0.0.0.0 \
  --end-ip-address 0.0.0.0
```

#### Allow Your Local IP (for running migrations)

**PowerShell**:
```powershell
$ip = (Invoke-WebRequest -Uri "https://api.ipify.org" -UseBasicParsing).Content

az sql server firewall-rule create `
  --server youtube-kurator-server `
  --resource-group youtube-kurator-rg `
  --name "LocalDevelopment" `
  --start-ip-address $ip `
  --end-ip-address $ip
```

**Bash/Linux**:
```bash
ip=$(curl -s https://api.ipify.org)

az sql server firewall-rule create \
  --server youtube-kurator-server \
  --resource-group youtube-kurator-rg \
  --name "LocalDevelopment" \
  --start-ip-address $ip \
  --end-ip-address $ip
```

### Step 5: Create Azure Key Vault

```bash
az keyvault create \
  --name youtube-kurator-kv \
  --resource-group youtube-kurator-rg \
  --location norwayeast \
  --enable-rbac-authorization false
```

**Note**: If `youtube-kurator-kv` is taken, Key Vault names must be globally unique. Try `youtube-kurator-kv-yourname`.

### Step 6: Store Secrets in Key Vault

#### Store YouTube API Key

```bash
az keyvault secret set \
  --vault-name youtube-kurator-kv \
  --name YouTubeApiKey \
  --value "YOUR_YOUTUBE_API_KEY_HERE"
```

#### Store SQL Connection String

```bash
az keyvault secret set \
  --vault-name youtube-kurator-kv \
  --name SqlConnectionString \
  --value "Server=tcp:youtube-kurator-server.database.windows.net,1433;Initial Catalog=youtube-kurator-db;Persist Security Info=False;User ID=sqladmin;Password=YOUR_SECURE_PASSWORD_HERE;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

**Replace**:
- `YOUR_SECURE_PASSWORD_HERE` with your SQL Server admin password
- `youtube-kurator-server` with your actual server name if different

### Step 7: Run Database Migrations

#### Set Connection String Environment Variable

**PowerShell**:
```powershell
$env:ConnectionStrings__DefaultConnection = "Server=tcp:youtube-kurator-server.database.windows.net,1433;Initial Catalog=youtube-kurator-db;Persist Security Info=False;User ID=sqladmin;Password=YOUR_SECURE_PASSWORD_HERE;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

**Bash/Linux**:
```bash
export ConnectionStrings__DefaultConnection="Server=tcp:youtube-kurator-server.database.windows.net,1433;Initial Catalog=youtube-kurator-db;Persist Security Info=False;User ID=sqladmin;Password=YOUR_SECURE_PASSWORD_HERE;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

#### Run Migrations

```bash
cd YouTubeKurator.Api
dotnet ef database update
```

Expected output:
```
Build started...
Build succeeded.
Applying migration '20251217202528_InitialCreate'.
Done.
```

### Step 8: Verify Database Structure

Connect to Azure SQL and verify tables exist:

```bash
# Using Azure Data Studio or SQL Server Management Studio
Server: youtube-kurator-server.database.windows.net
Database: youtube-kurator-db
Username: sqladmin
Password: YOUR_SECURE_PASSWORD_HERE
```

Expected tables:
- `Playlists`
- `CachedSearches`
- `__EFMigrationsHistory`

---

## Azure Container Apps Setup

### Step 1: Create Azure Container Registry (ACR)

```bash
az acr create \
  --resource-group youtube-kurator-rg \
  --name youtubekuratoracr \
  --sku Basic \
  --admin-enabled true
```

**Note**: ACR names must be globally unique and contain only alphanumeric characters. If taken, try `youtubekuratoracrYOURNAME`.

#### Get ACR Credentials

```bash
az acr credential show \
  --name youtubekuratoracr \
  --resource-group youtube-kurator-rg
```

Save the username and password - you'll need them for Docker login.

### Step 2: Create Container Apps Environment

```bash
az containerapp env create \
  --name youtube-kurator-env \
  --resource-group youtube-kurator-rg \
  --location norwayeast
```

This creates an isolated environment for your container apps with built-in monitoring and networking.

### Step 3: Create Managed Identity

Create a User-Assigned Managed Identity for secure Key Vault access:

```bash
az identity create \
  --name youtube-kurator-identity \
  --resource-group youtube-kurator-rg
```

#### Get Identity Details

**PowerShell**:
```powershell
$identityId = (az identity show --name youtube-kurator-identity --resource-group youtube-kurator-rg --query id -o tsv)
$principalId = (az identity show --name youtube-kurator-identity --resource-group youtube-kurator-rg --query principalId -o tsv)
```

**Bash/Linux**:
```bash
identityId=$(az identity show --name youtube-kurator-identity --resource-group youtube-kurator-rg --query id -o tsv)
principalId=$(az identity show --name youtube-kurator-identity --resource-group youtube-kurator-rg --query principalId -o tsv)
```

### Step 4: Grant Key Vault Access to Managed Identity

```bash
az keyvault set-policy \
  --name youtube-kurator-kv \
  --secret-permissions get list \
  --object-id $principalId
```

This allows the Container App to read secrets from Key Vault without storing credentials.

### Step 5: Create Container App

```bash
az containerapp create \
  --name youtube-kurator \
  --resource-group youtube-kurator-rg \
  --environment youtube-kurator-env \
  --image mcr.microsoft.com/azuredocs/containerapps-helloworld:latest \
  --target-port 80 \
  --ingress external \
  --min-replicas 1 \
  --max-replicas 3
```

**Note**: We use a placeholder image initially. We'll update it with our actual image after building.

### Step 6: Assign Managed Identity to Container App

**PowerShell**:
```powershell
az containerapp identity assign `
  --name youtube-kurator `
  --resource-group youtube-kurator-rg `
  --user-assigned $identityId
```

**Bash/Linux**:
```bash
az containerapp identity assign \
  --name youtube-kurator \
  --resource-group youtube-kurator-rg \
  --user-assigned $identityId
```

### Step 7: Configure Environment Variables with Key Vault References

Get the Key Vault URI:
```bash
az keyvault show --name youtube-kurator-kv --resource-group youtube-kurator-rg --query properties.vaultUri -o tsv
```

Configure secrets:
```bash
az containerapp secret set \
  --name youtube-kurator \
  --resource-group youtube-kurator-rg \
  --secrets \
    youtube-api-key=keyvaultref:https://youtube-kurator-kv.vault.azure.net/secrets/YouTubeApiKey,identityref:$identityId \
    sql-connection-string=keyvaultref:https://youtube-kurator-kv.vault.azure.net/secrets/SqlConnectionString,identityref:$identityId
```

Set environment variables:
```bash
az containerapp update \
  --name youtube-kurator \
  --resource-group youtube-kurator-rg \
  --set-env-vars \
    "YouTubeApi__ApiKey=secretref:youtube-api-key" \
    "ConnectionStrings__DefaultConnection=secretref:sql-connection-string"
```

### Step 8: Get Container App URL

```bash
az containerapp show \
  --name youtube-kurator \
  --resource-group youtube-kurator-rg \
  --query properties.configuration.ingress.fqdn -o tsv
```

Save this URL - this is your production endpoint.

---

## Docker Build & Deployment

### Step 1: Test Docker Build Locally

From the project root directory:

```bash
docker build -t youtube-kurator:latest .
```

### Step 2: Test Container Locally

```bash
docker run -p 8080:80 \
  -e "YouTubeApi__ApiKey=YOUR_API_KEY" \
  -e "ConnectionStrings__DefaultConnection=Data Source=youtube-kurator.db" \
  youtube-kurator:latest
```

Visit `http://localhost:8080` and verify:
- App loads successfully
- Can create playlists
- Frontend is served correctly

Press `Ctrl+C` to stop the container.

### Step 3: Login to Azure Container Registry

```bash
az acr login --name youtubekuratoracr
```

Alternative using Docker:
```bash
docker login youtubekuratoracr.azurecr.io -u USERNAME -p PASSWORD
```

### Step 4: Tag Image for ACR

```bash
docker tag youtube-kurator:latest youtubekuratoracr.azurecr.io/youtube-kurator:latest
```

### Step 5: Push Image to ACR

```bash
docker push youtubekuratoracr.azurecr.io/youtube-kurator:latest
```

Expected output:
```
The push refers to repository [youtubekuratoracr.azurecr.io/youtube-kurator]
...
latest: digest: sha256:... size: 1234
```

### Step 6: Update Container App with New Image

```bash
az containerapp update \
  --name youtube-kurator \
  --resource-group youtube-kurator-rg \
  --image youtubekuratoracr.azurecr.io/youtube-kurator:latest
```

### Step 7: Verify Deployment

#### Check Provisioning State

```bash
az containerapp show \
  --name youtube-kurator \
  --resource-group youtube-kurator-rg \
  --query properties.provisioningState -o tsv
```

Should return: `Succeeded`

#### Check Running Replicas

```bash
az containerapp replica list \
  --name youtube-kurator \
  --resource-group youtube-kurator-rg \
  --output table
```

#### View Logs

```bash
az containerapp logs show \
  --name youtube-kurator \
  --resource-group youtube-kurator-rg \
  --follow
```

Press `Ctrl+C` to stop following logs.

### Step 8: Test Production Application

Get the app URL:
```bash
az containerapp show \
  --name youtube-kurator \
  --resource-group youtube-kurator-rg \
  --query properties.configuration.ingress.fqdn -o tsv
```

Test checklist:
- [ ] App loads at HTTPS URL
- [ ] Can create a new playlist
- [ ] Can edit playlist name and search query
- [ ] Can refresh videos (YouTube API works)
- [ ] Videos display with thumbnails
- [ ] Can click video to open on YouTube
- [ ] Can delete playlist
- [ ] PWA manifest loads (`/manifest.json`)
- [ ] Service Worker registers
- [ ] Can install as PWA on mobile

---

## Environment Configuration

### Environment Variables Overview

| Variable | Source | Description |
|----------|--------|-------------|
| `YouTubeApi__ApiKey` | Key Vault | YouTube Data API v3 key |
| `ConnectionStrings__DefaultConnection` | Key Vault | Azure SQL connection string |
| `ASPNETCORE_ENVIRONMENT` | Direct | Environment name (Production/Development) |
| `ASPNETCORE_URLS` | Dockerfile | HTTP port binding (http://+:80) |

### Connection String Format

**Azure SQL Database**:
```
Server=tcp:youtube-kurator-server.database.windows.net,1433;Initial Catalog=youtube-kurator-db;Persist Security Info=False;User ID=sqladmin;Password=PASSWORD;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

**SQLite (Local Development)**:
```
Data Source=youtube-kurator.db
```

### Scaling Configuration

#### Manual Scaling

```bash
az containerapp update \
  --name youtube-kurator \
  --resource-group youtube-kurator-rg \
  --min-replicas 1 \
  --max-replicas 5
```

#### Auto-scaling Rules

Based on HTTP requests:
```bash
az containerapp update \
  --name youtube-kurator \
  --resource-group youtube-kurator-rg \
  --scale-rule-name http-rule \
  --scale-rule-type http \
  --scale-rule-http-concurrency 100
```

Based on CPU:
```bash
az containerapp update \
  --name youtube-kurator \
  --resource-group youtube-kurator-rg \
  --scale-rule-name cpu-rule \
  --scale-rule-type cpu \
  --scale-rule-metadata "type=Utilization" "value=75"
```

---

## Troubleshooting

### Database Connection Issues

**Error**: "Cannot open server 'youtube-kurator-server' requested by the login"

**Solutions**:
1. Verify firewall rules allow your IP:
   ```bash
   az sql server firewall-rule list \
     --server youtube-kurator-server \
     --resource-group youtube-kurator-rg
   ```

2. Ensure Azure services are allowed (0.0.0.0 rule exists)

3. Check connection string format matches Azure SQL requirements

### Container App Won't Start

**Error**: "Container terminated with exit code 1"

**Solutions**:

1. Check logs for errors:
   ```bash
   az containerapp logs show \
     --name youtube-kurator \
     --resource-group youtube-kurator-rg \
     --tail 100
   ```

2. Verify environment variables are set:
   ```bash
   az containerapp show \
     --name youtube-kurator \
     --resource-group youtube-kurator-rg \
     --query properties.template.containers[0].env
   ```

3. Test Docker image locally first

4. Verify Key Vault permissions:
   ```bash
   az keyvault show-deleted-secret \
     --vault-name youtube-kurator-kv \
     --name YouTubeApiKey
   ```

### YouTube API Quota Exceeded

**Error**: "quotaExceeded" in logs

**Solutions**:
1. Check quota usage in [Google Cloud Console](https://console.cloud.google.com/apis/api/youtube.googleapis.com/quotas)
2. Each search costs ~100 quota units (10,000 daily limit = ~100 searches)
3. Caching reduces API calls - verify cache is working
4. Request quota increase or upgrade to paid plan
5. Wait until quota resets (midnight Pacific Time)

### Key Vault Access Denied

**Error**: "The user, group or application does not have secrets get permission"

**Solutions**:
1. Verify managed identity has access:
   ```bash
   az keyvault show \
     --name youtube-kurator-kv \
     --resource-group youtube-kurator-rg \
     --query properties.accessPolicies
   ```

2. Re-grant permissions:
   ```bash
   az keyvault set-policy \
     --name youtube-kurator-kv \
     --secret-permissions get list \
     --object-id $(az identity show --name youtube-kurator-identity --resource-group youtube-kurator-rg --query principalId -o tsv)
   ```

### Service Worker Not Registering

**Error**: "Failed to register a ServiceWorker"

**Solutions**:
1. Service Workers require HTTPS - verify app uses HTTPS URL
2. Check browser console for errors
3. Verify `/sw.js` is accessible at root
4. Clear browser cache and retry
5. Test in incognito/private mode

### Docker Push Fails

**Error**: "unauthorized: authentication required"

**Solutions**:
1. Login to ACR:
   ```bash
   az acr login --name youtubekuratoracr
   ```

2. Verify ACR admin is enabled:
   ```bash
   az acr update --name youtubekuratoracr --admin-enabled true
   ```

3. Use ACR credentials:
   ```bash
   az acr credential show --name youtubekuratoracr
   ```

---

## GitHub Actions CI/CD

### Prerequisites

1. GitHub repository for your code
2. Azure Service Principal for authentication

### Create Service Principal

```bash
az ad sp create-for-rbac \
  --name youtube-kurator-sp \
  --role contributor \
  --scopes /subscriptions/YOUR_SUBSCRIPTION_ID/resourceGroups/youtube-kurator-rg \
  --sdk-auth
```

Save the JSON output - you'll need it for GitHub Secrets.

### Configure GitHub Secrets

In your GitHub repository, go to **Settings > Secrets and variables > Actions**, and add:

| Secret Name | Value |
|-------------|-------|
| `AZURE_CREDENTIALS` | JSON output from service principal creation |
| `REGISTRY_LOGIN_SERVER` | `youtubekuratoracr.azurecr.io` |
| `REGISTRY_USERNAME` | From `az acr credential show` |
| `REGISTRY_PASSWORD` | From `az acr credential show` |
| `RESOURCE_GROUP` | `youtube-kurator-rg` |
| `CONTAINER_APP_NAME` | `youtube-kurator` |

### Create Workflow File

Create `.github/workflows/deploy.yml`:

```yaml
name: Build and Deploy to Azure Container Apps

on:
  push:
    branches: [ main ]
  workflow_dispatch:

env:
  IMAGE_NAME: youtube-kurator

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest

    steps:
    - name: Checkout code
      uses: actions/checkout@v4

    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '10.0.x'

    - name: Restore dependencies
      run: dotnet restore YouTubeKurator.Api/YouTubeKurator.Api.csproj

    - name: Build
      run: dotnet build YouTubeKurator.Api/YouTubeKurator.Api.csproj --configuration Release --no-restore

    - name: Test
      run: dotnet test YouTubeKurator.Tests/YouTubeKurator.Tests.csproj --no-restore --verbosity normal

    - name: Login to Azure Container Registry
      uses: docker/login-action@v3
      with:
        registry: ${{ secrets.REGISTRY_LOGIN_SERVER }}
        username: ${{ secrets.REGISTRY_USERNAME }}
        password: ${{ secrets.REGISTRY_PASSWORD }}

    - name: Build and push Docker image
      uses: docker/build-push-action@v5
      with:
        context: .
        push: true
        tags: ${{ secrets.REGISTRY_LOGIN_SERVER }}/${{ env.IMAGE_NAME }}:latest,${{ secrets.REGISTRY_LOGIN_SERVER }}/${{ env.IMAGE_NAME }}:${{ github.sha }}

    - name: Azure Login
      uses: azure/login@v1
      with:
        creds: ${{ secrets.AZURE_CREDENTIALS }}

    - name: Deploy to Container Apps
      uses: azure/container-apps-deploy-action@v1
      with:
        containerAppName: ${{ secrets.CONTAINER_APP_NAME }}
        resourceGroup: ${{ secrets.RESOURCE_GROUP }}
        imageToDeploy: ${{ secrets.REGISTRY_LOGIN_SERVER }}/${{ env.IMAGE_NAME }}:${{ github.sha }}
```

### Verify CI/CD Pipeline

1. Commit and push changes to `main` branch
2. Go to **Actions** tab in GitHub
3. Watch the workflow run
4. Verify deployment succeeded
5. Test the live application

---

## Cost Optimization

### Azure SQL Database

- Use **Serverless** tier for development (auto-pauses when idle)
- Use **Basic** tier for low-traffic production
- Consider **Elastic Pool** if running multiple databases

### Container Apps

- Set `--min-replicas 0` to scale to zero when idle (saves costs)
- Use `--max-replicas 3` to limit maximum scale
- Monitor metrics to right-size container resources

### Container Registry

- Use **Basic** SKU for low-traffic scenarios
- Enable **geo-replication** only if needed

### Key Vault

- Standard tier is sufficient for most scenarios
- Avoid Premium unless HSM-backed keys are required

### Estimated Monthly Costs (Norway East)

| Resource | Tier | Estimated Cost |
|----------|------|---------------|
| Azure SQL Database | Serverless (1 vCore) | $50-150/month |
| Container Apps | 1-3 replicas | $20-60/month |
| Container Registry | Basic | $5/month |
| Key Vault | Standard | $0.03/10k ops |
| **Total** | | **$75-215/month** |

---

## Security Best Practices

1. **Never commit secrets** - Use Key Vault for all sensitive data
2. **Use Managed Identity** - Avoid storing credentials in code
3. **Enable HTTPS only** - Container Apps supports automatic HTTPS
4. **Restrict firewall rules** - Only allow necessary IPs to SQL Database
5. **Rotate secrets regularly** - Update API keys and passwords periodically
6. **Enable monitoring** - Use Application Insights for security alerts
7. **Use RBAC** - Grant minimal required permissions

---

## Monitoring & Logging

### View Container App Metrics

```bash
az containerapp show \
  --name youtube-kurator \
  --resource-group youtube-kurator-rg \
  --query properties.template.containers[0].resources
```

### Stream Live Logs

```bash
az containerapp logs show \
  --name youtube-kurator \
  --resource-group youtube-kurator-rg \
  --follow
```

### Enable Application Insights

```bash
# Create Application Insights
az monitor app-insights component create \
  --app youtube-kurator-insights \
  --location norwayeast \
  --resource-group youtube-kurator-rg

# Get instrumentation key
az monitor app-insights component show \
  --app youtube-kurator-insights \
  --resource-group youtube-kurator-rg \
  --query instrumentationKey -o tsv

# Add to Container App
az containerapp update \
  --name youtube-kurator \
  --resource-group youtube-kurator-rg \
  --set-env-vars "APPLICATIONINSIGHTS_CONNECTION_STRING=InstrumentationKey=YOUR_KEY"
```

---

## Cleanup Resources

To delete all Azure resources and stop incurring costs:

```bash
az group delete --name youtube-kurator-rg --yes --no-wait
```

**Warning**: This permanently deletes:
- SQL Database and all data
- Container Apps
- Container Registry and images
- Key Vault and secrets
- All other resources in the resource group

---

## Next Steps

After successful deployment:

1. **Configure Custom Domain** - Map your own domain to Container App
2. **Setup SSL Certificate** - Add custom SSL for branded domain
3. **Enable Authentication** - Add Azure AD or other auth providers
4. **Configure Backup** - Setup automated database backups
5. **Implement Monitoring** - Add Application Insights alerts
6. **Performance Tuning** - Optimize SQL queries and caching
7. **Load Testing** - Verify app handles expected traffic

---

## References

- [Azure Container Apps Documentation](https://learn.microsoft.com/en-us/azure/container-apps/)
- [Azure SQL Database Documentation](https://learn.microsoft.com/en-us/azure/azure-sql/database/)
- [Azure Key Vault Documentation](https://learn.microsoft.com/en-us/azure/key-vault/)
- [Docker Documentation](https://docs.docker.com/)
- [YouTube Data API v3](https://developers.google.com/youtube/v3)

---

**Document Version**: 1.0
**Last Updated**: 2025-12-18
**Maintained By**: YouTube Kurator Team
