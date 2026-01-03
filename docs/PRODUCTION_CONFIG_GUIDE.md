# Production Configuration Guide

## Overview
This guide addresses critical production runtime issues and provides step-by-step solutions for deploying the GRC System to production.

## Critical Issues Summary

| Issue | Severity | Impact | File Location |
|-------|----------|--------|---------------|
| Database Connection String | 🔴 Critical | Application won't start | appsettings.Production.json:10, HttpApi.Host/appsettings.Production.json:29 |
| CORS Configuration | 🔴 Critical | All API requests will fail | appsettings.Production.json:16-18, HttpApi.Host/appsettings.Production.json:34-36 |
| AllowedHosts | 🔴 Critical | Requests rejected | HttpApi.Host/appsettings.Production.json:27 |
| JWT Secret Key | 🟡 High | Security vulnerability | appsettings.Production.json:23, HttpApi.Host/appsettings.Production.json:53 |
| Application Insights | 🟢 Medium | No telemetry/monitoring | HttpApi.Host/appsettings.Production.json:59 |

---

## Issue #1: Database Connection String

### Problem
Two configuration files have invalid/placeholder connection strings:

**Root appsettings.Production.json:10**
```json
"Default": "Server=PRODUCTION_SERVER;Database=GrcDb;Trusted_Connection=True;..."
```
- `PRODUCTION_SERVER` is a placeholder (will fail DNS lookup)
- `Trusted_Connection=True` requires Windows Authentication (typically not available in production)

**HttpApi.Host/appsettings.Production.json:29**
```json
"Default": "Server=localhost;Database=GrcDb;Trusted_Connection=True;..."
```
- `localhost` points to local machine (incorrect for production)

### Solution Options

#### Option 1: Use Environment Variables (Recommended for Docker/Kubernetes)
```bash
export ConnectionStrings__Default="Server=your-sql-server.database.windows.net;Database=GrcDb;User Id=grc_app_user;Password=YourSecurePassword123!;TrustServerCertificate=True;MultipleActiveResultSets=true;Encrypt=True"
```

#### Option 2: Update Configuration Files (For Traditional Hosting)

**For Azure SQL Database:**
```json
{
  "ConnectionStrings": {
    "Default": "Server=tcp:your-server.database.windows.net,1433;Initial Catalog=GrcDb;Persist Security Info=False;User ID=grc_app_user;Password=YourSecurePassword123!;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  }
}
```

