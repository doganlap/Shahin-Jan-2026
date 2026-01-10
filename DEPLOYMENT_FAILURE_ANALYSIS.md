# Deployment Failure Analysis
## Why Build Succeeded but Deployment Failed

**Date:** 2026-01-10
**Status:** Build ✅ Successful | Deployment ❌ Failed

---

## Executive Summary

Your Docker containers **build successfully** but **fail to deploy** due to **runtime configuration issues**, not build-time compilation issues. The build process (creating Docker images) works perfectly, but when containers try to start and run, they encounter missing environment variables, database permissions, and cluster ID mismatches.

**Key Insight:** Build ≠ Deployment. Building creates the binary/image. Deployment runs it with real configuration.

---

## Current Container Status

### ✅ Healthy Services (6/12)
1. **PostgreSQL** - Running (healthy)
2. **Redis** - Running (healthy)
3. **ClickHouse** - Running (healthy)
4. **Zookeeper** - Running (healthy)
5. **Camunda** - Running (healthy)
6. **Grafana** - Running (healthy)
7. **Kafka UI** - Running (but useless without Kafka)

### ❌ Failed Services (5/12)
1. **GrcMvc** (Main App) - Exit 139 (Segmentation Fault)
2. **Kafka** - Exit 1 (Cluster ID Mismatch)
3. **Kafka Connect** - Exit 2 (Depends on Kafka)
4. **Metabase** - Exit 1 (Database Schema Issues)
5. **n8n** - Exit 1 (Permission Denied)
6. **Superset** - Exit 1 (Missing Python Module)

---

## Root Causes Analysis

### 1. GRC MVC Application - CRITICAL ⚠️

**Container:** `shahin-jan-2026_grcmvc_1`
**Status:** Exited (139) - Segmentation Fault
**Root Cause:** Missing JWT_SECRET environment variable

#### Error Message
```
Unhandled exception. System.InvalidOperationException: JWT_SECRET environment variable is required.
Set it before starting the application.
   at Program.<Main>$(String[] args) in /src/GrcMvc/Program.cs:line 128
```

#### Why It Happens
- **Build Phase:** Compiles successfully because JWT_SECRET isn't needed at compile time
- **Runtime Phase:** Application crashes immediately on startup because it's required for JWT authentication initialization
- Exit code 139 indicates SIGSEGV (segmentation fault) - likely from .NET runtime crash after exception

#### Location in Code
[Program.cs:128](/home/Shahin-ai/Shahin-Jan-2026/src/GrcMvc/Program.cs#L128)

#### Fix Required
```bash
# Add to .env file
JWT_SECRET=YourSecureRandomSecret256BitsOrMore12345678901234567890
```

#### Impact
🔴 **BLOCKING** - This is your main application. Nothing works without it.

---

### 2. Apache Kafka - HIGH PRIORITY ⚠️

**Container:** `grc-kafka`
**Status:** Exit 1
**Root Cause:** Cluster ID mismatch from previous deployment

#### Error Message
```
kafka.common.InconsistentClusterIdException: The Cluster ID Cbrwrv-6T0Gjv_oEBI9aIw
doesn't match stored clusterId Some(KdwWgte0SSC5dg2Nrb2nUw) in meta.properties.
The broker is trying to join the wrong cluster.
```

#### Why It Happens
- Kafka stores cluster metadata in `/var/lib/kafka/data`
- Old cluster ID cached from previous run
- Zookeeper has different cluster ID
- Build works because it just copies files
- Runtime fails when Kafka checks consistency

#### Fix Required
```bash
# Option 1: Clean Kafka data (DELETES ALL KAFKA DATA)
docker-compose down
docker volume rm shahin-jan-2026_kafka_data
docker-compose up -d kafka

# Option 2: Update Zookeeper cluster ID (Safer)
docker exec -it grc-zookeeper zkCli.sh deleteall /cluster/id
docker-compose restart kafka
```

#### Impact
🟡 **HIGH** - Event streaming, CDC, and real-time analytics depend on this

---

### 3. n8n Workflow Engine - MEDIUM PRIORITY

**Container:** `grc-n8n`
**Status:** Exit 1 (Crash Loop)
**Root Cause:** PostgreSQL schema permission denied

#### Error Message
```
There was an error running database migrations
permission denied for schema public
```

#### Why It Happens
- n8n user (`n8n`) doesn't have CREATE permissions on `public` schema
- PostgreSQL 15 changed default permissions for security
- Build succeeds because database isn't touched
- Runtime fails when migrations try to run

#### Fix Required
```sql
-- Connect to PostgreSQL
docker exec -it grc-db psql -U postgres

-- Grant permissions
CREATE DATABASE n8n;
\c n8n
GRANT ALL ON SCHEMA public TO n8n;
GRANT CREATE ON DATABASE n8n TO n8n;
ALTER DATABASE n8n OWNER TO n8n;
```

#### Impact
🟡 **MEDIUM** - Workflow automation disabled, but not critical

---

### 4. Apache Superset - MEDIUM PRIORITY

**Container:** `grc-superset`
**Status:** Exit 1
**Root Cause:** Missing psycopg2 PostgreSQL driver

#### Error Message
```python
ModuleNotFoundError: No module named 'psycopg2'
```

#### Why It Happens
- docker-compose.yml has inline command to install psycopg2:
  ```yaml
  command: >
    bash -c "
      pip install psycopg2-binary &&
      superset db upgrade &&
      ...
  ```
- But the package installation is **failing silently** or **timing out**
- Build phase doesn't catch this (Dockerfile doesn't install psycopg2)
- Runtime phase fails when SQLAlchemy tries to connect

