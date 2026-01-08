#!/bin/bash
# Database Backup Script
# Creates compressed backups of all GRC databases

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
BACKUP_DIR="${PROJECT_ROOT}/backups"
DATE=$(date +%Y%m%d_%H%M%S)
RETENTION_DAYS=30

# Create backup directory
mkdir -p "$BACKUP_DIR"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

log_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

log_warn() {
    echo -e "${YELLOW}[WARN]${NC} $1"
}

log_error() {
    echo -e "${RED}[ERROR]${NC} $1" >&2
}

# Check if container exists and is running
if ! docker ps | grep -q "grc-db"; then
    log_error "grc-db container is not running"
    exit 1
fi

# Check database connectivity
if ! docker exec grc-db pg_isready -U postgres > /dev/null 2>&1; then
    log_error "Database is not ready"
    exit 1
fi

log_info "Starting database backup at $(date)"

# Backup GrcMvcDb
log_info "Backing up GrcMvcDb..."
if docker exec grc-db pg_dump -U postgres -Fc GrcMvcDb | \
    gzip > "$BACKUP_DIR/grcmvcdb_$DATE.sql.gz"; then
    log_info "✓ GrcMvcDb backup completed: grcmvcdb_$DATE.sql.gz"
    BACKUP_SIZE=$(du -h "$BACKUP_DIR/grcmvcdb_$DATE.sql.gz" | cut -f1)
    log_info "  Size: $BACKUP_SIZE"
else
    log_error "Failed to backup GrcMvcDb"
    exit 1
fi

# Backup GrcAuthDb
log_info "Backing up GrcAuthDb..."
if docker exec grc-db pg_dump -U postgres -Fc GrcAuthDb | \
    gzip > "$BACKUP_DIR/grcauthdb_$DATE.sql.gz"; then
    log_info "✓ GrcAuthDb backup completed: grcauthdb_$DATE.sql.gz"
    BACKUP_SIZE=$(du -h "$BACKUP_DIR/grcauthdb_$DATE.sql.gz" | cut -f1)
    log_info "  Size: $BACKUP_SIZE"
else
    log_error "Failed to backup GrcAuthDb"
    exit 1
fi

# Cleanup old backups
log_info "Cleaning up backups older than $RETENTION_DAYS days..."
DELETED=$(find "$BACKUP_DIR" -name "*.sql.gz" -mtime +$RETENTION_DAYS -delete -print | wc -l)
if [ "$DELETED" -gt 0 ]; then
    log_info "Deleted $DELETED old backup(s)"
fi

# List current backups
log_info "Current backups:"
ls -lh "$BACKUP_DIR"/*.sql.gz 2>/dev/null | tail -5 | awk '{print "  " $9 " (" $5 ")"}'

log_info "Backup completed successfully at $(date)"
