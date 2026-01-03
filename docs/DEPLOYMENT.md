# GRC System - Deployment Guide

## Overview

This guide covers deploying the GRC System to various environments, from development to production.

## Prerequisites

### Development
- .NET 8.0 SDK
- SQL Server 2019+ (or SQL Server Express)
- Visual Studio 2022 / VS Code / Rider
- Git

### Staging/Production
- Docker & Docker Compose
- SQL Server (containerized or managed)
- Domain name (for production)
- SSL certificate (for HTTPS)

## Development Deployment

### 1. Clone Repository

```bash
git clone <repository-url>
cd grc-system
```

### 2. Restore Packages

```bash
dotnet restore
```

### 3. Update Connection String

Edit `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=GrcDb_Dev;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

### 4. Run Database Migrations

```bash
cd src/Grc.DbMigrator
dotnet run
```

This will:
- Create database
- Run migrations
- Seed roles
- Create admin user

### 5. Start API Host

```bash
cd src/Grc.HttpApi.Host
dotnet run
```

API will be available at:
- HTTP: http://localhost:5000
- HTTPS: https://localhost:5001
- Swagger: http://localhost:5000 (root)

### 6. Start Blazor (Separate Terminal)

```bash
cd src/Grc.Blazor
dotnet run
```

Blazor will be available at:
- http://localhost:8080

### 7. Login

- **Username:** admin
- **Password:** 1q2w3E* (change in production!)

## Docker Deployment

### Quick Start

```bash
# Build and start all services
docker-compose up -d

# Run migrations
docker-compose --profile migrate run --rm migrator

# View logs
docker-compose logs -f

# Stop services
docker-compose down
```

### Access Points

- **Blazor UI:** http://localhost:8080
- **API:** http://localhost:5000
- **Database:** localhost:1433 (sa/YourStrong@Passw0rd123!)

See [DOCKER.md](./DOCKER.md) for detailed Docker documentation.

## Staging Deployment

### 1. Prepare Environment

```bash
# Set environment variables
export ASPNETCORE_ENVIRONMENT=Staging
export ConnectionStrings__Default="Server=staging-db;Database=GrcDb_Staging;..."
export Authentication__Jwt__SecretKey="YourStagingSecretKey..."
```

### 2. Build Images

```bash
docker build -f src/Grc.HttpApi.Host/Dockerfile -t grc-api:staging .
docker build -f src/Grc.Blazor/Dockerfile -t grc-blazor:staging .
```

### 3. Deploy

```bash
# Using docker-compose
docker-compose -f docker-compose.yml -f docker-compose.staging.yml up -d

# Or using Kubernetes
kubectl apply -f k8s/staging/
```

### 4. Run Migrations

```bash
docker-compose --profile migrate run --rm migrator
```

### 5. Verify

```bash
# Check health
curl https://staging-api.grc.example.com/health

# Check API
curl https://staging-api.grc.example.com/swagger
```

## Production Deployment

### 1. Prerequisites

- [ ] Production database server configured
- [ ] SSL certificates obtained
- [ ] Domain names configured
- [ ] Secrets stored securely
- [ ] Monitoring configured
- [ ] Backup strategy in place

### 2. Prepare Secrets

**Never commit secrets to git!**

Store in:
- Azure Key Vault
- AWS Secrets Manager
- HashiCorp Vault
- Environment variables
- Docker secrets

### 3. Build Production Images

```bash
# Build with production optimizations
docker build -f src/Grc.HttpApi.Host/Dockerfile \
  --target final \
  -t grc-api:prod \
  --build-arg BUILD_CONFIGURATION=Release .

docker build -f src/Grc.Blazor/Dockerfile \
  --target final \
  -t grc-blazor:prod \
  --build-arg BUILD_CONFIGURATION=Release .
```

### 4. Tag and Push to Registry

```bash
# Tag
docker tag grc-api:prod registry.example.com/grc-api:v1.0.0
docker tag grc-blazor:prod registry.example.com/grc-blazor:v1.0.0

# Push
docker push registry.example.com/grc-api:v1.0.0
docker push registry.example.com/grc-blazor:v1.0.0
```

### 5. Deploy

#### Option A: Docker Compose

```bash
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

#### Option B: Kubernetes

