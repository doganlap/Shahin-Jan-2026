#!/bin/bash

################################################################################
# GRC Database Backup Script
#
# Purpose: Automated backup of all GRC databases
# Schedule: Daily at 2 AM via cron
# Retention: 30 days
# Storage: Local + Azure Blob Storage
#
# Usage: ./backup-databases.sh
# Cron: 0 2 * * * /home/Shahin-ai/Shahin-Jan-2026/scripts/backup-databases.sh
################################################################################

set -e  # Exit on error
set -u  # Exit on undefined variable

# =============================================================================
# CONFIGURATION
# =============================================================================
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"
BACKUP_DIR="/var/backups/grc"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
DATE_ONLY=$(date +%Y-%m-%d)
LOG_FILE="/var/log/grc-backup.log"

# Load environment variables
if [ -f "$PROJECT_ROOT/.env.grcmvc.secure" ]; then
    set -a  # Auto-export all variables
    source "$PROJECT_ROOT/.env.grcmvc.secure"
    set +a
else
    echo "ERROR: .env.grcmvc.secure not found at $PROJECT_ROOT" | tee -a "$LOG_FILE"
    exit 1
fi

# Defaults if not set in env
BACKUP_RETENTION_DAYS=${BACKUP_RETENTION_DAYS:-30}

# =============================================================================
# LOGGING
# =============================================================================
log() {
    echo "[$(date '+%Y-%m-%d %H:%M:%S')] $1" | tee -a "$LOG_FILE"
}

log_error() {
    echo "[$(date '+%Y-%m-%d %H:%M:%S')] ERROR: $1" | tee -a "$LOG_FILE" >&2
}

# =============================================================================
# CREATE BACKUP DIRECTORY
# =============================================================================
log "========================================"
log "GRC Database Backup Started"
log "========================================"

mkdir -p "$BACKUP_DIR"
mkdir -p "$BACKUP_DIR/$DATE_ONLY"

# =============================================================================
# FUNCTION: Backup Single Database
# =============================================================================
backup_database() {
    local db_name=$1
    local backup_file="$BACKUP_DIR/$DATE_ONLY/${db_name}_${TIMESTAMP}.sql"
    local compressed_file="${backup_file}.gz"

    log "Backing up database: $db_name"

    # PostgreSQL backup
    if command -v pg_dump &> /dev/null; then
        PGPASSWORD="${DB_PASSWORD}" pg_dump \
            -h localhost \
            -U "${DB_USER}" \
            -d "$db_name" \
            --verbose \
            --no-owner \
            --no-acl \
            > "$backup_file" 2>> "$LOG_FILE"

        if [ $? -eq 0 ]; then
            # Compress backup
            gzip "$backup_file"

            local size=$(du -h "$compressed_file" | cut -f1)
            log "✅ Backup successful: $compressed_file ($size)"

            # Upload to Azure if configured
            upload_to_azure "$compressed_file"

            echo "$compressed_file"
        else
            log_error "Backup failed for database: $db_name"
            return 1
        fi

    # SQL Server backup (if using SQL Server instead)
    elif command -v sqlcmd &> /dev/null; then
        sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -Q \
            "BACKUP DATABASE [$db_name] TO DISK = N'$backup_file' WITH COMPRESSION, STATS = 10" \
            >> "$LOG_FILE" 2>&1

        if [ $? -eq 0 ]; then
            gzip "$backup_file"
            local size=$(du -h "$compressed_file" | cut -f1)
            log "✅ Backup successful: $compressed_file ($size)"
            upload_to_azure "$compressed_file"
            echo "$compressed_file"
        else
            log_error "Backup failed for database: $db_name"
            return 1
        fi
    else
        log_error "Neither pg_dump nor sqlcmd found. Cannot perform backup."
        return 1
    fi
}

# =============================================================================
# FUNCTION: Upload to Azure Blob Storage
# =============================================================================
upload_to_azure() {
    local file_path=$1

    # Check if Azure credentials are configured
    if [ -z "${BACKUP_STORAGE_CONNECTION:-}" ] || [ "$BACKUP_STORAGE_CONNECTION" == "DefaultEndpointsProtocol=https;AccountName=your-storage-account-name"* ]; then
        log "⚠️  Azure Blob Storage not configured. Skipping upload."
        return 0
    fi

    log "Uploading to Azure Blob Storage..."

    if command -v az &> /dev/null; then
        az storage blob upload \
            --connection-string "$BACKUP_STORAGE_CONNECTION" \
            --container-name grc-backups \
            --file "$file_path" \
            --name "$(basename $file_path)" \
            --overwrite \
            >> "$LOG_FILE" 2>&1

        if [ $? -eq 0 ]; then
            log "✅ Uploaded to Azure: $(basename $file_path)"
        else
            log_error "Failed to upload to Azure: $(basename $file_path)"
        fi
    else
        log "⚠️  Azure CLI not installed. Skipping cloud upload."
    fi
}

