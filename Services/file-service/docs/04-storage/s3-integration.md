# Storage & S3 Integration

## Strategy
The service uses a provider pattern to abstract the underlying storage mechanism.
- **Local Development:** Uses local file system.
- **Production:** Uses AWS S3.

## S3 Implementation Details
- **Bucket Organization:** Folders by `TenantId` / `ProjectId` / `CategoryId`.
- **Pre-signed URLs:** Use pre-signed URLs for secure upload/download directly from the client to S3, reducing load on the service for large files.
- **Lifecycle Policies:** Configure S3 lifecycle policies for temporary files (e.g., generated reports that are only needed for a short time).
