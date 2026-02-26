# Validation & Limits

## Purpose
To protect the system from abuse and ensure data integrity by enforcing constraints on uploaded files.

## Configuration
Validation rules are configurable via `appsettings.json`.

```json
"FileValidation": {
  "MaxFileSizeMb": 50,
  "AllowedExtensions": [".dxf", ".pdf", ".xlsx", ".png", ".jpg"],
  "MagicNumberCheck": true
}
```

## Rules
- **File Size:** Reject files larger than the configured limit.
- **File Type:**
    - verify extension.
    - Verify MIME type.
    - (Optional) Check magic numbers for stricter validation.
- **Filename:** Sanitize filenames to prevent path traversal or special character issues.
