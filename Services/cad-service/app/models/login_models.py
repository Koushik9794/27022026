from __future__ import annotations

from datetime import datetime, timezone
from typing import Optional
from uuid import UUID, uuid4
from urllib.parse import urlparse
import os

from pydantic import BaseModel, Field, HttpUrl


# ---------- Request model (client should NOT send name/role/login_time/file_name) ----------
class LoginInfoRequest(BaseModel):
    enquiry_id: str = Field(..., min_length=1, description="Business enquiry id")
    s3_url_path_dxf: HttpUrl = Field(..., description="HTTPS URL to the DXF file")
    s3_url_path_json: HttpUrl = Field(..., description="HTTPS URL to the JSON file")
    upload_status: Optional[str] = Field(default="uploaded", description="Upload status")
    warehouse_type: Optional[str] = Field(default="Gable", description="Warehouse type")
    total_layers: Optional[int] = Field(default=None, ge=0, description="Total DXF layers")
    total_entities: Optional[int] = Field(default=None, ge=0, description="Total DXF entities")
    file_size: Optional[str] = Field(default=None, description="e.g., '60mb'")
    dxf_version: Optional[str] = Field(default="AutoCAD 2018", description="DXF version")

    class Config:
        json_schema_extra = {
            "example": {
                "enquiry_id": "ENQ-2026-0001",
                "s3_url_path_dxf": "https://s3.amazonaws.com/my-bucket/designs/sample_1.dxf",
                "s3_url_path_json": "https://s3.amazonaws.com/my-bucket/designs/sample_1.json",
                "upload_status": "uploaded",
                "warehouse_type": "Gable",
                "total_layers": 8,
                "total_entities": 542,
                "file_size": "60mb",
                "dxf_version": "AutoCAD 2018"
            }
        }


# ---------- Response model (contains server-derived/user-context fields) ----------
class LoginInfoResponse(BaseModel):
    id: UUID
    name: str
    role: str
    enquiry_id: str
    login_time: str  # formatted with trailing 'Z'
    file_name: str
    s3_url_path_dxf: HttpUrl
    s3_url_path_json: HttpUrl
    revision_id: UUID
    status: str
    created_at: str  # formatted with trailing 'Z'
    updated_at: str  # formatted with trailing 'Z'
    warehouse_type: str
    total_layers: int
    total_entities: int
    file_size: Optional[str] = None
    dxf_version: str
    upload_status: str

    class Config:
        json_schema_extra = {
            "example": {
                "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
                "name": "Varsha",
                "role": "Backend Engineer",
                "enquiry_id": "ENQ-2026-0001",
                "login_time": "2026-01-14T10:29:04.549000Z",
                "file_name": "sample_1.dxf",
                "s3_url_path_dxf": "https://s3.amazonaws.com/my-bucket/designs/part.dxf",
                "s3_url_path_json": "https://s3.amazonaws.com/my-bucket/designs/part.json",
                "revision_id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
                "status": "active",
                "created_at": "2026-01-14T10:30:42.602220Z",
                "updated_at": "2026-01-14T10:30:42.602220Z",
                "warehouse_type": "Gable",
                "total_layers": 8,
                "total_entities": 542,
                "file_size": "60mb",
                "dxf_version": "AutoCAD 2018",
                "upload_status": "uploaded"
            }
        }


# ---------- helpers ----------
def _z(dt: datetime) -> str:
    """Format a datetime as strict UTC Z-suffix string."""
    if dt.tzinfo is None:
        dt = dt.replace(tzinfo=timezone.utc)
    return dt.astimezone(timezone.utc).isoformat().replace("+00:00", "Z")


def _file_name_from_url(url: str) -> str:
    path = urlparse(url).path
    base = os.path.basename(path)
    return base or "unknown.dxf"