**For On-Premise SQL Server:**
```json
{
  "ConnectionStrings": {
    "Default": "Server=192.168.1.100,1433;Database=GrcDb;User Id=sa;Password=YourStrong@Passw0rd123!;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

**For AWS RDS SQL Server:**
```json
{
  "ConnectionStrings": {
    "Default": "Server=your-db.xxxxxx.us-east-1.rds.amazonaws.com,1433;Database=GrcDb;User Id=admin;Password=YourSecurePassword123!;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

### Validation
The application validates the connection string exists at startup ([Program.cs:22-26](../src/Grc.HttpApi.Host/Program.cs#L22-L26)):
```csharp
var connectionString = _configuration.GetConnectionString("Default");
if (string.IsNullOrWhiteSpace(connectionString))
{
    errors.Add("ConnectionStrings:Default is required");
}
```

---

## Issue #2: CORS Configuration

### Problem
CORS origins contain placeholders that will block legitimate requests:

**appsettings.Production.json:16-18**
```json
"AllowedOrigins": [
  "https://grc.yourdomain.com"  // Placeholder domain
]
```

**HttpApi.Host/appsettings.Production.json:34-36**
```json
"AllowedOrigins": [
  "https://yourdomain.com",      // Placeholder domain
  "https://api.yourdomain.com"   // Placeholder domain
]
```

### Impact
All browser requests from your actual domain will receive CORS errors:
```
Access to fetch at 'https://api.yoursite.com' from origin 'https://yoursite.com' has been blocked by CORS policy
```

### Solution

#### For Production Deployment
Update BOTH configuration files with your actual domains:

```json
{
  "Cors": {
    "AllowedOrigins": [
      "https://grc.yourcompany.com",
      "https://yourcompany.com",
      "https://www.yourcompany.com"
    ],
    "AllowedMethods": ["GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS"],
    "AllowedHeaders": ["*"],
    "AllowCredentials": true
  }
}
```

#### Using Environment Variables (Docker/Kubernetes)
```bash
export Cors__AllowedOrigins__0="https://grc.yourcompany.com"
export Cors__AllowedOrigins__1="https://yourcompany.com"
export Cors__AllowedOrigins__2="https://www.yourcompany.com"
```

Or in docker-compose.yml:
```yaml
environment:
  - Cors__AllowedOrigins__0=https://grc.yourcompany.com
  - Cors__AllowedOrigins__1=https://yourcompany.com
  - Cors__AllowedOrigins__2=https://www.yourcompany.com
```

### Testing CORS
```bash
# Test CORS preflight
curl -X OPTIONS https://api.yoursite.com/api/health \
  -H "Origin: https://yoursite.com" \
  -H "Access-Control-Request-Method: GET" \
  -v
```

Expected response should include:
```
Access-Control-Allow-Origin: https://yoursite.com
Access-Control-Allow-Methods: GET, POST, PUT, DELETE, PATCH, OPTIONS
Access-Control-Allow-Credentials: true
```

---

## Issue #3: AllowedHosts

### Problem
```json
"AllowedHosts": "yourdomain.com"  // Placeholder in HttpApi.Host/appsettings.Production.json:27
```

This restricts which Host headers are accepted. With a placeholder, ALL requests will be rejected with `400 Bad Request`.

### Solution
```json
{
  "AllowedHosts": "grc.yourcompany.com;api.yourcompany.com;yourcompany.com"
}
```

Or allow all (not recommended for production):
```json
{
  "AllowedHosts": "*"
}
```

---

## Issue #4: JWT Secret Key

### Current State
Both configuration files contain hardcoded JWT secrets:
- Root: `GrcSystemProductionSecretKeyMustBeAtLeast32CharactersLong2026`
- HttpApi.Host: `GrcSystemProductionSecretKeyMustBeAtLeast32CharactersLong2026ReplaceInProd`

### Security Risk
- Keys are visible in source control
- Same key across environments
- Compromised key allows token forgery

### Solution: Use Environment Variables

#### Step 1: Generate Secure Key
```bash
# Linux/Mac - Generate 64-character random key
openssl rand -base64 48

# Windows PowerShell
$bytes = New-Object byte[] 48
[Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
[Convert]::ToBase64String($bytes)
```

Example output: `k8n3h9j2m5p7q1r4s6t8v0w2x4y6z8a1b3c5d7e9f1g3h5j7k9m1n3p5q7r9s1t3u5`

#### Step 2: Store Securely

**For Docker/Docker Compose:**
```bash
# Create .env file (add to .gitignore!)
echo "JWT_SECRET_KEY=k8n3h9j2m5p7q1r4s6t8v0w2x4y6z8a1b3c5d7e9f1g3h5j7k9m1n3p5q7r9s1t3u5" >> .env
```

Update docker-compose.yml:
```yaml
services:
  api:
    environment:
      - Authentication__Jwt__SecretKey=${JWT_SECRET_KEY}
```

**For Kubernetes:**
```bash
# Create Kubernetes secret
kubectl create secret generic grc-jwt-secret \
  --from-literal=secret-key='k8n3h9j2m5p7q1r4s6t8v0w2x4y6z8a1b3c5d7e9f1g3h5j7k9m1n3p5q7r9s1t3u5'
```

Deployment YAML:
```yaml
env:
  - name: Authentication__Jwt__SecretKey
    valueFrom:
      secretKeyRef:
        name: grc-jwt-secret
        key: secret-key
```

**For Azure App Service:**
```bash
az webapp config appsettings set \
  --name your-app-name \
  --resource-group your-rg \
  --settings Authentication__Jwt__SecretKey='your-secret-key'
```

**For Traditional IIS/Windows:**
```powershell
# Set machine-level environment variable
[Environment]::SetEnvironmentVariable("Authentication__Jwt__SecretKey", "your-secret-key", "Machine")
```

#### Step 3: Remove from appsettings.Production.json
```json
{
  "Authentication": {
    "Jwt": {
      // SecretKey removed - set via environment variable
      "Issuer": "GrcSystem",
      "Audience": "GrcSystem",
      "Expiration": 3600
    }
  }
}
```

---

## Issue #5: Application Insights (Monitoring)

### Current State
```json
"ApplicationInsights": {
  "ConnectionString": "",  // Empty
}
```

### Impact
- No telemetry/logging in production
- Cannot monitor performance, errors, or usage
- No alerting capabilities

### Solution

#### Option 1: Azure Application Insights (Recommended)

**Step 1: Create Application Insights Resource**
```bash
# Using Azure CLI
az monitor app-insights component create \
  --app grc-system-prod \
  --location eastus \
  --resource-group your-rg \
  --application-type web

# Get connection string
az monitor app-insights component show \
  --app grc-system-prod \
  --resource-group your-rg \
  --query connectionString -o tsv
```

**Step 2: Configure Application**
```json
{
  "ApplicationInsights": {
    "ConnectionString": "InstrumentationKey=12345678-1234-1234-1234-123456789012;IngestionEndpoint=https://eastus-1.in.applicationinsights.azure.com/;LiveEndpoint=https://eastus.livediagnostics.monitor.azure.com/",
    "EnableAdaptiveSampling": true,
    "EnablePerformanceCounterCollectionModule": true
  }
}
```

Or via environment variable:
```bash
export ApplicationInsights__ConnectionString="InstrumentationKey=...;IngestionEndpoint=..."
```

#### Option 2: Self-Hosted Monitoring (Seq, Elasticsearch, etc.)

**For Seq:**
```bash
# Run Seq in Docker
docker run -d --name seq \
  -e ACCEPT_EULA=Y \
  -p 5341:80 \
  datalust/seq:latest
```

Update Serilog configuration in appsettings.Production.json:
```json
{
  "Serilog": {
    "WriteTo": [
      {
        "Name": "Seq",
        "Args": {
          "serverUrl": "http://seq:5341"
        }
      }
    ]
  }
}
```

---

## Complete Production Configuration Example

### File: src/Grc.HttpApi.Host/appsettings.Production.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Error"
    }
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Warning",
      "Override": {
        "Microsoft": "Error",
        "System": "Error"
      }
    }
  },
  "Environment": {
    "Name": "Production"
  },
  "RateLimiting": {
    "Enabled": true,
    "PermitLimit": 100,
    "Window": "00:01:00",
    "QueueLimit": 0
  },
  "AllowedHosts": "grc.yourcompany.com;api.yourcompany.com;yourcompany.com",
  "ConnectionStrings": {
    "Default": "Server=your-db-server;Database=GrcDb;User Id=grc_app;Password=${DB_PASSWORD};TrustServerCertificate=True;MultipleActiveResultSets=true",
    "Redis": "${REDIS_CONNECTION_STRING}",
    "BlobStorage": "${BLOB_STORAGE_CONNECTION_STRING}"
  },
  "Cors": {
    "AllowedOrigins": [
      "https://grc.yourcompany.com",
      "https://yourcompany.com",
      "https://www.yourcompany.com"
    ],
    "AllowedMethods": ["GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS"],
    "AllowedHeaders": ["*"],
    "AllowCredentials": true
  },
  "Authentication": {
    "Jwt": {
      // SecretKey from environment: Authentication__Jwt__SecretKey
      "Issuer": "GrcSystem",
      "Audience": "GrcSystem",
      "Expiration": 3600
    }
  },
  "ApplicationInsights": {
    "ConnectionString": "${APP_INSIGHTS_CONNECTION_STRING}",
    "EnableAdaptiveSampling": true,
    "EnablePerformanceCounterCollectionModule": true
  },
  "HealthChecks": {
    "Enabled": true,
    "Path": "/health",
    "DetailedOutput": false
  },
  "Swagger": {
    "Enabled": false
  }
}
```

### Environment Variables (.env file)
```bash
# Database
DB_PASSWORD=YourSecureDbPassword123!
ConnectionStrings__Default=Server=prod-sql.yourcompany.com;Database=GrcDb;User Id=grc_app;Password=${DB_PASSWORD};TrustServerCertificate=True;MultipleActiveResultSets=true

