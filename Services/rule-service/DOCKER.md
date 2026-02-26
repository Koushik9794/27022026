# Running Rule-Service with Docker

## Quick Start

1. **Copy the environment file:**
   ```bash
   cp .env.example .env
   ```

2. **Start the service:**
   ```bash
   docker-compose up -d
   ```

3. **Check the logs:**
   ```bash
   docker-compose logs -f rule_service
   ```

4. **Access the service:**
   - API: http://localhost:5001
   - Health: http://localhost:5001/health
   - Swagger: http://localhost:5001/swagger

## Stop the service

```bash
docker-compose down
```

## Rebuild after code changes

```bash
docker-compose up -d --build
```

## Clean up (including database)

```bash
docker-compose down -v
```

## Environment Variables

Edit `.env` to customize:
- `API_PORT` - API port (default: 5001)
- `DB_PORT` - PostgreSQL port (default: 5432)
- `DB_PASSWORD` - Database password
- See `.env.example` for all options

## Troubleshooting

**Database connection issues:**
```bash
docker-compose logs postgres
```

**Service won't start:**
```bash
docker-compose logs rule_service
```

**Reset everything:**
```bash
docker-compose down -v
docker-compose up -d --build
```
