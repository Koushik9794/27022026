"""
Pydantic models for DXF data structures
"""
from typing import Optional, Any
from pydantic import BaseModel, Field


class LayerProperties(BaseModel):
    """Complete layer properties from DXF file"""
    
    name: str = Field(..., description="Layer name")
    color: int = Field(..., description="ACI color index (0-256)")
    rgb_color: Optional[str] = Field(None, description="RGB color as hex string")
    true_color: Optional[int] = Field(None, description="True color value")
    linetype: str = Field(default="Continuous", description="Line type name")
    lineweight: int = Field(default=-1, description="Line weight in 1/100 mm")
    plot: bool = Field(default=True, description="Whether layer is plotted")
    is_frozen: bool = Field(default=False, description="Layer frozen status")
    is_locked: bool = Field(default=False, description="Layer locked status")
    is_off: bool = Field(default=False, description="Layer visibility off")
    is_on: bool = Field(default=True, description="Layer visibility on")
    transparency: float = Field(default=0.0, description="Layer transparency (0-1)")
    description: Optional[str] = Field(None, description="Layer description")
    material: Optional[str] = Field(None, description="Material name")
    plot_style_name: Optional[str] = Field(None, description="Plot style name")
    entity_count: int = Field(default=0, description="Number of entities on this layer")


class EntityInfo(BaseModel):
    """Information about a DXF entity"""
    
    entity_type: str = Field(..., description="Type of entity (LINE, CIRCLE, etc.)")
    layer: str = Field(..., description="Layer name")
    color: Optional[int] = Field(None, description="Entity color")
    handle: Optional[str] = Field(None, description="Entity handle (may be None for virtual entities)")
    lineweight: int = Field(default=-1, description="Line weight in 1/100 mm")
    linetype: str = Field(default="BYLAYER", description="Linetype name")
    is_virtual: bool = Field(default=False, description="Whether this is a virtual entity from a block")
    # Geometry data for rendering
    geometry: dict = Field(default_factory=dict, description="Geometry coordinates")
    # Bounding box for exact replication
    bbox: Optional[dict] = Field(None, description="Precise bounding box {min_x, min_y, max_x, max_y}")
    # Optional metadata for dimensions
    dimension_text: Optional[str] = Field(None, description="Actual text for dimension entities")
    dimension_type: Optional[str] = Field(None, description="Specific type of dimension")


class BlockInfo(BaseModel):
    """Information about a DXF block"""
    
    name: str = Field(..., description="Block name")
    base_point: dict = Field(default_factory=dict, description="Block base point")
    entity_count: int = Field(default=0, description="Number of entities in block")
    is_anonymous: bool = Field(default=False, description="Whether block is anonymous")
    is_xref: bool = Field(default=False, description="Whether block is external reference")
    entities: list[EntityInfo] = Field(default_factory=list, description="Entities within the block")



class LinetypeInfo(BaseModel):
    """Information about a DXF linetype"""
    
    name: str = Field(..., description="Linetype name")
    description: Optional[str] = Field(None, description="Linetype description")
    pattern_length: float = Field(default=0.0, description="Total pattern length")


class FileMetadata(BaseModel):
    """DXF file metadata"""
    
    filename: str = Field(..., description="Original filename")
    dxf_version: str = Field(..., description="DXF version (e.g., AC1027)")
    dxf_version_name: str = Field(..., description="Human readable version name")
    created_by: Optional[str] = Field(None, description="Application that created the file")
    creation_date: Optional[str] = Field(None, description="File creation date")
    update_date: Optional[str] = Field(None, description="Last update date")
    file_size: int = Field(..., description="File size in bytes")
    file_size_formatted: str = Field(..., description="Human readable file size")
    units: Optional[str] = Field(None, description="Drawing units")
    

class ExtentsInfo(BaseModel):
    """Drawing extents information"""
    
    min_x: float = Field(..., description="Minimum X coordinate")
    min_y: float = Field(..., description="Minimum Y coordinate")
    max_x: float = Field(..., description="Maximum X coordinate")
    max_y: float = Field(..., description="Maximum Y coordinate")
    width: float = Field(..., description="Drawing width")
    height: float = Field(..., description="Drawing height")


class DXFParseResponse(BaseModel):
    """Complete response from DXF parsing"""
    
    success: bool = Field(default=True, description="Whether parsing was successful")
    message: str = Field(default="File parsed successfully", description="Status message")
    
    # File metadata
    metadata: FileMetadata
    
    # Drawing information
    extents: Optional[ExtentsInfo] = Field(None, description="Drawing extents")
    
    # Layer information
    total_layers: int = Field(..., description="Total number of layers")
    layers: list[LayerProperties] = Field(default_factory=list, description="All layers")
    
    # Entity information
    total_entities: int = Field(..., description="Total number of entities")
    entity_types: dict[str, int] = Field(default_factory=dict, description="Entity type counts")
    entities: list[EntityInfo] = Field(default_factory=list, description="All entities for rendering")
    
    # Additional info
    total_blocks: int = Field(default=0, description="Total number of blocks")
    blocks: list[BlockInfo] = Field(default_factory=list, description="Block definitions")
    linetypes: list[LinetypeInfo] = Field(default_factory=list, description="Linetype definitions")


class ErrorResponse(BaseModel):
    """Error response model"""
    
    success: bool = Field(default=False)
    message: str = Field(..., description="Error message")
    detail: Optional[str] = Field(None, description="Detailed error information")
