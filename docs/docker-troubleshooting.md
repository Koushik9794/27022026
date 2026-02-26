# Docker Compose Troubleshooting Guide

## ContainerConfig Error (Corrupted Container)

**Error:**
```
ERROR: for postgres  'ContainerConfig'
```

**Solution:**

```bash
# Step 1: Stop all containers
sudo docker-compose down

# Step 2: Remove corrupted containers
sudo docker rm -f catalog_service_db catalog_service

# Step 3: Clean up volumes (CAUTION: This deletes data!)
sudo docker-compose down -v

# Step 4: Start fresh
sudo docker-compose up -d

# If still failing, prune everything:
sudo docker system prune -a --volumes
# Then: sudo docker-compose up -d
```

## Port Already in Use

**Error:**
```
Bind for 0.0.0.0:6432 failed: port is already allocated
```

**Solution:**

```bash
# Find what's using the port
sudo lsof -i :6432
# or
sudo netstat -tulpn | grep 6432

# Stop the conflicting service
sudo docker stop <container_id>

# Or change port in docker-compose.yml
ports:
  - "6433:5432"  # Use different host port
```

## WSL-Specific Issues

**Docker not accessible:**

```bash
# Check Docker is running
docker ps

# If not working, restart Docker Desktop on Windows
# Then in WSL:
docker context use default
```

**Path issues:**

```bash
# Use /mnt/c/ for Windows paths
cd /mnt/c/Users/sk72/Projects/gss/gss-backend/Services/catalog-service
```

## Quick Cleanup Commands

```bash
# Stop all containers
sudo docker-compose down

# Stop and remove volumes
sudo docker-compose down -v

# Remove specific container
sudo docker rm -f catalog_service_db

# Remove all stopped containers
sudo docker container prune

# Remove unused images
sudo docker image prune -a

# Nuclear option - clean everything
sudo docker system prune -a --volumes
```

## Recommended Workflow

**For Development:**
1. Use **PowerShell** on Windows (not WSL) for Docker commands
2. Use **VS Code** for debugging (F5)
3. PostgreSQL in Docker, app runs locally

```powershell
# In PowerShell
cd C:\Users\sk72\Projects\gss\gss-backend\Services\catalog-service
docker-compose up -d postgres  # Only start database
# Then F5 in VS Code to debug the app
```

## Checking Service Status

```bash
# List running containers
docker ps

# View logs
docker-compose logs -f

# Check specific service
docker-compose logs -f postgres

# Inspect container
docker inspect catalog_service_db
```

## Database Connection Test

```bash
# Connect to PostgreSQL
docker exec -it catalog_service_db psql -U postgres

# List databases
\l

# Connect to catalog_service database
\c catalog_service

# List tables
\dt
```

---

**Quick Fix for Your Current Error:**

```bash
sudo docker-compose down
sudo docker rm -f catalog_service_db catalog_service
sudo docker-compose up -d
```
