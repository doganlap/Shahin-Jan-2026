# Secrets Management Guide

## Overview

This document describes how to manage secrets and sensitive configuration in the GRC System.

## Environment Variables

The GRC System uses environment variables for sensitive configuration values. These should **never** be committed to source control.

### Required Environment Variables

#### Database Connection
```bash
DB_CONNECTION_STRING="Server=your-server;Database=GrcDb;User Id=sa;Password=YourSecurePassword;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

#### JWT Authentication
```bash
JWT_SECRET_KEY="YourSecretKeyHere-MustBeAtLeast32CharactersLong-ChangeInProduction"
```

#### Application Insights (Optional but Recommended)
```bash
APPLICATIONINSIGHTS_CONNECTION_STRING="InstrumentationKey=your-key;IngestionEndpoint=https://your-region.in.applicationinsights.azure.com/"
```

#### Redis (Optional)
```bash
REDIS_CONNECTION_STRING="your-redis-connection-string"
```

#### Blob Storage (Optional)
```bash
BLOB_STORAGE_CONNECTION_STRING="DefaultEndpointsProtocol=https;AccountName=your-account;AccountKey=your-key;EndpointSuffix=core.windows.net"
```

## Configuration Methods

### 1. Environment Variables (Recommended for Production)

Set environment variables before running the application:

```bash
export DB_CONNECTION_STRING="..."
export JWT_SECRET_KEY="..."
export APPLICATIONINSIGHTS_CONNECTION_STRING="..."
```

### 2. appsettings.json with Placeholders

The configuration files use placeholders like `${DB_CONNECTION_STRING}` which are replaced at runtime:

```json
{
  "ConnectionStrings": {
    "Default": "${DB_CONNECTION_STRING}"
  },
  "Authentication": {
    "Jwt": {
      "SecretKey": "${JWT_SECRET_KEY}"
    }
  }
}
```

### 3. Azure Key Vault (Recommended for Azure Deployments)

For Azure deployments, use Azure Key Vault:

1. **Create Key Vault**:
   ```bash
   az keyvault create --name YourKeyVaultName --resource-group YourResourceGroup --location eastus
   ```

2. **Add Secrets**:
   ```bash
   az keyvault secret set --vault-name YourKeyVaultName --name "DB-ConnectionString" --value "your-connection-string"
   az keyvault secret set --vault-name YourKeyVaultName --name "JWT-SecretKey" --value "your-secret-key"
   ```

3. **Configure Application**:
   Add the following to `appsettings.json`:
   ```json
   {
     "KeyVault": {
       "VaultName": "YourKeyVaultName",
       "Enabled": true
     }
   }
   ```

4. **Install Package**:
   ```bash
   dotnet add package Azure.Extensions.AspNetCore.Configuration.Secrets
   ```

5. **Configure in Program.cs**:
   ```csharp
   if (builder.Environment.IsProduction())
   {
       var keyVaultName = builder.Configuration["KeyVault:VaultName"];
       if (!string.IsNullOrEmpty(keyVaultName))
       {
           var keyVaultUri = $"https://{keyVaultName}.vault.azure.net/";
           builder.Configuration.AddAzureKeyVault(
               new Uri(keyVaultUri),
               new DefaultAzureCredential());
       }
   }
   ```

### 4. Docker Secrets (For Docker Deployments)

For Docker deployments, use Docker secrets:

1. **Create Secrets**:
   ```bash
   echo "your-connection-string" | docker secret create db_connection_string -
   echo "your-secret-key" | docker secret create jwt_secret_key -
   ```

2. **Use in docker-compose.yml**:
   ```yaml
   services:
     api:
       secrets:
         - db_connection_string
         - jwt_secret_key
       environment:
         - DB_CONNECTION_STRING_FILE=/run/secrets/db_connection_string
         - JWT_SECRET_KEY_FILE=/run/secrets/jwt_secret_key
   
   secrets:
     db_connection_string:
       external: true
     jwt_secret_key:
       external: true
   ```

## Secrets Rotation

### Manual Rotation

1. **Update Secret**:
   - Update the secret in your secret store (Key Vault, environment variables, etc.)
   - Restart the application to pick up the new value

2. **Verify**:
   - Check application logs for successful startup
   - Verify authentication still works

### Automated Rotation (Azure Key Vault)

Azure Key Vault supports automatic secret rotation:

1. **Enable Auto-Rotation**:
   ```bash
   az keyvault secret set-attributes --vault-name YourKeyVaultName --name "DB-ConnectionString" --enable-rotation true
   ```

2. **Configure Rotation Policy**:
   - Set rotation interval (e.g., every 90 days)
   - Configure notification alerts

## Security Best Practices

1. **Never Commit Secrets**:
   - Use `.gitignore` to exclude files containing secrets
   - Use environment variables or secret stores

2. **Use Strong Secrets**:
   - JWT secret keys should be at least 32 characters
   - Use cryptographically secure random generators

3. **Limit Access**:
   - Grant access only to necessary services/users
   - Use role-based access control (RBAC)

4. **Monitor Access**:
   - Enable audit logging for secret access
   - Set up alerts for suspicious access patterns

5. **Regular Rotation**:
   - Rotate secrets regularly (recommended: every 90 days)
   - Have a rotation procedure documented

6. **Encryption at Rest**:
   - Ensure secrets are encrypted at rest in your secret store
   - Use TLS for secrets in transit

## Troubleshooting

### Secret Not Found

**Error**: `Configuration validation failed: Authentication:Jwt:SecretKey is required`

**Solution**: Ensure the environment variable is set or the secret is available in your secret store.

### Invalid Secret Format

**Error**: `Authentication:Jwt:SecretKey must be at least 32 characters long`

**Solution**: Ensure the JWT secret key is at least 32 characters long.

### Key Vault Access Denied

**Error**: `Access denied to Key Vault`

**Solution**: 
1. Verify the application identity has access to the Key Vault
2. Check RBAC assignments: `az keyvault show --name YourKeyVaultName --query properties.accessPolicies`

## References

- [Azure Key Vault Documentation](https://docs.microsoft.com/azure/key-vault/)
- [ASP.NET Core Configuration](https://docs.microsoft.com/aspnet/core/fundamentals/configuration/)
- [Docker Secrets](https://docs.docker.com/engine/swarm/secrets/)
