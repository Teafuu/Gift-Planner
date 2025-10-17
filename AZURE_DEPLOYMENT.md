# Azure Deployment Guide - Family Gift Planner

This guide walks you through deploying your .NET Aspire application to Azure.

## Prerequisites

1. **Azure CLI** installed
   ```bash
   # Check if installed
   az --version

   # If not installed, download from: https://aka.ms/installazurecliwindows
   ```

2. **Azure Developer CLI (azd)** installed
   ```bash
   # Check if installed
   azd version

   # If not installed:
   winget install microsoft.azd
   ```

3. **Azure Subscription**
   - You need an active Azure subscription
   - Sign in: `az login`

## Option 1: Deploy with Azure Developer CLI (Recommended)

This is the easiest method for .NET Aspire apps.

### Step 1: Initialize azd

```bash
cd D:\Repos\Gift-Planner
azd init
```

When prompted:
- **Environment name**: Choose something like `giftplanner-prod` or `giftplanner-dev`
- **Use code in current directory**: Yes

### Step 2: Configure Azure resources

```bash
# Set your preferred Azure region (choose one close to you)
azd env set AZURE_LOCATION eastus

# Optional: Set a specific subscription if you have multiple
az account set --subscription "Your Subscription Name"
```

### Step 3: Provision and Deploy

```bash
# This single command will:
# - Create an Azure Resource Group
# - Provision Azure Cosmos DB
# - Provision Azure Container Apps for your web app
# - Deploy your application
# - Configure all connections automatically

azd up
```

This will take 5-10 minutes. When complete, it will output your app's URL.

### Step 4: View your app

```bash
# Open the deployed app in browser
azd show

# Or manually navigate to the URL from the output
```

## Option 2: Manual Deployment with Visual Studio

1. **Right-click** on `ChristmasGiftCollection.AppHost` project
2. Select **Publish...**
3. Choose **Azure Container Apps**
4. Follow the wizard to:
   - Select your Azure subscription
   - Create or select a resource group
   - Configure the region
   - Deploy

## What Gets Deployed

### Resources Created in Azure:

1. **Azure Container Apps Environment**
   - Hosts your Blazor web application
   - Fully managed, serverless container hosting

2. **Azure Cosmos DB Account**
   - NoSQL database for event sourcing
   - Automatically provisioned and configured
   - Connection strings handled by Aspire

3. **Azure Container Registry**
   - Stores your Docker images
   - Automatically created by azd

4. **Managed Identity**
   - Secure authentication between services
   - No connection strings needed in production

## Configuration Changes for Production

Your application is already configured for production! The AppHost.cs handles both environments:

```csharp
if (builder.Environment.EnvironmentName == "Development")
{
    // Uses local Docker emulator
    cosmosDb.RunAsEmulator(...)
}
// In production/Azure:
// - Provisions real Azure Cosmos DB
// - Configures connection automatically
// - Uses managed identity for auth
```

## Monitoring and Management

### View Application Logs

```bash
# Stream logs from your deployed app
azd logs
```

### View Resources in Azure Portal

```bash
# Open resource group in Azure Portal
az group show --name rg-giftplanner-prod --output table
```

Or visit: https://portal.azure.com

### Update Your Deployment

After making code changes:

```bash
# Deploy updates
azd deploy
```

## Cost Estimation

Approximate monthly costs for low-traffic family app:

- **Azure Container Apps**: ~$15-20/month (consumption tier)
- **Azure Cosmos DB**: ~$25-30/month (serverless mode)
- **Azure Container Registry**: ~$5/month
- **Total**: ~$45-55/month

### Cost Optimization Tips

1. **Use Cosmos DB Serverless**
   - Pay only for what you use
   - Perfect for family apps with low traffic

2. **Scale to Zero**
   - Container Apps can scale to 0 when not in use
   - You'll only pay when someone uses the app

3. **Development Environment**
   - Create separate `dev` environment with minimal resources
   - Use `azd down` to delete when not needed

## Troubleshooting

### Check deployment status
```bash
azd show
```

### View detailed logs
```bash
azd logs --follow
```

### Check Cosmos DB connection
```bash
# List Cosmos DB accounts
az cosmosdb list --output table

# Get connection string (if needed)
az cosmosdb keys list --name <cosmos-account-name> --resource-group <resource-group-name>
```

### Common Issues

**1. Deployment fails with "Resource already exists"**
   - Use a different environment name: `azd init` and choose new name

**2. "Insufficient permissions"**
   - Ensure you have Contributor role on subscription: `az role assignment list --assignee <your-email>`

**3. App shows 503 errors**
   - Container might be starting up (takes 1-2 minutes)
   - Check logs: `azd logs`

## Clean Up Resources

When you want to delete everything:

```bash
# Delete all Azure resources for this environment
azd down
```

This will delete:
- Resource Group
- All resources inside (Cosmos DB, Container Apps, etc.)
- Saved environment configuration

## Custom Domain (Optional)

To use your own domain (e.g., `gifts.yourfamily.com`):

1. **In Azure Portal**, navigate to your Container App
2. Go to **Custom domains**
3. Follow the wizard to add your domain
4. Update DNS records as instructed

## Environment Variables

If you need to add environment variables for production:

```bash
# Set an environment variable
azd env set MY_VARIABLE "my-value"

# Redeploy to apply changes
azd deploy
```

## Next Steps

1. **Set up CI/CD**: Use GitHub Actions or Azure DevOps for automated deployments
2. **Enable Application Insights**: Monitor performance and errors
3. **Configure backup**: Set up Cosmos DB backup policy
4. **Add authentication**: Consider adding Azure AD B2C for additional security

## Useful Commands Reference

```bash
# Initialize new environment
azd init

# Deploy everything
azd up

# Deploy code changes only
azd deploy

# Provision infrastructure only
azd provision

# View deployment info
azd show

# Stream logs
azd logs --follow

# Delete everything
azd down

# List environments
azd env list

# Switch environment
azd env select <environment-name>
```

## Support

- **Azure Developer CLI Docs**: https://learn.microsoft.com/azure/developer/azure-developer-cli/
- **.NET Aspire Deployment**: https://learn.microsoft.com/dotnet/aspire/deployment/azure/
- **Cosmos DB Docs**: https://learn.microsoft.com/azure/cosmos-db/

---

**That's it!** With `azd up`, your family gift planner will be running in Azure with production-grade infrastructure in minutes.
