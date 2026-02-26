# Tester Guide - GSS Backend

Welcome! This guide will help you set up, run, and test the GSS Backend services.

## Table of Contents

- [Quick Start](#quick-start)
- [Environment Setup](#environment-setup)
- [Running Services](#running-services)
- [API Testing](#api-testing)
- [Test Scenarios](#test-scenarios)
- [Common Issues](#common-issues)

## Quick Start

### 1. Install Prerequisites

Download and install:

1. **Docker Desktop** - https://www.docker.com/products/docker-desktop
   - Simplest way to run all services
   - No need to install .NET or PostgreSQL separately
   
   **For Windows Users (Highly Recommended)**:
   - Install WSL2 first for 10x better performance:
     ```powershell
     # Run in PowerShell as Administrator
     wsl --install -d Ubuntu-22.04
     # Restart computer
     ```
   - Install Docker Desktop and enable WSL2 integration
   - Clone repository in WSL (Ubuntu terminal):
     ```bash
     cd ~
     git clone <repository-url>
     cd gss-backend
     ```
   - Use WSL terminal for all Docker commands
   
   > **Why WSL2?** Much faster Docker performance on Windows. Services start in seconds instead of minutes.

2. **Postman** (Optional) - https://www.postman.com/downloads/
   - For advanced API testing
   - We'll primarily use Swagger UI (built-in)

### 2. Start All Services

```powershell
# Open PowerShell in the gss-backend directory
cd path\to\gss-backend

# Start all services
docker-compose up -d

# Wait 30-60 seconds for services to start

# Verify all services are running
docker-compose ps
```

You should see all services with status "Up":
- admin_service
- catalog_service
- rule_service
- file_service
- configuration_service
- bom_service

### 3. Access Swagger UI

Open your browser and navigate to:

- **Admin Service**: http://localhost:5001/swagger
- **Catalog Service**: http://localhost:5002/swagger
- **Rule Service**: http://localhost:5000/swagger
- **File Service**: http://localhost:5003/swagger
- **Configuration Service**: http://localhost:5004/swagger
- **BOM Service**: http://localhost:5005/swagger

## Environment Setup

### Option 1: Docker (Recommended for Testers)

**Advantages**:
- ✅ No .NET installation required
- ✅ All services start with one command
- ✅ Isolated environment
- ✅ Easy cleanup

**Steps**:

1. Install Docker Desktop
2. Clone the repository
3. Run `docker-compose up -d`
4. Access Swagger UI

### Option 2: Local .NET (For Advanced Testing)

**Requirements**:
- .NET 10 SDK
- PostgreSQL 13+

**Steps**:

```powershell
# Install .NET 10 SDK
# Download from: https://dotnet.microsoft.com/download/dotnet/10.0

# Install PostgreSQL
# Download from: https://www.postgresql.org/download/

# Create databases
psql -U postgres -c "CREATE DATABASE admin_service;"
psql -U postgres -c "CREATE DATABASE catalog_service;"
psql -U postgres -c "CREATE DATABASE rule_service;"

# Run a service
cd Services\admin-service
dotnet run

# Access at http://localhost:5001/swagger
```

## Running Services

### Start All Services

```powershell
docker-compose up -d
```

### Start Specific Service

```powershell
docker-compose up -d admin_service
```

### View Logs

```powershell
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f admin_service
```

### Stop Services

```powershell
# Stop all
docker-compose down

# Stop specific service
docker-compose stop admin_service
```

### Restart Services

```powershell
# Restart all
docker-compose restart

# Restart specific service
docker-compose restart admin_service
```

## API Testing

### Using Swagger UI (Recommended)

Swagger UI provides an interactive interface for testing APIs.

#### 1. Navigate to Swagger UI

Example: http://localhost:5001/swagger

#### 2. Explore Endpoints

- Endpoints are grouped by controller
- Click on an endpoint to expand details
- View request/response schemas

#### 3. Test an Endpoint

**Example: Create a User**

1. Navigate to http://localhost:5001/swagger
2. Find `POST /api/v1/users`
3. Click "Try it out"
4. Enter request body:
   ```json
   {
     "email": "test@example.com",
     "displayName": "Test User",
     "role": "DESIGNER"
   }
   ```
5. Click "Execute"
6. View response:
   - **201 Created**: Success, returns user ID
   - **400 Bad Request**: Validation error
   - **500 Internal Server Error**: Server error

#### 4. Use Response Data

Copy the user ID from the response to use in other requests.

**Example: Get User by ID**

1. Find `GET /api/v1/users/{id}`
2. Click "Try it out"
3. Paste the user ID
4. Click "Execute"
5. View user details

### Using Postman

#### Import Collection

1. Open Postman
2. Import → Upload Files
3. Select `postman/GSS-Backend.postman_collection.json` (if available)

#### Create Manual Request

1. New Request
2. Set method (GET, POST, PUT, DELETE)
3. Enter URL: `http://localhost:5001/api/v1/users`
4. Add headers:
   - `Content-Type: application/json`
5. Add body (for POST/PUT):
   ```json
   {
     "email": "test@example.com",
     "displayName": "Test User"
   }
   ```
6. Send

### Using cURL

```powershell
# Create user
curl -X POST http://localhost:5001/api/v1/users `
  -H "Content-Type: application/json" `
  -d '{\"email\":\"test@example.com\",\"displayName\":\"Test User\"}'

# Get user
curl http://localhost:5001/api/v1/users/{user-id}

# Get all users
curl http://localhost:5001/api/v1/users
```

## Test Scenarios

### Admin Service

#### Scenario 1: User Registration Flow

1. **Create User** - `POST /api/v1/users`
   ```json
   {
     "email": "john.doe@example.com",
     "displayName": "John Doe",
     "role": "DESIGNER"
   }
   ```
   - Expected: 201 Created, returns user ID

2. **Get User** - `GET /api/v1/users/{id}`
   - Expected: 200 OK, returns user details
   - Verify: Status is "PENDING"

3. **Activate User** - `POST /api/v1/users/{id}/activate`
   - Expected: 200 OK

4. **Verify Activation** - `GET /api/v1/users/{id}`
   - Expected: Status is "ACTIVE"

#### Scenario 2: Validation Testing

1. **Invalid Email** - `POST /api/v1/users`
   ```json
   {
     "email": "invalid-email",
     "displayName": "Test User"
   }
   ```
   - Expected: 400 Bad Request
   - Error: "Invalid email format"

2. **Empty Display Name** - `POST /api/v1/users`
   ```json
   {
     "email": "test@example.com",
     "displayName": ""
   }
   ```
   - Expected: 400 Bad Request
   - Error: "Display name is required"

3. **Duplicate Email** - Create user with same email twice
   - Expected: 409 Conflict (or 400 Bad Request)

### Catalog Service

#### Scenario 1: SKU Management

1. **Create SKU** - `POST /api/v1/sku`
   ```json
   {
     "code": "SKU-001",
     "name": "Warehouse Shelf",
     "glbFile": "shelf-001.glb"
   }
   ```
   - Expected: 201 Created

2. **Get All SKUs** - `GET /api/v1/sku`
   - Expected: 200 OK, returns array of SKUs

3. **Update SKU** - `PUT /api/v1/sku/{id}`
   ```json
   {
     "name": "Updated Shelf Name"
   }
   ```
   - Expected: 200 OK

4. **Delete SKU** - `DELETE /api/v1/sku/{id}`
   - Expected: 204 No Content

#### Scenario 2: Pallet Management

1. **Create Pallet** - `POST /api/v1/pallet`
   ```json
   {
     "code": "PALLET-001",
     "name": "Standard Pallet",
     "palletType": "EURO"
   }
   ```
   - Expected: 201 Created

2. **Get Pallet** - `GET /api/v1/pallet/{id}`
   - Expected: 200 OK

### Rule Service

#### Scenario 1: RuleSet Creation

1. **Create RuleSet** - `POST /api/v1/ruleset`
   ```json
   {
     "name": "US Warehouse Rules",
     "productGroupId": "guid-here",
     "countryId": "guid-here",
     "effectiveFrom": "2026-01-01T00:00:00Z"
   }
   ```
   - Expected: 201 Created

2. **Activate RuleSet** - `POST /api/v1/ruleset/{id}/activate`
   - Expected: 200 OK

3. **Validate RuleSet** - `POST /api/v1/ruleset/{id}/validate`
   - Expected: 200 OK, returns validation result

### File Service (When Available)

#### Scenario 1: File Upload

1. **Upload GLB File** - `POST /api/v1/files/upload`
   - Upload a .glb file
   - Expected: 201 Created, returns file ID

2. **Get File Metadata** - `GET /api/v1/files/{id}`
   - Expected: 200 OK, returns file details

3. **Download File** - `GET /api/v1/files/{id}/download`
   - Expected: 200 OK, file download

### Configuration Service (When Available)

#### Scenario 1: Configuration Management

1. **Create Configuration** - `POST /api/v1/configurations`
   - Expected: 201 Created

2. **Get Configuration** - `GET /api/v1/configurations/{id}`
   - Expected: 200 OK

3. **Create Snapshot** - `POST /api/v1/configurations/{id}/snapshot`
   - Expected: 201 Created

### BOM Service (When Available)

#### Scenario 1: BOM Generation

1. **Generate BOM** - `POST /api/v1/bom/generate`
   - Expected: 200 OK, returns BOM data

2. **Export BOM** - `GET /api/v1/bom/{id}/export`
   - Expected: 200 OK, file download

## Test Data

### Sample Users

```json
[
  {
    "email": "admin@gss.com",
    "displayName": "System Admin",
    "role": "SUPER_ADMIN"
  },
  {
    "email": "dealer@gss.com",
    "displayName": "Dealer User",
    "role": "DEALER"
  },
  {
    "email": "designer@gss.com",
    "displayName": "Designer User",
    "role": "DESIGNER"
  }
]
```

### Sample SKUs

```json
[
  {
    "code": "SHELF-001",
    "name": "Standard Shelf 2m",
    "glbFile": "shelf-2m.glb"
  },
  {
    "code": "RACK-001",
    "name": "Heavy Duty Rack",
    "glbFile": "rack-hd.glb"
  }
]
```

## Health Checks

Verify services are running:

```powershell
# Admin Service
curl http://localhost:5001/health

# Catalog Service
curl http://localhost:5002/health

# Rule Service
curl http://localhost:5000/health
```

Expected response: `200 OK` with status "Healthy"

## Common Issues

### Service Won't Start

**Problem**: Docker container exits immediately

**Solution**:
```powershell
# Check logs
docker-compose logs admin_service

# Common causes:
# 1. Port already in use
# 2. Database connection failed
# 3. Migration error

# Restart with fresh database
docker-compose down -v
docker-compose up -d
```

### Can't Access Swagger UI

**Problem**: Browser shows "Connection refused"

**Solution**:
```powershell
# Verify service is running
docker-compose ps

# Check if port is correct
# Admin: 5001, Catalog: 5002, Rule: 5000

# Restart service
docker-compose restart admin_service
```

### 500 Internal Server Error

**Problem**: API returns 500 error

**Solution**:
```powershell
# Check service logs
docker-compose logs -f admin_service

# Look for error messages
# Common causes:
# 1. Database connection issue
# 2. Missing migration
# 3. Invalid data

# Restart service
docker-compose restart admin_service
```

### Database Connection Error

**Problem**: Service can't connect to database

**Solution**:
```powershell
# Verify PostgreSQL is running
docker-compose ps postgres

# Restart database
docker-compose restart postgres

# Restart service
docker-compose restart admin_service
```

## Tips for Effective Testing

### 1. Test Happy Path First
- Start with valid data
- Verify basic CRUD operations work
- Build confidence in the system

### 2. Test Edge Cases
- Empty strings
- Very long strings
- Special characters
- Null values
- Boundary values

### 3. Test Error Handling
- Invalid data formats
- Missing required fields
- Duplicate entries
- Non-existent IDs

### 4. Test Workflows
- Complete user journeys
- Multiple related operations
- State transitions

### 5. Document Issues
When you find a bug:
1. Note the endpoint and request
2. Save the request body
3. Capture the error response
4. Note steps to reproduce
5. Create an issue in the repository

## Reporting Bugs

### Bug Report Template

```markdown
**Service**: Admin Service
**Endpoint**: POST /api/v1/users
**Expected**: 201 Created
**Actual**: 500 Internal Server Error

**Request**:
{
  "email": "test@example.com",
  "displayName": "Test User"
}

**Response**:
{
  "error": "Internal server error"
}

**Steps to Reproduce**:
1. Navigate to http://localhost:5001/swagger
2. POST /api/v1/users
3. Use request body above
4. Click Execute

**Logs**:
[Paste relevant logs from docker-compose logs]
```

## Resources

- **Swagger Documentation**: Available at each service's `/swagger` endpoint
- **API Schemas**: View request/response schemas in Swagger UI
- **Service READMEs**: Detailed documentation in each service directory
- **Architecture Docs**: See `docs/` directory

## Getting Help

- **Questions**: Ask the development team
- **Issues**: Create an issue in the repository
- **Documentation**: Check service-specific READMEs

---

Happy Testing! 🧪