#### Fix Required

**Option 1: Fix docker-compose command (Current approach - NOT WORKING)**
```yaml
# Already tried, still failing - pip might be timing out
```

**Option 2: Create custom Dockerfile (RECOMMENDED)**
```dockerfile
# File: superset/Dockerfile.custom
FROM apache/superset:latest

USER root
RUN pip install --no-cache-dir psycopg2-binary
USER superset
```

Then update docker-compose.yml:
```yaml
superset:
  build:
    context: ./superset
    dockerfile: Dockerfile.custom
  # Remove pip install from command
```

#### Impact
🟡 **MEDIUM** - BI dashboards unavailable, but Grafana still works

---

### 5. Metabase - LOW PRIORITY

**Container:** `grc-metabase`
**Status:** Exit 1
**Root Cause:** Database migration/schema issue

#### Error Message
```
There was an error running database migrations
(Complex Java stack trace related to Liquibase)
```

#### Why It Happens
- Metabase database schema exists but is incompatible
- Possible version mismatch or corrupted migration state
- Build works (no compilation needed for Java JAR)
- Runtime fails during Liquibase schema validation

#### Fix Required
```bash
# Option 1: Reset Metabase database
docker exec -it grc-db psql -U postgres
DROP DATABASE metabase;
CREATE DATABASE metabase;
GRANT ALL PRIVILEGES ON DATABASE metabase TO metabase;

# Option 2: Fresh start with new database name
# In docker-compose.yml, change:
MB_DB_DBNAME: metabase_new
```

#### Impact
🟢 **LOW** - You have Superset and Grafana for analytics

---

## Why Build Succeeds vs Why Deployment Fails

### Build Phase (✅ Works)
```
Build Phase Checks:
✅ Code compiles
✅ Dependencies resolve
✅ Docker layers cache correctly
✅ No syntax errors
✅ Static analysis passes

What's NOT Checked:
❌ Environment variables
❌ Database connectivity
❌ External service dependencies
❌ Runtime configuration
❌ Cluster state consistency
```

### Deployment Phase (❌ Fails)
```
Runtime Checks:
✅ Container starts
❌ Environment variables loaded → JWT_SECRET missing
❌ Database connection → Permission errors
❌ Service dependencies → Kafka cluster mismatch
❌ Module imports → psycopg2 not installed
❌ Schema migrations → Version conflicts
```

---

## The Build vs Deploy Difference

| Phase | What It Does | What Can Fail |
|-------|--------------|---------------|
| **Build** | Compiles code, creates image | Syntax errors, missing dependencies in Dockerfile |
| **Deploy** | Runs the built image | Missing config, wrong credentials, service dependencies |

**Analogy:**
- **Build** = Building a car in factory ✅
- **Deploy** = Trying to drive it without gas, keys, or license ❌

---

## Priority Fix Order

### 🔴 CRITICAL - Do First
1. **Fix GrcMvc JWT_SECRET**
   ```bash
   echo 'JWT_SECRET="$(openssl rand -base64 32)"' >> .env
   docker-compose up -d grcmvc
   ```

