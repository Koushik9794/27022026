# Backend - DXF Metadata Extractor API

Python FastAPI backend for parsing and extracting metadata from DXF files.

## Setup

```bash
# Create virtual environment
python -m venv venv
source venv/bin/activate  # On Windows: venv\Scripts\activate

# Install dependencies
pip install -r requirements.txt

# Run the server
uvicorn app.main:app --reload --host 0.0.0.0 --port 8000
```

## API Endpoints

- `POST /api/dxf/upload` - Upload and parse DXF file
- `GET /api/health` - Health check

## Technology Stack

- **FastAPI** - Modern Python web framework
- **ezdxf** - DXF file parsing library
- **Pydantic** - Data validation
