"""
DXF API Routes - File upload and parsing endpoints
"""
from fastapi import APIRouter, UploadFile, File, HTTPException, Response
from fastapi.responses import JSONResponse

from app.config import settings
from app.models.dxf_models import DXFParseResponse, ErrorResponse
from app.services.dxf_parser import parse_dxf_file

router = APIRouter(prefix="/api/dxf", tags=["DXF"])


async def _process_dxf_upload(file: UploadFile) -> DXFParseResponse:
    """Helper to process DXF upload and return parsing results."""
    # Validate file extension
    if not file.filename:
        raise HTTPException(status_code=400, detail="No filename provided")
    
    file_ext = "." + file.filename.split(".")[-1].lower()
    if file_ext not in settings.ALLOWED_EXTENSIONS:
        raise HTTPException(
            status_code=400,
            detail=f"Invalid file type. Allowed types: {settings.ALLOWED_EXTENSIONS}"
        )
    
    # Read file content
    try:
        content = await file.read()
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Error reading file: {str(e)}")
    
    # Check file size
    if len(content) > settings.MAX_FILE_SIZE:
        raise HTTPException(
            status_code=413,
            detail=f"File too large. Maximum size: {settings.MAX_FILE_SIZE / (1024*1024):.0f}MB"
        )
    
    # Parse DXF file
    try:
        result = await parse_dxf_file(content, file.filename)
        return result
    except Exception as e:
        raise HTTPException(
            status_code=500,
            detail=f"Error parsing DXF file: {str(e)}"
        )


@router.post(
    "/upload",
    response_model=DXFParseResponse,
    responses={
        400: {"model": ErrorResponse, "description": "Invalid file"},
        413: {"model": ErrorResponse, "description": "File too large"},
        500: {"model": ErrorResponse, "description": "Server error"},
    },
)
async def upload_dxf(file: UploadFile = File(...)):
    """
    Upload a DXF file and extract all metadata, layers, and entities.
    
    - **file**: DXF file to parse (max 100MB)
    
    Returns complete metadata including layers, entities, blocks, and linetypes.
    """
    return await _process_dxf_upload(file)


@router.post(
    "/download",
    responses={
        400: {"model": ErrorResponse, "description": "Invalid file"},
        413: {"model": ErrorResponse, "description": "File too large"},
        500: {"model": ErrorResponse, "description": "Server error"},
    },
)
async def download_dxf_json(file: UploadFile = File(...)):
    """
    Upload a DXF file and download the extracted metadata as a JSON file.
    
    - **file**: DXF file to parse (max 100MB)
    
    Returns a downloadable JSON file containing the DXF metadata.
    """
    result = await _process_dxf_upload(file)
    
    # Convert result to JSON
    json_data = result.model_dump_json(indent=2)
    
    # Generate download filename
    original_name = file.filename or "layout.dxf"
    download_filename = f"{original_name.rsplit('.', 1)[0]}.json"
    
    return Response(
        content=json_data,
        media_type="application/json",
        headers={
            "Content-Disposition": f"attachment; filename={download_filename}",
            "Access-Control-Expose-Headers": "Content-Disposition"
        }
    )


@router.get("/health")
async def health_check():
    """Health check endpoint"""
    return {"status": "healthy", "service": "DXF Parser API"}