# JWT Authentication
Authentication__Jwt__SecretKey=your-generated-64-character-secret-key-here-use-openssl-rand

# Optional: Redis Cache
REDIS_CONNECTION_STRING=prod-redis.yourcompany.com:6379,password=your-redis-password

# Optional: Azure Blob Storage
BLOB_STORAGE_CONNECTION_STRING=DefaultEndpointsProtocol=https;AccountName=grcprodstg;AccountKey=...

# Monitoring
APP_INSIGHTS_CONNECTION_STRING=InstrumentationKey=12345678-1234-1234-1234-123456789012;IngestionEndpoint=https://eastus-1.in.applicationinsights.azure.com/

# CORS
Cors__AllowedOrigins__0=https://grc.yourcompany.com
Cors__AllowedOrigins__1=https://yourcompany.com
Cors__AllowedOrigins__2=https://www.yourcompany.com
```

---

## Pre-Deployment Validation Checklist

Run this checklist before deploying to production:

### 1. Configuration Validation
```bash
# Check for placeholder values
grep -r "yourdomain" src/Grc.HttpApi.Host/appsettings.Production.json
grep -r "PRODUCTION_SERVER" appsettings.Production.json
grep -r "localhost" src/Grc.HttpApi.Host/appsettings.Production.json

# Should return no results - if found, update them!
```

### 2. Database Connectivity
```bash
# Test database connection
dotnet ef database list-migrations --project src/Grc.EntityFrameworkCore

