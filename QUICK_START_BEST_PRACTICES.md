# Quick Start - Database Best Practices

## 🚀 Daily Workflow

### Starting Services
```bash
# Use safe startup script (prevents conflicts)
./scripts/start-safe.sh

# Or manually (after checking conflicts)
docker compose up -d
```

### Checking Health
```bash
# Quick health check
./scripts/monitor-db.sh

# Or check manually
docker compose ps
curl http://localhost:8888/health
```

### Backup Before Changes
```bash
# Always backup before migrations or major changes
./scripts/backup-db.sh
```

## 🛡️ Prevention Checklist

### Before Making Changes:
- [ ] Run backup: `./scripts/backup-db.sh`
- [ ] Check health: `./scripts/monitor-db.sh`
- [ ] Review `.env` file for correct credentials
- [ ] Verify container is on correct network

### After Making Changes:
- [ ] Verify health: `./scripts/monitor-db.sh`
- [ ] Check application logs: `docker compose logs grcmvc`
- [ ] Test critical functionality
- [ ] Document changes

## 📋 Common Issues Prevention

### Issue: Container Conflict
**Prevention**: Always use `./scripts/start-safe.sh` or check before starting
**Solution**: Script automatically detects and handles conflicts

### Issue: Connection Failure
**Prevention**: 
- Use `.env` file for all configuration
- Verify network connectivity
- Check password matches
**Solution**: Script validates connection before starting

### Issue: Data Loss
**Prevention**: 
- Always backup before migrations
- Use transactions in migrations
- Test on development first
**Solution**: Automated backup script with retention

### Issue: Configuration Drift
**Prevention**:
- Single source of truth (`.env` file)
- Version control configuration templates
- Document all changes
**Solution**: `.env.example` template for consistency

## 🔧 Emergency Procedures

### Database Connection Fails
1. Check container: `docker ps | grep grc-db`
2. Check logs: `docker logs grc-db --tail 50`
3. Test connection: `docker exec grc-db psql -U postgres -c "SELECT 1;"`
4. Check network: `docker network inspect grc-system_grc-network`
5. Verify password: Check `.env` file

### Container Won't Start
1. Check conflicts: `docker ps -a | grep grc-db`
2. Remove old: `docker compose down`
3. Clean start: `docker compose up -d`
4. Check logs: `docker compose logs db`

### Need to Restore Backup
1. Stop app: `docker compose stop grcmvc`
2. Restore: `gunzip < backups/grcmvcdb_YYYYMMDD_HHMMSS.sql.gz | docker exec -i grc-db psql -U postgres -d GrcMvcDb`
3. Verify: `docker exec grc-db psql -U postgres -d GrcMvcDb -c "SELECT COUNT(*) FROM \"Users\";"`
4. Start app: `docker compose start grcmvc`

## 📚 Full Documentation

See `DATABASE_BEST_PRACTICES.md` for comprehensive guide covering:
- Container management
- Connection string management
- Backup & recovery
- Monitoring & health checks
- Error handling
- And much more...