### 🟡 HIGH - Do Second
2. **Fix Kafka Cluster ID**
   ```bash
   docker volume rm shahin-jan-2026_kafka_data
   docker-compose up -d kafka
   ```

### 🟡 MEDIUM - Do Third
3. **Fix n8n Permissions**
   ```sql
   docker exec -it grc-db psql -U postgres -c "GRANT ALL ON SCHEMA public TO n8n"
   ```

4. **Fix Superset psycopg2**
   ```bash
   # Create custom Dockerfile as shown above
   docker-compose build superset
   docker-compose up -d superset
   ```

### 🟢 LOW - Optional
5. **Fix Metabase**
   ```bash
   docker exec -it grc-db psql -U postgres -c "DROP DATABASE metabase; CREATE DATABASE metabase;"
   docker-compose up -d metabase
   ```

---

## Verification Commands

After each fix, verify:

```bash
# Check container status
docker-compose ps

# Check specific container logs
docker logs grcmvc -f
docker logs grc-kafka -f
docker logs grc-n8n -f

# Check health
docker-compose ps | grep "Up (healthy)"

# Test application endpoint
curl http://localhost:8888/health
```

---

## Complete Fix Script

```bash
#!/bin/bash
# File: fix-deployment.sh

echo "🔧 Fixing Shahin AI GRC Deployment Issues"

# 1. Fix GrcMvc - Add JWT_SECRET
echo "📝 Step 1: Adding JWT_SECRET to .env"
if ! grep -q "JWT_SECRET" .env; then
    echo "JWT_SECRET=$(openssl rand -base64 32)" >> .env
    echo "✅ JWT_SECRET added"
else
    echo "⚠️  JWT_SECRET already exists"
fi

# 2. Fix Kafka - Reset cluster data
echo "📝 Step 2: Resetting Kafka cluster"
docker-compose down kafka kafka-connect
docker volume rm shahin-jan-2026_kafka_data || true
echo "✅ Kafka data reset"

# 3. Fix n8n - Database permissions
echo "📝 Step 3: Fixing n8n database permissions"
docker exec -it grc-db psql -U postgres -c "
    CREATE DATABASE IF NOT EXISTS n8n;
    GRANT ALL ON SCHEMA public TO n8n;
    GRANT ALL ON DATABASE n8n TO n8n;
"
echo "✅ n8n permissions fixed"

# 4. Fix Metabase - Reset database
echo "📝 Step 4: Resetting Metabase database"
docker exec -it grc-db psql -U postgres -c "
    DROP DATABASE IF EXISTS metabase;
    CREATE DATABASE metabase;
    GRANT ALL PRIVILEGES ON DATABASE metabase TO metabase;
"
echo "✅ Metabase database reset"

# 5. Restart all services
echo "📝 Step 5: Restarting all services"
docker-compose up -d

echo "✅ All fixes applied!"
echo "⏳ Waiting 30 seconds for services to start..."
sleep 30

echo "📊 Current Status:"
docker-compose ps

echo ""
echo "🔍 To check logs:"
echo "  docker logs grcmvc -f"
echo "  docker logs grc-kafka -f"
echo "  docker-compose logs -f"
```

---

## Key Takeaways

1. **Build ≠ Runtime:** Docker build validates code compilation, not configuration
2. **Environment Variables:** Required at runtime, not build time
3. **Database State:** Can cause failures even with perfect code
4. **Service Dependencies:** Each service needs its dependencies healthy
5. **Permissions:** PostgreSQL 15 requires explicit schema grants

---

## Next Steps

1. Run the fix script above
2. Monitor logs for each service
3. Verify all services reach "healthy" state
4. Test application functionality
5. Document any additional configuration needed

---

## References

- Container logs: `docker logs <container_name>`
- Service status: `docker-compose ps`
- Environment file: [.env](/home/Shahin-ai/Shahin-Jan-2026/.env)
- Main compose: [docker-compose.yml](/home/Shahin-ai/Shahin-Jan-2026/docker-compose.yml)
- Production compose: [docker-compose.production.yml](/home/Shahin-ai/Shahin-Jan-2026/docker-compose.production.yml)

---

**Generated:** 2026-01-10 by Claude Code
**Analysis Confidence:** HIGH
**Recommended Action:** Apply fixes in priority order listed above