# Expected: List of migrations, no errors
```

### 3. JWT Configuration
```bash
# Verify JWT secret is set
echo $Authentication__Jwt__SecretKey | wc -c
# Should be >= 32 characters

# Verify it's not the default placeholder
if echo $Authentication__Jwt__SecretKey | grep -q "GrcSystem"; then
  echo "ERROR: Using default JWT secret!"
fi
```

### 4. CORS Testing
```bash
# Test CORS from expected origin
curl -X OPTIONS https://your-api.com/api/health \
  -H "Origin: https://your-frontend.com" \
  -H "Access-Control-Request-Method: GET" \
  -v
```

### 5. Health Checks
```bash
# Test application health
curl https://your-api.com/health
# Expected: {"status":"Healthy"}

curl https://your-api.com/health/ready
curl https://your-api.com/health/live
```

---

## Deployment Instructions

### Docker Deployment

**Step 1: Update docker-compose.yml**
```yaml
services:
  api:
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__Default=${DB_CONNECTION_STRING}
      - Authentication__Jwt__SecretKey=${JWT_SECRET_KEY}
      - Cors__AllowedOrigins__0=https://grc.yourcompany.com
      - ApplicationInsights__ConnectionString=${APP_INSIGHTS_CONNECTION_STRING}
```

**Step 2: Create .env file**
```bash
cat > .env << EOF
DB_CONNECTION_STRING=Server=your-db;Database=GrcDb;User Id=sa;Password=YourPassword!;TrustServerCertificate=True
JWT_SECRET_KEY=$(openssl rand -base64 48)
APP_INSIGHTS_CONNECTION_STRING=InstrumentationKey=...
EOF

chmod 600 .env  # Secure the file
```

**Step 3: Deploy**
```bash
./deploy.sh
```

### Kubernetes Deployment

**Step 1: Create Secrets**
```bash
kubectl create secret generic grc-config \
  --from-literal=db-connection-string='Server=...' \
  --from-literal=jwt-secret='your-secret-key' \
  --from-literal=app-insights='InstrumentationKey=...'
```

**Step 2: Apply Deployment**
```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: grc-api
spec:
  template:
    spec:
      containers:
      - name: api
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: ConnectionStrings__Default
          valueFrom:
            secretKeyRef:
              name: grc-config
              key: db-connection-string
        - name: Authentication__Jwt__SecretKey
          valueFrom:
            secretKeyRef:
              name: grc-config
              key: jwt-secret
```

### Azure App Service Deployment

**Step 1: Configure App Settings**
```bash
az webapp config appsettings set \
  --name grc-api-prod \
  --resource-group grc-prod-rg \
  --settings \
    ASPNETCORE_ENVIRONMENT=Production \
    ConnectionStrings__Default='Server=...' \
    Authentication__Jwt__SecretKey='your-secret' \
    Cors__AllowedOrigins__0='https://grc.yourcompany.com'