```bash
kubectl apply -f k8s/production/
```

#### Option C: Azure App Service

```bash
az webapp create --name grc-api --resource-group grc-rg --plan grc-plan
az webapp config container set --name grc-api --docker-custom-image-name registry.example.com/grc-api:v1.0.0
```

### 6. Configure Production Settings

**appsettings.Production.json:**
```json
{
  "Environment": {
    "Name": "Production"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  },
  "Swagger": {
    "Enabled": false
  },
  "RateLimiting": {
    "Enabled": true,
    "PermitLimit": 50
  }
}
```

### 7. Run Migrations

```bash
# Via Docker
docker-compose --profile migrate run --rm migrator

# Or directly
cd src/Grc.DbMigrator
dotnet run --environment Production
```

### 8. Verify Deployment

```bash
# Health check
curl https://api.grc.example.com/health

# API check (should return 401 without auth)
curl https://api.grc.example.com/api/evidence

# UI check
curl https://grc.example.com
```

## Post-Deployment

### 1. Verify Health Checks

```bash
curl https://api.grc.example.com/health
curl https://api.grc.example.com/health/ready
curl https://api.grc.example.com/health/live
```

### 2. Test Authentication

```bash
# Login
curl -X POST https://api.grc.example.com/api/account/login \
  -H "Content-Type: application/json" \
  -d '{"userName":"admin","password":"..."}'

# Use token in subsequent requests
curl https://api.grc.example.com/api/evidence \
  -H "Authorization: Bearer <token>"
```

### 3. Monitor Logs

```bash
# Docker
docker-compose logs -f api

# Kubernetes
kubectl logs -f deployment/grc-api

# Application Insights
# View in Azure Portal
```

### 4. Check Metrics

- Application Insights dashboard
- Health check endpoints
- Database performance
- API response times

## Rollback Procedure

### 1. Identify Previous Version

```bash
# List versions
docker images | grep grc-api

# Or from registry
az acr repository show-tags --name registry --repository grc-api
```

### 2. Rollback

```bash
# Update docker-compose.yml with previous version
# Or update Kubernetes deployment

docker-compose up -d

# Or
kubectl set image deployment/grc-api grc-api=registry.example.com/grc-api:v0.9.0
```

### 3. Verify

```bash
curl https://api.grc.example.com/health
```

## Troubleshooting

### Database Connection Issues

**Symptoms:**
- Health check fails
- API returns 500 errors
- Logs show connection timeouts

**Solutions:**
1. Verify connection string
2. Check database is accessible
3. Verify firewall rules
4. Check credentials

### API Not Starting

**Symptoms:**
- Container exits immediately
- Health check fails

**Solutions:**
1. Check logs: `docker-compose logs api`
2. Verify configuration
3. Check environment variables
4. Verify secrets are set

### Blazor Not Loading

**Symptoms:**
- Blank page
- 404 errors
- API calls fail

**Solutions:**
1. Check nginx logs: `docker-compose logs blazor`
2. Verify API is running
3. Check CORS configuration
4. Check browser console

### Performance Issues

**Symptoms:**
- Slow response times
- Timeouts
- High CPU/memory

**Solutions:**
1. Check database performance
2. Review query execution plans
3. Add database indexes
4. Scale horizontally
5. Enable caching

## Security Checklist

- [ ] JWT secret key changed from default
- [ ] Database password is strong
- [ ] HTTPS enabled
- [ ] CORS configured for production domains only
- [ ] Secrets stored securely (not in code)
- [ ] Firewall rules configured
- [ ] Security headers enabled
- [ ] Rate limiting enabled
- [ ] Swagger disabled in production
- [ ] Error messages don't expose sensitive info
- [ ] Logging doesn't include secrets
- [ ] Backup strategy in place

## Maintenance

### Regular Tasks

1. **Weekly:**
   - Review logs
   - Check health metrics
   - Verify backups

2. **Monthly:**
   - Update dependencies
   - Review security alerts
   - Performance analysis

3. **Quarterly:**
   - Rotate secrets
   - Review access logs
   - Disaster recovery test

### Updates

1. Pull latest code
2. Run tests
3. Build new images
4. Deploy to staging
5. Test thoroughly
6. Deploy to production
7. Monitor closely

---

**Last Updated:** 2026-01-02
