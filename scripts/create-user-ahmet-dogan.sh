#!/bin/bash
# Script to create Ahmet Dogan user via API endpoint
# This script will retry until the endpoint is available

EMAIL="ahmet.dogan@doganconsult.com"
PASSWORD="DogCon@Admin2026"

echo "========================================="
echo "Creating User: Ahmet Dogan"
echo "========================================="
echo "Email: $EMAIL"
echo "Role: PlatformAdmin"
echo ""

# Wait for application to be ready
echo "Waiting for application to be ready..."
for i in {1..30}; do
    if curl -s http://localhost:8888/api/health > /dev/null 2>&1; then
        echo "✅ Application is ready"
        break
    fi
    echo "  Attempt $i/30: Application not ready yet..."
    sleep 2
done

# Try to create user via API
echo ""
echo "Creating user via API endpoint..."
RESPONSE=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
    -X POST \
    -H "Content-Type: application/json" \
    http://localhost:8888/api/seed/users/create \
    -d "{
        \"firstName\": \"Ahmet\",
        \"lastName\": \"Dogan\",
        \"email\": \"$EMAIL\",
        \"password\": \"$PASSWORD\",
        \"department\": \"Administration\",
        \"jobTitle\": \"Platform Administrator\",
        \"roleName\": \"PlatformAdmin\"
    }")

HTTP_CODE=$(echo "$RESPONSE" | grep "HTTP_CODE" | cut -d: -f2)
BODY=$(echo "$RESPONSE" | grep -v "HTTP_CODE")

echo "HTTP Status: $HTTP_CODE"
echo "Response: $BODY"

if [ "$HTTP_CODE" = "200" ] || [ "$HTTP_CODE" = "201" ]; then
    echo ""
    echo "✅ User created successfully!"
    
    # Verify user exists
    echo ""
    echo "Verifying user in database..."
    docker exec grc-db psql -U postgres -d GrcAuthDb -c \
        "SELECT \"UserName\", \"Email\", \"FirstName\", \"LastName\", \"IsActive\" FROM \"AspNetUsers\" WHERE \"Email\" = '$EMAIL';" 2>&1
else
    echo ""
    echo "❌ Failed to create user via API (HTTP $HTTP_CODE)"
    echo ""
    echo "The endpoint may not be available yet, or the application needs to be rebuilt."
    echo "The user will be created automatically when the application starts if the code is in place."
fi
