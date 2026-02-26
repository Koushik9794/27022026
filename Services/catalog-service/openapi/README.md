# OpenAPI Specification

This directory contains the API specification for the Catalog Service.

## Files

| File | Purpose |
|------|---------|
| `api.yaml` | OpenAPI 3.0 specification (reference documentation) |

## How It Works

- **Swagger UI** is auto-generated from controllers at runtime
- **api.yaml** serves as reference documentation and can be used for:
  - Sharing API contracts with frontend teams
  - SDK generation if needed
  - API gateway configuration

## Accessing the Live API

| Environment | Swagger UI |
|-------------|------------|
| Local | http://localhost:60188/swagger |
| Docker | http://localhost:5002/swagger |

## Updating the Specification

When you add new endpoints, the Swagger UI updates automatically. 
Optionally update `api.yaml` to keep it in sync for documentation purposes.
