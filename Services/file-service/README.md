# File Service

File import/export microservice for GSS Warehouse Configurator.

## Overview

The File Service handles all file operations for the warehouse configuration system, including GLB model uploads, Excel/CSV imports, and configuration exports.

### Key Features

- **File Upload/Download**: Secure file storage and retrieval
- **GLB Model Management**: 3D model file handling for warehouse components
- **Excel/CSV Import**: Bulk data import from spreadsheets
- **Configuration Export**: Export warehouse configurations in various formats
- **File Validation**: Validate file types, sizes, and content
- **Storage Integration**: S3-compatible storage (AWS S3, etc.)

## Architecture

### Domain-Driven Design (DDD)

```
file-service/
├── FileService.csproj          # Project file
├── Program.cs                  # Entry point
├── README.md                   # This file
├── Dockerfile                  # Container configuration
├── docker-compose.yml          # Standalone development
├── .env.example                # Environment template
├── appsettings.json            # Configuration
├── src/
│   ├── api/                   # REST controllers
│   ├── application/           # CQRS layer
│   │   ├── commands/          # File operations (Upload, Delete)
│   │   ├── queries/           # File retrieval
│   │   ├── handlers/          # Command/Query handlers
│   │   └── dtos/              # Data transfer objects
│   ├── domain/                # Core business logic
│   │   ├── aggregates/        # File, FileMetadata aggregates
│   │   ├── valueobjects/      # FileType, FileSize, StoragePath
│   │   └── repositories/      # Repository interfaces
│   └── infrastructure/        # Technical concerns
│       ├── persistence/       # File metadata repository
│       ├── storage/           # S3 storage adapter
│       └── migrations/        # Database migrations
└── tests/                     # Unit and integration tests
```

## Getting Started

### Prerequisites

- .NET 10+
- PostgreSQL 13+ (for file metadata)
- S3-compatible storage (AWS S3, MinIO, or local filesystem)
- Docker (optional)

### Environment Setup

1. **Copy environment template:**
```bash
cp .env.example .env
```

2. **Update `.env` for your environment:**
```env
DB_HOST=localhost
DB_PORT=5432
DB_NAME=file_service
DB_USER=postgres
DB_PASSWORD=postgres
CONNECTION_STRING=Server=localhost;Database=file_service;User Id=postgres;Password=postgres;Port=5432;
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:5003

# Storage Configuration
STORAGE_TYPE=S3  # Options: S3, MinIO, FileSystem
S3_BUCKET_NAME=gss-files
S3_REGION=us-east-1
AWS_ACCESS_KEY_ID=your-access-key
AWS_SECRET_ACCESS_KEY=your-secret-key

# File Upload Limits
MAX_FILE_SIZE_MB=100
ALLOWED_FILE_TYPES=.glb,.xlsx,.csv,.json
```

### Local Development

#### Option 1: Docker Compose (Recommended)

```bash
# Start PostgreSQL, MinIO, and File Service
docker-compose up -d

# Verify services are running
docker-compose ps

# Access the API
# Swagger UI: http://localhost:5003/swagger
# MinIO Console: http://localhost:9001 (admin/password)

# Stop services
docker-compose down
```

#### Option 2: Local .NET CLI

```bash
# Start PostgreSQL and MinIO (or use AWS S3)
# ...

# Build
dotnet build

# Run
dotnet run

# App available at http://localhost:5003
# Swagger at http://localhost:5003/swagger
```

## API Endpoints

### Swagger UI Documentation

```
http://localhost:5003/swagger
```

### Health Check

```bash
curl http://localhost:5003/health
```

### File Operations

- `POST /api/v1/files/upload` - Upload a file
- `GET /api/v1/files/{id}` - Get file metadata
- `GET /api/v1/files/{id}/download` - Download file
- `DELETE /api/v1/files/{id}` - Delete file
- `GET /api/v1/files` - List files with pagination

### GLB Model Operations

- `POST /api/v1/files/glb/upload` - Upload GLB model
- `GET /api/v1/files/glb/{id}` - Get GLB model metadata
- `GET /api/v1/files/glb` - List all GLB models

### Import/Export Operations

- `POST /api/v1/import/excel` - Import data from Excel
- `POST /api/v1/import/csv` - Import data from CSV
- `GET /api/v1/export/configuration/{id}` - Export configuration

## Domain Models

### File (Aggregate Root)

```csharp
public class File
{
    public Guid Id { get; }
    public FileName Name { get; }
    public FileType Type { get; }
    public FileSize Size { get; }
    public StoragePath Path { get; }
    public DateTime UploadedAt { get; }
    public Guid UploadedBy { get; }
    public FileStatus Status { get; }
}
```

### Value Objects

- **FileName**: Validated file name
- **FileType**: File extension and MIME type
- **FileSize**: Size in bytes with validation
- **StoragePath**: S3 key or file system path

## Configuration

### Storage Options

#### AWS S3

```env
STORAGE_TYPE=S3
S3_BUCKET_NAME=gss-files
S3_REGION=us-east-1
AWS_ACCESS_KEY_ID=your-key
AWS_SECRET_ACCESS_KEY=your-secret
```

#### MinIO (Local Development)

```env
STORAGE_TYPE=MinIO
S3_BUCKET_NAME=gss-files
S3_ENDPOINT=http://localhost:9000
AWS_ACCESS_KEY_ID=minioadmin
AWS_SECRET_ACCESS_KEY=minioadmin
```

#### File System (Testing)

```env
STORAGE_TYPE=FileSystem
FILE_STORAGE_PATH=C:\temp\gss-files
```

### File Upload Limits

```env
MAX_FILE_SIZE_MB=100
ALLOWED_FILE_TYPES=.glb,.xlsx,.csv,.json,.pdf
```

## Testing

```bash
# Run all tests
dotnet test

# Run unit tests
dotnet test --filter Category=Unit

# Run integration tests (requires MinIO/S3)
dotnet test --filter Category=Integration
```

## Contributing

1. Follow DDD principles
2. Validate file types and sizes
3. Handle storage errors gracefully
4. Write tests for file operations
5. Document API endpoints

## Production Readiness

Before deploying to production, complete the [Service Design Checklist](../../docs/service-design-checklist.md).

Key requirements:
- Health checks with storage connectivity validation
- Authentication for all file operations
- File type and size validation
- Virus scanning for uploaded files
- Secure storage with encryption at rest
- Audit logging for all file operations

## Status

🚧 **In Development** - Service scaffolding complete, implementation in progress

## License

Proprietary - GSS
