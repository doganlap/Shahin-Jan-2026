# Docker Setup and Usage Guide

## Overview

The GRC System is containerized using Docker and Docker Compose. This guide explains how to build, run, and manage the containerized application.

## Prerequisites

- Docker Desktop (or Docker Engine) installed
- Docker Compose v3.8 or later
- At least 4GB RAM available for Docker
- Ports 1433, 5000, 5001, 8080 available

## Architecture

The system consists of three main containers:

1. **db** - SQL Server 2022 database
2. **api** - GRC API Host (ASP.NET Core)
3. **blazor** - GRC Blazor WebAssembly UI (served via nginx)

## Quick Start

### 1. Build and Start All Services

```bash
cd /home/dogan/grc-system
docker-compose up -d
```

This will:
- Build all Docker images
- Start SQL Server database
- Start API Host on ports 5000 (HTTP) and 5001 (HTTPS)
- Start Blazor UI on port 8080
- Wait for database to be healthy before starting API

### 2. Run Database Migrations

```bash
docker-compose --profile migrate run --rm migrator
```

This will:
- Run the database migrator container
- Apply all pending migrations
- Seed initial data (roles, admin user)
- Exit after completion

### 3. Access the Application

- **Blazor UI:** http://localhost:8080
- **API Swagger:** http://localhost:5000 (when in Development mode)
- **API Health:** http://localhost:5000/health
- **Database:** localhost:1433 (sa/YourStrong@Passw0rd123!)

## Building Individual Images

### Build API Host

```bash
docker build -f src/Grc.HttpApi.Host/Dockerfile -t grc-api:latest .
```

### Build Blazor

```bash
docker build -f src/Grc.Blazor/Dockerfile -t grc-blazor:latest .
```

### Build DbMigrator

```bash
docker build -f src/Grc.DbMigrator/Dockerfile -t grc-migrator:latest .
```

## Running Containers

### Start All Services

```bash
docker-compose up -d
```

### Start Specific Service

```bash
docker-compose up -d db      # Start only database
docker-compose up -d api     # Start database + API
docker-compose up -d blazor  # Start all services
```

### View Logs

```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f api
docker-compose logs -f blazor
docker-compose logs -f db
```

### Stop Services

```bash
docker-compose stop
```

### Stop and Remove Containers

```bash
docker-compose down
```

### Stop and Remove Everything (including volumes)

```bash
docker-compose down -v
```

## Environment Configuration

### Development

The default `docker-compose.yml` is configured for development. To use it:

```bash
docker-compose up -d
```

### Production

Create `docker-compose.prod.yml`:

```yaml
version: '3.8'

services:
  api:
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__Default=${DB_CONNECTION_STRING}
      - Authentication__Jwt__SecretKey=${JWT_SECRET_KEY}
    restart: always

  blazor:
    restart: always
```

Run with:

```bash
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

## Database Management

### Connect to Database

```bash
docker exec -it grc-db /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P "YourStrong@Passw0rd123!" \
  -Q "SELECT name FROM sys.databases"
```

### Backup Database

```bash
docker exec grc-db /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P "YourStrong@Passw0rd123!" \
  -Q "BACKUP DATABASE GrcDb TO DISK='/var/opt/mssql/backup/GrcDb.bak'"
```

### Restore Database

```bash
docker exec grc-db /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P "YourStrong@Passw0rd123!" \
  -Q "RESTORE DATABASE GrcDb FROM DISK='/var/opt/mssql/backup/GrcDb.bak' WITH REPLACE"
```

## Troubleshooting

### Database Not Starting

```bash
# Check database logs
docker-compose logs db

# Check if port 1433 is available
netstat -an | grep 1433

# Remove and recreate database container
docker-compose down
docker volume rm grc-system_sql_data
docker-compose up -d db
```

### API Not Connecting to Database

1. Check database is healthy:
   ```bash
   docker-compose ps db
   ```

2. Check API logs:
   ```bash
   docker-compose logs api
   ```

3. Verify connection string in docker-compose.yml

### Blazor Not Loading

1. Check nginx logs:
   ```bash
   docker-compose logs blazor
   ```

2. Verify API is running:
   ```bash
   curl http://localhost:5000/health
   ```

3. Check browser console for errors

### Port Conflicts

If ports are already in use, modify `docker-compose.yml`:

```yaml
services:
  db:
    ports:
      - "1434:1433"  # Change external port
  api:
    ports:
      - "5002:80"    # Change external port
  blazor:
    ports:
      - "8081:80"    # Change external port
```

## Development Workflow

### 1. Make Code Changes

Edit code in your IDE as usual.

### 2. Rebuild and Restart

```bash
# Rebuild specific service
docker-compose build api
docker-compose up -d api

# Or rebuild all
docker-compose build
docker-compose up -d
```

### 3. View Logs

```bash
docker-compose logs -f api
```

## Production Deployment

### 1. Build Production Images

```bash
docker build -f src/Grc.HttpApi.Host/Dockerfile -t grc-api:prod --target final .
docker build -f src/Grc.Blazor/Dockerfile -t grc-blazor:prod --target final .
```

### 2. Tag for Registry

```bash
docker tag grc-api:prod your-registry/grc-api:v1.0.0
docker tag grc-blazor:prod your-registry/grc-blazor:v1.0.0
```

### 3. Push to Registry

```bash
docker push your-registry/grc-api:v1.0.0
docker push your-registry/grc-blazor:v1.0.0
```

### 4. Deploy

Update `docker-compose.prod.yml` with registry images and deploy.

## Security Considerations

1. **Change Default Passwords:**
   - Update `SA_PASSWORD` in docker-compose.yml
   - Use strong passwords in production

2. **Secrets Management:**
   - Use Docker secrets or environment variables
   - Never commit secrets to git

3. **Network Security:**
   - Use internal Docker networks
   - Expose only necessary ports

4. **Image Security:**
   - Regularly update base images
   - Scan images for vulnerabilities

## Performance Tuning

### Database

```yaml
services:
  db:
    environment:
      - MSSQL_MEMORY_LIMIT_MB=2048
```

### API

```yaml
services:
  api:
    deploy:
      resources:
        limits:
          cpus: '2'
          memory: 2G
        reservations:
          cpus: '1'
          memory: 1G
```

## Monitoring

### Container Stats

```bash
docker stats
```

### Health Checks

```bash
# API health
curl http://localhost:5000/health

# Database health
docker-compose ps db
```

## Cleanup

### Remove All Containers and Volumes

```bash
docker-compose down -v
```

### Remove Images

```bash
docker rmi grc-api grc-blazor grc-migrator
```

### Full Cleanup

```bash
docker-compose down -v --rmi all
```

## Additional Resources

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [SQL Server on Linux](https://docs.microsoft.com/sql/linux/)

---

**Last Updated:** 2026-01-02
