# File Service Documentation

This directory contains comprehensive documentation for the File Service.

## Quick Links

| Section | Purpose | Start Here |
|---------|---------|------------|
| [00-overview](./00-overview/) | Problem, goals, scope | [problem-statement.md](./00-overview/problem-statement.md) |
| [01-architecture](./01-architecture/) | System design, components | [context-diagram.md](./01-architecture/context-diagram.md) |
| [02-dxf-import](./02-dxf-import/) | DXF parsing & Civil layout | [README.md](./02-dxf-import/README.md) |
| [03-output-generation](./03-output-generation/) | Excel & PDF generation | [README.md](./03-output-generation/README.md) |
| [04-storage](./04-storage/) | S3 integration & storage | [s3-integration.md](./04-storage/s3-integration.md) |
| [05-configuration](./05-configuration/) | Validation & limits | [validation-rules.md](./05-configuration/validation-rules.md) |

## Core Concepts

### File Handling
The service acts as a central hub for all file interactions, ensuring consistent validation, storage abstraction, and processing logic.

### Structural Flow
1. **Upload**: Client uploads file -> Validator checks rules -> File stored in S3 (temp or perm).
2. **Processing**: Event triggered -> Worker downloads file -> Parses/Processes -> Updates domain data.
3. **Generation**: Request received -> Generator fetches data -> Builds document -> Stores in S3 -> Returns link.
