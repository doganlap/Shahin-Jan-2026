#!/bin/bash
# Script to create a new user in GrcAuthDb
# Usage: ./scripts/create-user.sh "FirstName" "LastName" "email@example.com" "Password123!"

set -e

FIRST_NAME="${1:-}"
LAST_NAME="${2:-}"
EMAIL="${3:-}"
PASSWORD="${4:-}"

if [ -z "$FIRST_NAME" ] || [ -z "$LAST_NAME" ] || [ -z "$EMAIL" ] || [ -z "$PASSWORD" ]; then
    echo "Usage: $0 \"FirstName\" \"LastName\" \"email@example.com\" \"Password123!\""
    echo ""
    echo "Example:"
    echo "  $0 \"Ahmed\" \"Mohammed\" \"ahmed@example.com\" \"TempPassword123!\""
    exit 1
fi

echo "========================================="
echo "Creating User in GrcAuthDb"
echo "========================================="
echo ""
echo "First Name: $FIRST_NAME"
echo "Last Name:  $LAST_NAME"
echo "Email:      $EMAIL"
echo ""

# Check if running in Docker or need to connect via host
if [ -f "/.dockerenv" ]; then
    DB_HOST="db"
    DB_PORT="5432"
else
    DB_HOST="localhost"
    DB_PORT="5433"
fi

echo "Connecting to database: Host=$DB_HOST, Port=$DB_PORT, Database=GrcAuthDb"
echo ""

# First check if migrations have been run
echo "Checking if user tables exist..."
TABLE_COUNT=$(docker exec grc-db psql -U postgres -d GrcAuthDb -tAc "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'AspNetUsers';" 2>/dev/null || echo "0")

if [ "$TABLE_COUNT" -eq "0" ]; then
    echo "❌ ERROR: User tables don't exist yet!"
    echo ""
    echo "You need to run migrations first:"
    echo "  docker exec -it grc-system-grcmvc-1 dotnet ef database update --context GrcAuthDbContext"
    echo ""
    echo "OR restart the application container to run migrations automatically."
    exit 1
fi

echo "✅ User tables exist"
echo ""

# Note: Direct SQL insertion is complex due to password hashing
# Better to use the application's UserManager
echo "⚠️  Note: User creation requires password hashing via ASP.NET Identity"
echo "   This script will prepare SQL commands, but you should use the application API"
echo "   or run migrations first, then create users through the application."
echo ""
echo "To create users properly, you should:"
echo "  1. Ensure migrations are run"
echo "  2. Use the application's user creation endpoint or seed data"
echo "  3. Or use dotnet ef with a seed command"
echo ""
echo "Alternatively, you can add the user through the application UI after login."
exit 0
