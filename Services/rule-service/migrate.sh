#!/bin/bash
# Migration management script for Rule Service

SERVICE_DIR="Services/rule-service"

case "$1" in
  "create")
    if [ -z "$2" ]; then
      echo "Usage: ./migrate.sh create <migration_name>"
      exit 1
    fi
    
    # Generate timestamp-based migration number
    TIMESTAMP=$(date +%Y%m%d%H%M%S)
    MIGRATION_NAME="$2"
    MIGRATION_NUM="${TIMESTAMP: -7}"
    MIGRATION_FILE="$SERVICE_DIR/src/infrastructure/migrations/M${MIGRATION_NUM}_${MIGRATION_NAME}.cs"
    
    echo "Creating migration: $MIGRATION_FILE"
    echo "TODO: Update migration file with schema changes"
    ;;
    
  "up"|"migrate")
    echo "Running migrations..."
    cd "$SERVICE_DIR"
    dotnet run
    cd ../..
    ;;
    
  "down"|"rollback")
    echo "Rollback command not directly supported. Update migrations manually."
    ;;
    
  *)
    echo "Usage: $0 {create|up|migrate|down|rollback} [args]"
    echo ""
    echo "Examples:"
    echo "  $0 create AddNewColumn     # Create new migration"
    echo "  $0 up                      # Run pending migrations"
    echo "  $0 migrate                 # Alias for up"
    exit 1
    ;;
esac