```

**Step 2: Deploy**
```bash
dotnet publish -c Release
cd src/Grc.HttpApi.Host/bin/Release/net8.0/publish
az webapp deployment source config-zip \
  --resource-group grc-prod-rg \
  --name grc-api-prod \
  --src ./deploy.zip
```

---

## Security Best Practices

### 1. Secrets Management
- ✅ Use environment variables for secrets
- ✅ Use Azure Key Vault, AWS Secrets Manager, or HashiCorp Vault
- ❌ Never commit secrets to source control
- ❌ Never hardcode connection strings

### 2. Database Security
- ✅ Use dedicated application user (not 'sa')
- ✅ Use strong passwords (>= 16 characters, mixed case, symbols)
- ✅ Enable SSL/TLS for database connections
- ✅ Restrict database firewall rules to application IPs only

### 3. JWT Security
- ✅ Use 64+ character secret keys
- ✅ Rotate keys periodically (every 90 days)
- ✅ Use short expiration times (1 hour recommended)
- ✅ Implement token refresh mechanism

### 4. CORS Security
- ✅ Explicitly list allowed origins (avoid wildcards)
- ✅ Use HTTPS-only origins in production
- ✅ Review CORS settings regularly

---

## Troubleshooting

### Application Won't Start

**Error: "Configuration validation failed"**
```
Check: src/Grc.HttpApi.Host/Program.cs:24-35
Solution: Review ConfigurationValidator output for missing/invalid settings
```

**Error: "Cannot open database 'GrcDb'"**
```
Check: Connection string in environment variables or appsettings
Solution: Verify server name, credentials, and network connectivity
Test: dotnet ef database list-migrations
```

### CORS Errors

**Error: "blocked by CORS policy"**
```
Check: Cors:AllowedOrigins in appsettings.Production.json
Test: curl with Origin header (see CORS Testing section)
Solution: Add your frontend domain to AllowedOrigins
```

### Authentication Errors

**Error: "IDX10503: Signature validation failed"**
```
Cause: JWT secret mismatch between token creation and validation
Solution: Ensure same secret key across all API instances
```

**Error: "IDX10223: Lifetime validation failed"**
```
Cause: Token expired or clock skew
Solution: Check token expiration settings and server time sync
```

---

## Monitoring & Alerts

### Key Metrics to Monitor
1. **Application Health**: `/health` endpoint status
2. **Database Connectivity**: Connection pool exhaustion
3. **Error Rate**: 4xx/5xx response codes
4. **Response Time**: P50, P95, P99 latencies
5. **JWT Failures**: Authentication/authorization errors

### Sample Application Insights Queries

**Failed Requests:**
```kusto
requests
| where success == false
| summarize count() by resultCode, name
| order by count_ desc
```

**Slow Requests:**
```kusto
requests
| where duration > 3000
| project timestamp, name, duration, resultCode
| order by duration desc
```

---

## Additional Resources

- [ABP Framework Configuration](https://docs.abp.io/en/abp/latest/Configuration)
- [.NET Configuration Providers](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration)
- [Azure App Service Configuration](https://learn.microsoft.com/en-us/azure/app-service/configure-common)
- [Kubernetes Secrets Management](https://kubernetes.io/docs/concepts/configuration/secret/)
- [Application Insights ASP.NET Core](https://learn.microsoft.com/en-us/azure/azure-monitor/app/asp-net-core)

---

## Quick Reference

| Setting | Environment Variable | Config Path | Required |
|---------|---------------------|-------------|----------|
| Database | `ConnectionStrings__Default` | ConnectionStrings:Default | ✅ Yes |
| JWT Secret | `Authentication__Jwt__SecretKey` | Authentication:Jwt:SecretKey | ✅ Yes |
| CORS Origins | `Cors__AllowedOrigins__0` | Cors:AllowedOrigins | ✅ Yes |
| Allowed Hosts | `AllowedHosts` | AllowedHosts | ✅ Yes |
| App Insights | `ApplicationInsights__ConnectionString` | ApplicationInsights:ConnectionString | ⚠️ Recommended |

---

Last Updated: 2026-01-03