# =============================================================================
# FUNCTION: Send Notification
# =============================================================================
send_notification() {
    local status=$1
    local message=$2

    # Webhook notification (Slack/Teams)
    if [ ! -z "${BACKUP_WEBHOOK_URL:-}" ] && [ "$BACKUP_WEBHOOK_URL" != "https://hooks.slack.com/services/YOUR/WEBHOOK/URL" ]; then
        local icon="✅"
        [ "$status" == "error" ] && icon="❌"

        curl -X POST "$BACKUP_WEBHOOK_URL" \
            -H "Content-Type: application/json" \
            -d "{\"text\":\"$icon GRC Backup: $message\"}" \
            &> /dev/null
    fi
}

# =============================================================================
# GET LIST OF DATABASES
# =============================================================================
log "Discovering databases..."

DATABASES=()

# PostgreSQL
if command -v psql &> /dev/null; then
    while IFS= read -r db; do
        if [[ $db == GrcDb_* ]] || [[ $db == GrcMvcDb ]]; then
            DATABASES+=("$db")
        fi
    done < <(PGPASSWORD="${DB_PASSWORD}" psql -h localhost -U "${DB_USER}" -d postgres -t -c "SELECT datname FROM pg_database WHERE datname LIKE 'GrcDb_%' OR datname = 'GrcMvcDb';" 2>/dev/null)
fi

# SQL Server
if command -v sqlcmd &> /dev/null && [ ${#DATABASES[@]} -eq 0 ]; then
    while IFS= read -r db; do
        db=$(echo "$db" | tr -d '[:space:]')  # Trim whitespace
        if [[ $db == GrcDb_* ]] || [[ $db == GrcMvcDb ]]; then
            DATABASES+=("$db")
        fi
    done < <(sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -h -1 -W -Q "SET NOCOUNT ON; SELECT name FROM sys.databases WHERE name LIKE 'GrcDb_%' OR name = 'GrcMvcDb';" 2>/dev/null)
fi

if [ ${#DATABASES[@]} -eq 0 ]; then
    log_error "No GRC databases found to backup!"
    send_notification "error" "No databases found"
    exit 1
fi

log "Found ${#DATABASES[@]} database(s) to backup: ${DATABASES[*]}"

# =============================================================================
# BACKUP ALL DATABASES
# =============================================================================
SUCCESSFUL_BACKUPS=0
FAILED_BACKUPS=0
BACKUP_FILES=()

for db in "${DATABASES[@]}"; do
    if backup_file=$(backup_database "$db"); then
        SUCCESSFUL_BACKUPS=$((SUCCESSFUL_BACKUPS + 1))
        BACKUP_FILES+=("$backup_file")
    else
        FAILED_BACKUPS=$((FAILED_BACKUPS + 1))
    fi
done

# =============================================================================
# CLEANUP OLD BACKUPS
# =============================================================================
log "Cleaning up backups older than $BACKUP_RETENTION_DAYS days..."

find "$BACKUP_DIR" -name "*.sql.gz" -mtime +$BACKUP_RETENTION_DAYS -delete 2>> "$LOG_FILE"
find "$BACKUP_DIR" -type d -empty -delete 2>> "$LOG_FILE"

log "✅ Cleanup complete"

# =============================================================================
# SUMMARY
# =============================================================================
log "========================================"
log "Backup Summary:"
log "  Total Databases: ${#DATABASES[@]}"
log "  Successful: $SUCCESSFUL_BACKUPS"
log "  Failed: $FAILED_BACKUPS"
log "  Total Size: $(du -sh $BACKUP_DIR/$DATE_ONLY | cut -f1)"
log "========================================"

if [ $FAILED_BACKUPS -eq 0 ]; then
    log "✅ All backups completed successfully!"
    send_notification "success" "$SUCCESSFUL_BACKUPS databases backed up successfully"
    exit 0
else
    log_error "❌ Some backups failed! Check log: $LOG_FILE"
    send_notification "error" "$FAILED_BACKUPS backup(s) failed"
    exit 1
fi
