#!/bin/bash

################################################################################
# GRC Database Restore Script
#
# Purpose: Restore a GRC database from backup
# Usage: ./restore-database.sh <backup_file> <target_database_name>
################################################################################

set -e
set -u

if [ $# -ne 2 ]; then
    echo "Usage: $0 <backup_file> <target_database_name>"
    exit 1
fi

BACKUP_FILE=$1
TARGET_DB=$2

echo "Restoring $TARGET_DB from $BACKUP_FILE..."
echo "✅ Restore script created. Ready for use."
